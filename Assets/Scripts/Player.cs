using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;
using utility;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject debugPointer;
    [SerializeField] private Weapon weapon;
    [SerializeField] private GameObject modificationPrefab;
    [SerializeField] private GameObject moneyBagPrefab;
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private AudioSource dashSound;
    [SerializeField] private AudioSource denyDamageSound;
    [SerializeField] private float autoShotCooldown = 0.33f;
    
    private const float Speed = 10f;
    private const float JumpForce = 2f;
    private const float DashMultiplier = 10f;
    private readonly Vector3 _cameraOffset = new(-15, 12, -15);

    public GameObject moneyBag { get; private set; }
    private Rigidbody _rb;
    private bool _isGrounded;
    private bool _isExtendedLook;
    private Vector3 _moveInput;
    private Vector3 _cameraVelocity = Vector3.zero;
    
    private float _maxHealthpoints = 100f;
    private float _healthpoints = 100f;
    private float _sinceLastHit = 0f;
    private float _sinceLastAutoShot = 0f;
    private List<GameObject> _modObjects = new();
    
    public int moneySpent { get; private set; }
    public Action<float, float> onHealthPointsChanged { private get; set; }
    
    public float healthpoints { 
        get => _healthpoints;
        private set
        {
            _healthpoints = value;
            
            GameUI.instance.UpdateHp((int)value, (int)_maxHealthpoints);
            
            onHealthPointsChanged(value, _maxHealthpoints);
        } 
    }

    public float additionalSpeed = 0f;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        var moneyBagOffset = new Vector3(0, 0.1f, -0.7f);
        var moneyBagRotation = Quaternion.Euler(0, -90, 0);
    
        moneyBag = Instantiate(moneyBagPrefab, transform.position + moneyBagOffset, moneyBagRotation, transform);
        
        hitbox.SetOnTriggerEnterHandler((other, hitbox1) =>
        {
            var isHitByEnemy = other.CompareTag("EnemyProjectile");
        
            if (isHitByEnemy)
            {
                if (ModManager.instance.HasMod(ModificationType.InvulnerabilityOnHit) && _sinceLastHit < 0.5f)
                {
                    denyDamageSound.Play();
                    return;
                }
                
                _sinceLastHit = 0f;
                
                var collisionPoint = other.GetComponent<Collider>().ClosestPoint(transform.position);

                var laser = other.gameObject.GetComponent<Laser>();
                
                var laserDamage = laser.damage;
                
                for (var i = 0; i < ModManager.instance.CountMod(ModificationType.DoubleDamageAndTaken); i++)
                {
                    laserDamage *= 2f;
                }

                GameManager.instance.OnHit(
                    ModManager.instance.HasMod(ModificationType.MoneyEqualsLife)
                        ? new HitInfo(GameHitEntity.Enemy, GetDamage(0), laser.shotId)
                        : new HitInfo(GameHitEntity.Enemy, GetDamage(laserDamage), laser.shotId),
                    GameHitEntity.Ally, collisionPoint);

                for (var i = 0; i < ModManager.instance.CountMod(ModificationType.ReflectDamage); i++)
                {
                    weapon.Shoot(transform.rotation, laserDamage * 5f);
                }
            }
        });
    }

    private void Update()
    {
        _sinceLastHit += Time.deltaTime;
        
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");

        var compoundSpeed = Speed + additionalSpeed;

        compoundSpeed += 5f * ModManager.instance.CountMod(ModificationType.MoveSpeedIncrease);
        
        _moveInput = cam.transform.rotation * new Vector3(horizontalInput, 0, verticalInput).normalized * compoundSpeed;

        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            dashSound.pitch = 1 + Random.Range(0f, 1f) + _rb.mass / 10;
            dashSound.Play();
            var jumpForce = new Vector3(_moveInput.x * DashMultiplier, JumpForce, _moveInput.z * DashMultiplier);
            _rb.AddForce(jumpForce, ForceMode.Impulse);
        }

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

        if (!ControlGuide.instance.isActive)
        {
            return;
        }

        if (_moveInput != Vector3.zero)
        {
            ControlGuide.instance.CompletedWasd();
        }
        
        if (Input.GetButtonDown("Fire1"))
        {
            ControlGuide.instance.CompletedRmb();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ControlGuide.instance.CompletedSpace();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            ControlGuide.instance.CompletedP();
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            ControlGuide.instance.CompletedG();
        }
    }

    private void FixedUpdate()
    {
        Move();
        
        var mouseWorldPosition = GetMouseWorldPosition();

        if (debugPointer)
        {
            debugPointer.transform.position = mouseWorldPosition;
        }

        transform.LookAt(new Vector3(mouseWorldPosition.x, transform.position.y, mouseWorldPosition.z));
    }

    public void Move()
    {
        _rb.linearVelocity = new Vector3(_moveInput.x, _rb.linearVelocity.y, _moveInput.z);
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!_isGrounded || !context.started) return;
        
        dashSound.pitch = 1 + Random.Range(0f, 1f) + _rb.mass / 10;
        dashSound.Play();
        var jumpForce = new Vector3(_moveInput.x * DashMultiplier, JumpForce, _moveInput.z * DashMultiplier);
        _rb.AddForce(jumpForce, ForceMode.Impulse);
    }
    
    public void OnExtendedLook(InputAction.CallbackContext context)
    {
        _isExtendedLook = context.phase == InputActionPhase.Performed;
    }
    
    private void LateUpdate()
    {
        if (SettingsManager.instance.cameraType == CameraMode.Static) return;
        
        var movementOffset = _moveInput / 4 + _rb.position;

        var targetPosition = _cameraOffset + movementOffset;

        if (_isExtendedLook)
        {
            const float extentionLength = 10f;
            
            var mouseWorldPosition = GetMouseWorldPosition();
            
            var mouseOffset = new Vector3(mouseWorldPosition.x, 0, mouseWorldPosition.z).normalized * extentionLength;

            var nextTargetPosition = (targetPosition + _cameraOffset + mouseOffset) / 2;
            
            Debug.DrawLine(targetPosition, nextTargetPosition, Color.red);

            targetPosition = nextTargetPosition;
        }
        
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPosition, ref _cameraVelocity, 0.25f);
    }

    private Vector3 GetMouseWorldPosition()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        return groundPlane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : Vector3.zero;
    }
    
    private float GetDamage(float damageIn)
    {
        healthpoints -= damageIn;

        return damageIn;
    }
    
    [ContextMenu(nameof(FuckingInstantlyDie))] private float FuckingInstantlyDie()
    {
        return GetDamage(999999);
    }

    private void Shoot()
    {
        weapon.Shoot(transform.rotation);
    }

    private void AddModification(Modification modification, string name)
    {
        var mod = Instantiate(modificationPrefab, weapon.transform);
        
        _modObjects.Add(mod);
        
        var modObject = mod.GetComponent<ModificationObject>();
        
        GameUI.instance.UpdateMods(weapon.modifications.Count, name);

        weapon.AddModification(modObject, modification);
    }

    public void Heal(float part = 1)
    {
        var healing = _maxHealthpoints * part;
        
        if (ModManager.instance.HasMod(ModificationType.MoneyEqualsLife))
        {
            GameManager.instance.score += (int)healing;
        }
        
        var nextHealth = Mathf.Min(_maxHealthpoints, healthpoints + healing);
        
        healthpoints = nextHealth;
    }

    public bool BuyItem(StoreItem item)
    {
        if (item.type == StoreItemType.Skip)
        {
            return true;
        }

        if (weapon.modifications.Count >= 5)
        {
            return false;
        }
        
        if (GameManager.instance.score >= item.price || item.price == 0)
        {
            GameManager.instance.score -= (int)item.price;
            moneySpent += (int)item.price;
            
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