using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;
using utility;

class HitscanWeapon : Weapon
{
    [SerializeField] private Color color = Color.lightBlue;
    const float LASER_WIDTH = 0.1f;
    const float LASER_ALPHA = 0.8f;
    private const float LASER_TTL = 0.1f;
    private LineRenderer _lineRenderer;
    private Awaitable _currentTask;

    private void Awake()
    {
        CreateLineRenderer();
    }

    protected override void PerformShot(Vector3 position, Vector3 direction, Quaternion rotation, float damage, int shotId, Action<GameObject, Laser, Quaternion, float, int> applyLaserData, ReadOnlyCollection<Modification> modifications, int projectileCount = 1)
    {
        foreach (var mod in modifications)
        {
            mod.Strategy.ApplyOnShoot(this, damage);
        }

        for (var i = 0; i < projectileCount; i++)
        {
            var directionWithRecoil = GetDirectionWithRecoil(direction);

            var isHit = Physics.Raycast(transform.position, directionWithRecoil, out var hit, 100f);

            var hitPoint = isHit ? hit.point : transform.position + directionWithRecoil * 100f;

            _ = DrawLaser(transform.position, hitPoint);

            if (isHit)
            {
                TryHitTarget(hit, applyLaserData, rotation, damage, shotId);    
            }
        }
    }

    private Vector3 GetDirectionWithRecoil(Vector3 direction)
    {
        var recoil = 0.1f; // Example recoil value, adjust as needed
        direction += new Vector3(UnityEngine.Random.Range(-recoil, recoil), UnityEngine.Random.Range(-recoil, recoil), 0);
        direction.Normalize();

        return direction;
    }

    private void TryHitTarget(RaycastHit hit, Action<GameObject, Laser, Quaternion, float, int> applyLaserData, Quaternion rotation, float damage, int shotId)
    {
        var targetCollider = hit.collider;

        var laserCollider = CreateLaserObject(hit.collider, applyLaserData, rotation, damage, shotId);

        // targetCollider.SendMessage("OnTriggerEnter", laserCollider, SendMessageOptions.DontRequireReceiver);
    }

    private Collider CreateLaserObject(Collider target, Action<GameObject, Laser, Quaternion, float, int> applyLaserData, Quaternion rotation, float damage, int shotId)
    {
        var laserHit = new GameObject("LaserHit");
        laserHit.tag = isOwnerPlayer ? "AllyProjectile" : "EnemyProjectile";
        // this line triggers the collision
        // feels hacky but just as hacky as all of the virtual laser creation stuff
        laserHit.transform.position = target.transform.position;

        var laser = laserHit.AddComponent<Laser>();
        
        var collider = laserHit.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.1f;

        var rb = laserHit.AddComponent<Rigidbody>();

        applyLaserData(laserHit, laser, rotation, damage, shotId);

        return collider;
    }

    private async Awaitable DrawLaser(Vector3 start, Vector3 end)
    {
        _currentTask?.Cancel();

        _lineRenderer.SetPositions(new Vector3[] { start, end });

        _lineRenderer.enabled = true;

        try
        {
            _currentTask = Awaitable.WaitForSecondsAsync(LASER_TTL);
            await _currentTask;

            _currentTask = null;
            _lineRenderer.enabled = false;
        }
        catch (OperationCanceledException) { }
    }

    private LineRenderer CreateLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();

        _lineRenderer.startWidth = LASER_WIDTH;
        _lineRenderer.endWidth = LASER_WIDTH;

        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new(color, 0f) },
            new GradientAlphaKey[] { new(LASER_ALPHA, 0f) }
        );

        _lineRenderer.colorGradient = gradient;

        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        _lineRenderer.enabled = false;

        return _lineRenderer;
    }
}