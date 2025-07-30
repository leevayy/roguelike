using System;
using UnityEngine;
using utility;

public class MovementManager : MonoBehaviour
{
    private const float Speed = 10f;
    private const float JumpForce = 2f;
    private const float DashMultiplier = 10f;
    
    private bool _isGrounded;
    private Vector3 _moveVector;
    public Vector3 MoveVector => _moveVector;
    
    public float additionalSpeed;

    public void Tick(Vector3 moveInput)
    {
        _isGrounded = Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1.1f);
        
        _moveVector = GetMovementVector(moveInput);
    }

    public void FixedTick(Rigidbody rb)
    {
        Move(rb, _moveVector);
    }

    private Vector3 GetMovementVector(Vector3 moveInput)
    {
        var compoundSpeed = Speed + additionalSpeed;

        compoundSpeed += 5f * ModManager.instance.CountMod(ModificationType.MoveSpeedIncrease);

        return moveInput * compoundSpeed;
    }
    
    private void Move(Rigidbody rb, Vector3 moveVector)
    {
        rb.linearVelocity = new Vector3(moveVector.x, rb.linearVelocity.y, moveVector.z);
    }

    public void Jump(Rigidbody rb, Action onDash)
    {
        if (!_isGrounded) return;
        
        var jumpForce = new Vector3(_moveVector.x * DashMultiplier, JumpForce, _moveVector.z * DashMultiplier);
        rb.AddForce(jumpForce, ForceMode.Impulse);

        onDash();
    } 
}
