using System.Collections.ObjectModel;
using UnityEngine;
using utility;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject debugPointer;
    [SerializeField] private Weapon weapon;
    [SerializeField] private GameObject modificationPrefab;
    [SerializeField] private GameObject moneyBagPrefab;
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private AudioSource dashSound;
    
    private const float Speed = 10f;
    private const float JumpForce = 2f;
    private const float DashMultiplier = 10f;
    private readonly Vector3 _cameraOffset = new(-15, 12, -15);

    public GameObject moneyBag { get; private set; }
    private Rigidbody _rb;
    private bool _isGrounded;
    private Vector3 _moveInput;
    private Vector3 _cameraVelocity = Vector3.zero;
    
    private float _maxHealthpoints = 100f;
    private float _healthpoints = 100f;
    
    public int moneySpent { get; private set; }

    public float healthpoints { 
        get => _healthpoints;
        private set
        {
            _healthpoints = value;
            
            GameUI.instance.UpdateHp((int)value, (int)_maxHealthpoints);
        } 
    }

    public float additionalSpeed = 0f;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    
        var moneyBagOffset = new Vector3(0, 0.1f, -0.7f);
        var moneyBagRotation = Quaternion.Euler(0, -90, 0);
    
        moneyBag = Instantiate(moneyBagPrefab, transform.position + moneyBagOffset, moneyBagRotation, transform);
        
        hitbox.SetOnTriggerEnterHandler(((other, hitbox1) =>
        {
            var isHitByEnemy = other.CompareTag("EnemyProjectile");
        
            if (isHitByEnemy)
            {
                var collisionPoint = other.GetComponent<Collider>().ClosestPoint(transform.position);

                var laserDamage = other.gameObject.GetComponent<Laser>().damage;
            
                GameManager.instance.OnHit(new HitInfo(GameHitEntity.Enemy, GetDamage(laserDamage)), GameHitEntity.Ally, collisionPoint);
            }
        }));
    }

    private void Update()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        
        _moveInput = cam.transform.rotation * new Vector3(horizontalInput, 0, verticalInput).normalized * (Speed + additionalSpeed);

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
            weapon.Shoot(transform.rotation);
        }
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector3(_moveInput.x, _rb.linearVelocity.y, _moveInput.z);

        var mouseWorldPosition = GetMouseWorldPosition();

        if (debugPointer)
        {
            debugPointer.transform.position = mouseWorldPosition;
        }

        transform.LookAt(new Vector3(mouseWorldPosition.x, transform.position.y, mouseWorldPosition.z));
    }
    
    private void LateUpdate()
    {
        if (SettingsManager.instance.cameraType == CameraMode.Static) return;
        
        var movementOffset = _moveInput / 4 + _rb.position;
        
        var targetPosition = _cameraOffset + movementOffset;
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

    private void AddModification(Modification modification)
    {
        var mod = Instantiate(modificationPrefab, weapon.transform);
        
        var modObject = mod.GetComponent<ModificationObject>();
        
        weapon.AddModification(modObject, modification);
    }

    public void Heal()
    {
        if (ModManager.instance.HasMoneyEqualsLife())
        {
            GameManager.instance.score += (int)_maxHealthpoints;    
        }
        
        healthpoints = _maxHealthpoints;
    }

    public bool BuyModification(StoreItem item)
    {
        if (item.type == StoreItemType.Skip)
        {
            return true;
        }
        
        if (GameManager.instance.score > item.price || item.price == 0)
        {
            GameManager.instance.score -= (int)item.price;
            moneySpent += (int)item.price;
            
            item.Buy();
            AddModification(item.modification);
            
            return true;
        }
        else
        {
            return false;
        }
    }

    public ReadOnlyCollection<Modification> GetModifications()
    {
        return weapon.modifications;
    }

    public ReadOnlyCollection<Modification> DropModifications()
    {
        return weapon.DropModifications();
    }
}