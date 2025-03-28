using System;
using JetBrains.Annotations;
using UnityEngine;
using utility;

public class EnemyInstance : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [CanBeNull] public Action<float> onHealthPointsChange { private get; set; }
    public bool isAlive { get; private set; } = true;
    
    private float _healthPoints = 100f;
    private string _name;

    public float healthPoints
    {
        get => _healthPoints;
        private set
        {
            if (Mathf.Approximately(_healthPoints, value)) return;
            
            _healthPoints = value;
            onHealthPointsChange?.Invoke(value); 
        }
    }

    private void Awake()
    {
        _name = RandomName.GetRandomName();
        gameObject.name = _name;
            
        enemy.GetHitbox().SetOnTriggerEnterHandler((other, hitbox) =>
        {
            if (!isAlive)
            {
                return;
            }
        
            var isHitByAlly = other.CompareTag("AllyProjectile");
        
            if (isHitByAlly)
            {
                var collisionPoint = other.GetComponent<Collider>().ClosestPoint(enemy.transform.position);

                var laserDamage = other.GetComponent<Laser>().damage;
                
                GameManager.instance.OnHit(new HitInfo(GameHitEntity.Ally, GetDamage(laserDamage)), GameHitEntity.Enemy, collisionPoint);

                if (healthPoints <= 0)
                {
                    var hitDirection = other.GetComponent<Rigidbody>().linearVelocity.normalized;

                    Die(hitbox, hitDirection);
                }
            }
        });
    }

    private void Start()
    {
        if (_name == "Max Verstappen")
        {
            enemy.SET_MAX_SPEED();
        }
    }

    private float GetDamage(float damageIn)
    {
        healthPoints -= damageIn;
        
        return damageIn;
    }

    private void Die(Hitbox hitbox, Vector3 hitDirection)
    {
        isAlive = false;
                
        enemy.PickTarget(null);
                
        enemy.StopMoving();
                
        hitbox.enabled = false;

        var enemyRigidbody = enemy.GetComponent<Rigidbody>();

        enemyRigidbody.constraints = RigidbodyConstraints.None;
        
        enemyRigidbody.AddForceAtPosition(hitDirection, enemy.transform.position + Vector3.up, ForceMode.Impulse);
                
        var rotationAxis = Vector3.Cross(hitDirection, Vector3.up) * -1;
                
        enemyRigidbody.AddTorque(rotationAxis, ForceMode.Force);

        Flatten();
    }

    private async Awaitable Flatten()
    {
        var capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        var sphereCollider = GetComponentInChildren<SphereCollider>();
        capsuleCollider.radius = 0.1f;
        sphereCollider.radius = 0.1f;

        const float duration = 5f;
        var elapsedTime = 0f;
        var startScale = Vector3.one;
        var targetScale = new Vector3(1.5f, 0.1f, 1.5f);

        while (elapsedTime < duration)
        {
            var t = elapsedTime / duration;
            this.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }
        this.transform.localScale = targetScale;
    }
        
    public void FocusEntity(GameObject entity)
    {
        if (!enemy)
        {
            return;
        }

        enemy.StartMoving();
        enemy.PickTarget(entity);
    }
}
