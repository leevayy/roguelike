using UnityEngine;
using utility;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject debugPointer;
    [SerializeField] private Weapon weapon;
    [SerializeField] private GameObject modificationPrefab;
    [SerializeField] private GameObject moneyBagPrefab;

    private const float Speed = 10f;
    private const float JumpForce = 2f;
    private const float DashMultiplier = 10f;
    private readonly Vector3 _cameraOffset = new(-15, 12, -15);


    public GameObject moneyBag { get; private set; }
    private Rigidbody _rb;
    private bool _isGrounded;
    private Vector3 _moveInput;
    private Vector3 _mousePosition;
    private Vector3 _cameraVelocity = Vector3.zero;
    private float _healthpoints = 100f;

    public float additionalSpeed = 0f;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    
        var moneyBagOffset = new Vector3(0, 0.1f, -0.7f);
        var moneyBagRotation = Quaternion.Euler(0, -90, 0);
    
        moneyBag = Instantiate(moneyBagPrefab, transform.position + moneyBagOffset, moneyBagRotation, transform);
    }

    private void Update()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        
        _moveInput = cam.transform.rotation * new Vector3(horizontalInput, 0, verticalInput).normalized * (Speed + additionalSpeed);

        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
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

        transform.LookAt(new Vector3(mouseWorldPosition.x, 0f, mouseWorldPosition.z));
    }
    
    private void LateUpdate()
    {
        if (SettingsManager.instance.cameraType == CameraMode.Static) return;
        
        var movementOffset = (_mousePosition + _rb.position) / 2;
        
        var targetPosition = _cameraOffset + movementOffset;
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPosition, ref _cameraVelocity, 0.25f);
    }

    private Vector3 GetMouseWorldPosition()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var groundPlane = new Plane(Vector3.up, new Vector3(0f, -1f, 0f));

        return groundPlane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var isHitByEnemy = collision.collider.CompareTag("EnemyProjectile");
        
        if (isHitByEnemy)
        {
            var laserDamage = collision.gameObject.GetComponent<Laser>().damage;
            
            GameManager.instance.OnHit(new HitInfo(GameHitEntity.Enemy, GetDamage(laserDamage)), GameHitEntity.Ally, collision.GetContact(0).point);
        }
    }

    private float GetDamage(float damageIn)
    {
        _healthpoints -= damageIn;

        return damageIn;
    }

    public void AddModification(Modification modification)
    {
        var mod = Instantiate(modificationPrefab, weapon.transform);
        
        var modObject = mod.GetComponent<ModificationObject>();
        
        weapon.AddModification(modObject, modification);
    }
}