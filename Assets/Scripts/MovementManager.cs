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

    private ComposableModificationManager _modManager;
    private Func<AliveState> _getAliveState;

    public void Initialize(ComposableModificationManager modManager, Func<AliveState> getAliveState)
    {
        _modManager = modManager;
        _getAliveState = getAliveState;
    }

    public void Tick(Vector3 moveInput)
    {
        _isGrounded = Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1.1f);

        _moveVector = GetMovementVector(moveInput);
    }

    public void FixedTick(Rigidbody rb)
    {
        Move(rb, _moveVector);
    }

    public float GetSpeed()
    {
        var compoundSpeed = Speed + additionalSpeed;

        if (_modManager != null)
        {
            compoundSpeed = _modManager.GetModifiedValue(aliveState: _getAliveState(), compoundSpeed, ModificationType.MoveSpeedIncrease);

            compoundSpeed = _modManager.GetModifiedValue(aliveState: _getAliveState(), compoundSpeed, ModificationType.Desperation);

            compoundSpeed = _modManager.GetModifiedValue(aliveState: _getAliveState(), compoundSpeed, ModificationType.SpeedDemon);
        }

        return compoundSpeed;
    }

    private Vector3 GetMovementVector(Vector3 moveInput)
    {
        var speed = GetSpeed();

        return moveInput * speed;
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
