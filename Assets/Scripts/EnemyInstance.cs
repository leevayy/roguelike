using System;
using JetBrains.Annotations;
using UnityEngine;
using utility;

public class EnemyInstance : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    private string _name;
    private float _maxHealthPoints = 100f;
    private float _healthPoints = 100f;

    [CanBeNull] public Action<float, float> onHealthPointsChange { private get; set; }
    [CanBeNull] public Action onDispose { private get; set; }
    public bool isAlive { get; private set; } = true;
    
    public float healthPoints
    {
        get => _healthPoints;
        private set
        {
            if (Mathf.Approximately(_healthPoints, value)) return;
            
            _healthPoints = value;
            onHealthPointsChange?.Invoke(value, _maxHealthPoints); 
            
            if (healthPoints <= 0)
            {
                Die(enemy.GetHitbox());
            }
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
                    
                    Throwback(hitDirection);

                    GameManager.instance.OnKill();
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

    public void Kill()
    {
        healthPoints = 0;
    }

    private void Die(Hitbox hitbox)
    {
        isAlive = false;
                
        enemy.PickTarget(null);
                
        enemy.StopMoving();
                
        hitbox.enabled = false;

        StartCoroutine(Flatten());
    }

    private void Throwback(Vector3 hitDirection)
    {
        var enemyRigidbody = enemy.GetComponent<Rigidbody>();

        enemyRigidbody.constraints = RigidbodyConstraints.None;
        
        enemyRigidbody.AddForceAtPosition(hitDirection, enemy.transform.position + Vector3.up, ForceMode.Impulse);
                
        var rotationAxis = Vector3.Cross(hitDirection, Vector3.up) * -1;
                
        enemyRigidbody.AddTorque(rotationAxis, ForceMode.Force);
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

    public void SetLevel(int lvl)
    {
        _maxHealthPoints = HealthScaleFunction(lvl);
        healthPoints = _maxHealthPoints;
        AddModifications(ModCountScaleFunction(lvl));
    }

    private static int HealthScaleFunction(int lvl)
    {
        return Mathf.RoundToInt((75 + 25 * (float)(lvl * lvl)) / 100) * 100;
    }

    private static int ModCountScaleFunction(int lvl)
    {
        return Mathf.FloorToInt(-1 + 1.5f * Mathf.Sqrt(lvl)) * Mathf.FloorToInt(1 + 1f / 200f * lvl * lvl);
    }

    private void AddModifications(int count)
    {
        for (int i = 0; i < count; i++)
        {
            enemy.AddModification(new Modification());
        }
    }

    private void OnDestroy()
    {
        onDispose?.Invoke();
    }
}
