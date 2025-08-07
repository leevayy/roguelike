using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;
using utility;
using Plane = UnityEngine.Plane;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour, utility.IAliveEntity
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject debugPointer;
    [SerializeField] private Weapon weapon;
    [SerializeField] private Weapon hitscanWeapon;
    [SerializeField] private GameObject moneyBagPrefab;
    [SerializeField] private GameObject moneyBagAnchor;
    [SerializeField] private GameObject modificationPrefab;
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private AudioSource dashSound;
    [SerializeField] public AudioSource denyDamageSound;
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
    private Weapon _currentWeapon => weapon.enabled ? weapon : hitscanWeapon;
    
    public ComposableModificationManager modManager { get; private set; }
    
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

    // IAliveEntity implementation
    public bool IsAlive => Healthpoints > 0;
    public bool IsGrounded => _movementManager != null && Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1.1f);
    public float HealthPoints => Healthpoints;
    public float MaxHealthPoints => _maxHealthpoints;
    public Vector3 Position => transform.position;
    public Transform Transform => transform;
    public ComposableModificationManager ModManager => modManager;

    public utility.AliveState GetAliveState()
    {
        return new utility.AliveState(
            IsAlive,
            IsGrounded,
            HealthPoints,
            MaxHealthPoints,
            Position,
            Transform,
            ModManager
        );
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _movementManager = GetComponent<MovementManager>();
        _characterAnimationController = GetComponent<CharacterAnimationController>();
        _characterAnimationController.Initialize(false);
        
        _ragdollController = GetComponent<RagdollController>();
        modManager = gameObject.AddComponent<ComposableModificationManager>();

        _movementManager.Initialize(modManager, () => GetAliveState());
    }
    
    private void Start()
    {
        MoneyBag = Instantiate(moneyBagPrefab, moneyBagAnchor.transform);
        
        hitbox.SetOnTriggerEnterHandler((other, _) =>
        {
            var isHitByEnemy = other.CompareTag("EnemyProjectile");
            if (!isHitByEnemy) return;

            var laser = other.gameObject.GetComponent<Laser>();
            var damage = GetDamage(laser.damage);

            // If damage was negated, don't trigger hit
            if (damage <= 0) return;

            var collisionPoint = other.GetComponent<Collider>().ClosestPoint(transform.position);
            GameManager.instance.OnHit(
                new HitInfo(GameHitEntity.Enemy, damage, laser.shotId),
                GameHitEntity.Ally,
                collisionPoint);
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
        modManager.ApplyOnUpdate(GetAliveState());

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
            var playerPosition = new Vector3(_rb.position.x, 0, _rb.position.z);
            
            var mouseOffset = (new Vector3(mouseWorldPosition.x, 0, mouseWorldPosition.z) - playerPosition).normalized * extentionLength;

            targetPosition = targetPosition + mouseOffset;
        }
        
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPosition, ref _cameraVelocity, 0.25f);
    }

    public void ChangeWeaponType()
    {
        if (weapon.enabled)
        {
            weapon.enabled = false;
            hitscanWeapon.enabled = true;
        }
        else
        {
            weapon.enabled = true;
            hitscanWeapon.enabled = false;
        }
    }

    public void SetMoneyBagWeight(float weight)
    {
        _movementManager.additionalSpeed = weight;
    }

    private Vector3 GetMouseWorldPosition()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y + 2f, 0f));

        return groundPlane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : Vector3.zero;
    }
    
    private float GetDamage(float damageIn)
    {
        var modifiedDamage = modManager.ModifyIncomingDamage(GetAliveState(), damageIn);

        Healthpoints -= modifiedDamage;

        if (modifiedDamage > 0)
        {
            modManager.ApplyOnTakeDamage(GetAliveState(), modifiedDamage);
        }

        return modifiedDamage;
    }
    
    [ContextMenu(nameof(FuckingInstantlyDie))] private float FuckingInstantlyDie()
    {
        return GetDamage(999999);
    }

    private void Shoot()
    {
        _currentWeapon.Shoot(GetAliveState(), transform.rotation, modManager.GetModifications());
        _characterAnimationController.FireAnimation();
    }

    private void AddModification(Modification modification, string name)
    {
        modManager.AddModification(modification);
        modManager.ApplyOnPickUp(GetAliveState());
        
        // Create visual representation of the modification
        if (modificationPrefab != null)
        {
            var modObject = Instantiate(modificationPrefab, _currentWeapon.transform);
            _modObjects.Add(modObject);

            var modificationObjectComponent = modObject.GetComponent<ModificationObject>();
            if (modificationObjectComponent != null)
            {
                modificationObjectComponent.Init(modification, _modObjects.Count - 1);
            }
        }
        
        GameUI.instance.UpdateMods(modManager.GetModifications().Count - 1, name);
    }

    public void RemoveModification(ModificationType modificationType)
    {
        modManager.RemoveAllModifiersOfType(modificationType);
        modManager.ApplyOnDrop(GetAliveState());
    }

    public void Heal(float part = 1)
    {
        var healing = _maxHealthpoints * part;
        
        var nextHealth = Mathf.Min(_maxHealthpoints, Healthpoints + healing);
        
        Healthpoints = nextHealth;
    }

    public bool BuyItem(StoreItem item)
    {
        if (item.type == StoreItemType.Skip)
        {
            return true;
        }

        if (modManager.GetModifications().Count >= 5 && item.type != StoreItemType.Reroll)
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
        return modManager.GetModifications();
    }

    public ReadOnlyCollection<Modification> DropModifications()
    {
        GameUI.instance.ClearMods();
        
        // Create a copy of the modifications to avoid issues with clearing the underlying list
        var mods = new List<Modification>(modManager.GetModifications());
        
        foreach (var modObject in _modObjects)
        {
            if (modObject != null)
            {
                Destroy(modObject);
            }
        }
        
        _modObjects.Clear();
        modManager.Clear();
        
        return mods.AsReadOnly();
    }
}