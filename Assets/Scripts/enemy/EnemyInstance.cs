using System;
using JetBrains.Annotations;
using UnityEngine;
using utility;

public class EnemyInstance : MonoBehaviour, utility.IAliveEntity
{
    [SerializeField] private AudioSource maxVerstappenSound;
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

    public ComposableModificationManager modManager { get; private set; }

    // IAliveEntity implementation
    public bool IsAlive => isAlive;
    public bool IsGrounded => enemy != null && Physics.Raycast(enemy.transform.position + Vector3.up, Vector3.down, 1.1f);
    public float HealthPoints => healthPoints;
    public float MaxHealthPoints => _maxHealthPoints;
    public Vector3 Position => transform.position;
    public Transform Transform => transform;
    public ComposableModificationManager ModManager => modManager;

    public utility.AliveState GetAliveState()
    {
        var aliveState = new utility.AliveState(
            IsAlive,
            IsGrounded,
            HealthPoints,
            MaxHealthPoints,
            Position,
            Transform,
            ModManager
        );

        return aliveState;
    }

    private void Awake()
    {
        modManager = gameObject.AddComponent<ComposableModificationManager>();
        enemy.Initialize(modManager, () => GetAliveState());

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

                var laser = other.GetComponent<Laser>();

                var laserDamage = laser.damage;

                GameManager.instance.OnHit(new HitInfo(GameHitEntity.Ally, GetDamage(laserDamage), laser.shotId), GameHitEntity.Enemy, collisionPoint);

                if (laser.isBurn)
                {
                    // Only apply burn if this shot hasn't been processed before
                    if (ShotManager.Instance.TryProcessHit(laser.shotId))
                    {
                        _ = ApplyBurn((int)laserDamage / 2);
                    }
                }

                if (healthPoints <= 0)
                {
                    var hitDirection = other.GetComponent<Rigidbody>().linearVelocity.normalized;

                    Throwback(hitDirection);
                }

                Destroy(other.gameObject);
            }            
        });
    }

    private void Start()
    {
        if (_name == "Max Verstappen")
        {
            maxVerstappenSound.Play();
            enemy.SET_MAX_SPEED();
        }
    }

    private void Update()
    {
        // Apply modifications that run on update
        if (modManager != null)
        {
            modManager.ApplyOnUpdate(GetAliveState());
        }

        if (enemy && enemy.transform.position.y < -500)
        {
            Die(enemy.GetHitbox());
            
            Destroy(gameObject);
        }
    }

    private float GetDamage(float damageIn)
    {
        // Apply modifications that modify incoming damage
        var modifiedDamage = modManager.ModifyIncomingDamage(GetAliveState(), damageIn);

        var nextHealthPoints = healthPoints - modifiedDamage;
        
        if (nextHealthPoints <= 0 && isAlive)
        {
            GameManager.instance.OnKill(transform.position);
        }

        healthPoints = nextHealthPoints;

        // Apply modifications that trigger on taking damage
        if (modifiedDamage > 0)
        {
            modManager.ApplyOnTakeDamage(GetAliveState(), modifiedDamage);
        }
        
        return modifiedDamage;
    }


    private bool _isBurning;
    private int _ticksLeft = 5;
    
    private async Awaitable ApplyBurn(int damagePerTick)
    {
        if (_isBurning)
        {
            _ticksLeft += 5;
            return;
        }
        
        _isBurning = true;
        
        for (var i = 0; i < _ticksLeft; i++)
        {
            if (!isAlive) break;
            
            await Awaitable.WaitForSecondsAsync(1);
            
            GameManager.instance.OnHit(new HitInfo(GameHitEntity.Ally, GetDamage(damagePerTick), -1), GameHitEntity.Enemy, enemy.gameObject.transform.position);
        }

        _ticksLeft = 5;
        _isBurning = false;
    }

    public void Kill()
    {
        healthPoints = 0;
    }

    private void Die(Hitbox hitbox)
    {
        if (!isAlive) return;
        
        isAlive = false;

        enemy.Die();
                
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

        while (this && elapsedTime < duration)
        {
            var t = elapsedTime / duration;
            this.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }
        
        if (this) this.transform.localScale = targetScale;
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
            modManager.AddModification(new Modification());
        }
    }

    private void OnDestroy()
    {
        onDispose?.Invoke();
    }
}
