using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject debugPointer;
    [SerializeField] private Weapon weapon;

    private const float Speed = 10f;
    private const float JumpForce = 2f;
    private const float DashMultiplier = 10f;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 moveInput;
    private Vector3 mousePosition;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        
        moveInput = cam.transform.rotation * new Vector3(horizontalInput, 0, verticalInput).normalized * Speed;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            var jumpForce = new Vector3(moveInput.x * DashMultiplier, JumpForce, moveInput.z * DashMultiplier);
            rb.AddForce(jumpForce, ForceMode.Impulse);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            weapon.Shoot(transform.rotation);
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3(moveInput.x, rb.velocity.y, moveInput.z);

        var mouseWorldPosition = GetMouseWorldPosition();

        if (debugPointer)
        {
            debugPointer.transform.position = mouseWorldPosition;
        }

        transform.LookAt(new Vector3(mouseWorldPosition.x, 0f, mouseWorldPosition.z));
    }

    private Vector3 GetMouseWorldPosition()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var groundPlane = new Plane(Vector3.up, new Vector3(0f, -1f, 0f));

        return groundPlane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : Vector3.zero;
    }
}