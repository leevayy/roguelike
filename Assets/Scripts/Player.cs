using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using utility;
using Plane = UnityEngine.Plane;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject debugPointer;
    [SerializeField] private Weapon weapon;
    [SerializeField] private GameObject modificationPrefab;
    [SerializeField] private GameObject moneyBagPrefab;
    [SerializeField] private GameObject moneyBagAnchor;
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private AudioSource dashSound;
    [SerializeField] private AudioSource denyDamageSound;
    [SerializeField] private float autoShotCooldown = 0.33f;
    private readonly Vector3 _cameraOffset = new(-15, 12, -15);
    
    private Rigidbody _rb;
    private bool _isExtendedLook;
    private Vector3 _cameraVelocity = Vector3.zero;
    
    private MovementManager _movementManager;
    private CharacterAnimationController _characterAnimationController;
    private RagdollController _ragdollController;
    
    private float _maxHealthpoints = 100f;
    private float _healthpoints = 100f;
    private float _sinceLastHit = 0f;
    private float _sinceLastAutoShot = 0f;
    private readonly List<GameObject> _modObjects = new();
    
    public int MoneySpent { get; private set; }
    public GameObject MoneyBag { get; private set; }
    public Action<float, float> OnHealthPointsChanged { private get; set; }
    public Action<Player> AfterStart;

    public float Healthpoints { 
        get => _healthpoints;
        private set
        {
            _healthpoints = value;
            
            GameUI.instance.UpdateHp((int)value, (int)_maxHealthpoints);
            
            OnHealthPointsChanged(value, _maxHealthpoints);
        } 
    }
    
    public Action<PlayerEvent<PlayerEventPayload>> OnDashEvent;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _movementManager = GetComponent<MovementManager>();
        _characterAnimationController = GetComponent<CharacterAnimationController>();
        _ragdollController = GetComponent<RagdollController>();
    }
    
    private void Start()
    {
        MoneyBag = Instantiate(moneyBagPrefab, moneyBagAnchor.transform);
        
        hitbox.SetOnTriggerEnterHandler((other, _) =>
        {
            var isHitByEnemy = other.CompareTag("EnemyProjectile");

            if (!isHitByEnemy) return;
            
            if (ModManager.instance.HasMod(ModificationType.InvulnerabilityOnHit) && _sinceLastHit < 1f)
            {
                denyDamageSound.Play();
                return;
            }
                
            _sinceLastHit = 0f;
                
            var collisionPoint = other.GetComponent<Collider>().ClosestPoint(transform.position);

            var laser = other.gameObject.GetComponent<Laser>();

            GameManager.instance.OnHit(
                ModManager.instance.HasMod(ModificationType.MoneyEqualsLife)
                    ? new HitInfo(GameHitEntity.Enemy, laser.damage, laser.shotId)
                    : new HitInfo(GameHitEntity.Enemy, GetDamage(laser.damage), laser.shotId),
                GameHitEntity.Ally, collisionPoint);

            for (var i = 0; i < ModManager.instance.CountMod(ModificationType.ReflectDamage); i++)
            {
                weapon.Shoot(transform.rotation, laser.damage * 5f);
            }
        });

        AfterStart?.Invoke(this);
    }
    
    private Vector3 GetRawMoveInput()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        
        return cam.transform.rotation * new Vector3(horizontalInput, 0, verticalInput).normalized;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            _ragdollController.Die();
        }
        
        var moveInput = GetRawMoveInput();
        
        _movementManager.Tick(moveInput);

        var movementDirection = transform.InverseTransformVector(_movementManager.MoveVector);
        
        _characterAnimationController.Tick(movementDirection);
        
        MoveCamera(moveInput);
        
        _sinceLastHit += Time.deltaTime;

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
        
        if (Input.GetButtonUp("Fire1"))
        {
            _sinceLastAutoShot = 0f;
        }

        if (Input.GetButton("Fire1"))
        {
            if (_sinceLastAutoShot >= autoShotCooldown)
            {
                _sinceLastAutoShot = 0f;
                Shoot();
            }
            else
            {
                _sinceLastAutoShot += Time.deltaTime;
            }
        }
    }

    private void FixedUpdate()
    {
        _movementManager.FixedTick(_rb);

        var mouseWorldPosition = GetMouseWorldPosition();

        if (debugPointer)
        {
            debugPointer.transform.position = mouseWorldPosition;
        }

        transform.LookAt(new Vector3(mouseWorldPosition.x, transform.position.y, mouseWorldPosition.z));
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        _movementManager.Jump(_rb, () =>
        {
            dashSound.pitch = 1 + Random.Range(0f, 1f) + _rb.mass / 10;
            dashSound.Play();
        });
    }
    
    public void OnExtendedLook(InputAction.CallbackContext context)
    {
        _isExtendedLook = context.phase == InputActionPhase.Performed;
    }
    
    private void MoveCamera(Vector3 moveInput)
    {
        if (SettingsManager.instance.cameraType == CameraMode.Static) return;
        
        var movementOffset = moveInput / 4 + _rb.position;

        var targetPosition = _cameraOffset + movementOffset;

        if (_isExtendedLook)
        {
            const float extentionLength = 10f;
            
            var mouseWorldPosition = GetMouseWorldPosition();
            
            var mouseOffset = new Vector3(mouseWorldPosition.x, 0, mouseWorldPosition.z).normalized * extentionLength;

            var nextTargetPosition = (targetPosition + _cameraOffset + mouseOffset) / 2;
            
            targetPosition = nextTargetPosition;
        }
        
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPosition, ref _cameraVelocity, 0.25f);
    }

    public void SetAdditionalSpeed(float additionalSpeed)
    {
        _movementManager.additionalSpeed = additionalSpeed;
    }

    private Vector3 GetMouseWorldPosition()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y + 1f, 0f));

        return groundPlane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : Vector3.zero;
    }
    
    private float GetDamage(float damageIn)
    {
        for (var i = 0; i < ModManager.instance.CountMod(ModificationType.DoubleDamageAndTaken); i++)
        {
            damageIn *= 2f;
        }
        
        Healthpoints -= damageIn;

        return damageIn;
    }
    
    [ContextMenu(nameof(FuckingInstantlyDie))] private float FuckingInstantlyDie()
    {
        return GetDamage(999999);
    }

    private void Shoot()
    {
        weapon.Shoot(transform.rotation);
        _characterAnimationController.FireAnimation();
    }

    private void AddModification(Modification modification, string name)
    {
        var mod = Instantiate(modificationPrefab, weapon.transform);

        _modObjects.Add(mod);

        var modObject = mod.GetComponent<ModificationObject>();

        GameUI.instance.UpdateMods(weapon.modifications.Count, name);

        weapon.AddModification(modObject, modification);
    }

    public void RemoveModification(ModificationType modificationType)
    {
        weapon.RemoveModificationsOfType(modificationType);
    }

    public void Heal(float part = 1)
    {
        var healing = _maxHealthpoints * part;
        
        if (ModManager.instance.HasMod(ModificationType.MoneyEqualsLife))
        {
            GameManager.instance.score += (int)healing;
        }
        
        var nextHealth = Mathf.Min(_maxHealthpoints, Healthpoints + healing);
        
        Healthpoints = nextHealth;
    }

    public bool BuyItem(StoreItem item)
    {
        if (item.type == StoreItemType.Skip)
        {
            return true;
        }

        if (weapon.modifications.Count >= 5 && item.type != StoreItemType.Reroll)
        {
            return false;
        }
        
        if (GameManager.instance.score >= item.price || item.price == 0)
        {
            GameManager.instance.score -= (int)item.price;
            MoneySpent += (int)item.price;
            
            item.Buy();

            if (item.type == StoreItemType.Modification)
            {
                AddModification(item.modification, item.name);
            }
            else
            {
                GameManager.instance.RerollShop();
            }

            return true;
        }

        return false;
    }

    public ReadOnlyCollection<Modification> GetModifications()
    {
        return weapon.modifications;
    }

    public ReadOnlyCollection<Modification> DropModifications()
    {
        GameUI.instance.ClearMods();
        
        foreach (var modObject in _modObjects)
        {
            Destroy(modObject);
        }
        
        _modObjects.Clear();

        return weapon.DropModifications();
    }
}