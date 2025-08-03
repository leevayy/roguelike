using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimationController : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    
    private static readonly int IsFired = Animator.StringToHash("IsFired");
    private static readonly int IsDead = Animator.StringToHash("IsDead");
    private static readonly int IsMelee = Animator.StringToHash("IsMelee");
    
    private Animator _animator;
    
    private float _moveY;
    private float _moveX;
    
    public void Initialize(bool isMelee)
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _animator.SetBool(IsMelee, isMelee);
    }

    public void Tick(Vector3 movementDirection)
    {
        if (!_animator) return;

        const float epsilon = 0.001f;

        var isMoving = Vector3.Distance(movementDirection, Vector3.zero) > epsilon;

        _animator.SetBool(IsMoving, isMoving);
        _animator.SetFloat(Horizontal, movementDirection.x);
        _animator.SetFloat(Vertical, movementDirection.z);
    }

    public void FireAnimation()
    {
        _animator.SetTrigger(IsFired);
    }

    public void Die()
    {
        _animator.SetBool(IsDead, true);
    }
}