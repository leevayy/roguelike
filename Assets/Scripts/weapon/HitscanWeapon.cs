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
    private const float RECOIL = 0.05f;

    // private LineRenderer lineRenderer;
    private Awaitable _currentTask;

    protected override void PerformShot(Vector3 position, Vector3 direction, Quaternion rotation, float damage, int shotId, Action<GameObject, Laser, Quaternion, float, int> applyLaserData, ReadOnlyCollection<Modification> modifications, int projectileCount = 1)
    {
        foreach (var mod in modifications)
        {
            mod.Strategy.ApplyOnShoot(this, damage);
        }

        for (var i = 0; i < projectileCount; i++)
        {
            var directionWithRecoil = GetDirectionWithRecoil(direction);

            ShootLaser(directionWithRecoil, applyLaserData, rotation, damage, shotId);
        }
    }

    private void ShootLaser(Vector3 directionWithRecoil, Action<GameObject, Laser, Quaternion, float, int> applyLaserData, Quaternion rotation, float damage, int shotId)
    {
        var maxDistance = 100f;
        var endPoint = transform.position + directionWithRecoil * maxDistance;
        
        var isHit = Physics.Raycast(transform.position, directionWithRecoil, out var hit, maxDistance);
        var hitPoint = isHit ? hit.point : endPoint;
        
        var laserPosition = _shouldApplyGhost ? endPoint : (isHit ? hit.transform.position : hitPoint);
        var lineEnd = _shouldApplyGhost ? endPoint : hitPoint;
        
        var laserHit = CreateLaserObject(laserPosition, applyLaserData, rotation, damage, shotId);
        var laserCollider = laserHit.GetComponent<Collider>();
        
        if (_shouldApplyGhost)
        {
            var hits = Physics.RaycastAll(transform.position, directionWithRecoil, maxDistance);
            Array.Sort(hits, (h1, h2) => h1.distance.CompareTo(h2.distance));
            
            foreach (var raycastHit in hits)
            {
                HitColliderWithLaserObject(raycastHit.collider, laserCollider);
            }
        }
        
        var lineRenderer = CreateLineRenderer(laserHit);
        _ = DrawLaser(lineRenderer, transform.position, lineEnd);
    }

    private Vector3 GetDirectionWithRecoil(Vector3 direction)
    {
        direction += new Vector3(UnityEngine.Random.Range(-RECOIL, RECOIL), UnityEngine.Random.Range(-RECOIL, RECOIL), 0);
        direction.Normalize();

        return direction;
    }

    private GameObject CreateLaserObject(Vector3 targetPosition, Action<GameObject, Laser, Quaternion, float, int> applyLaserData, Quaternion rotation, float damage, int shotId)
    {
        var laserHit = new GameObject("LaserHit");
        laserHit.tag = isOwnerPlayer ? "AllyProjectile" : "EnemyProjectile";
        // this line triggers the collision
        // feels hacky but just as hacky as all of the virtual laser creation stuff
        laserHit.transform.position = targetPosition;
        
        var collider = laserHit.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.1f;

        var rb = laserHit.AddComponent<Rigidbody>();
        
        var laser = laserHit.AddComponent<Laser>();
        laser.ttl = LASER_TTL;
        applyLaserData(laserHit, laser, rotation, damage, shotId);

        return laserHit;
    }

    private void HitColliderWithLaserObject(Collider target, Collider laserCollider)
    {
        target.SendMessage("OnTriggerEnter", laserCollider, SendMessageOptions.DontRequireReceiver);
    }

    private async Awaitable DrawLaser(LineRenderer lineRenderer, Vector3 start, Vector3 end)
    {
        _currentTask?.Cancel();

        lineRenderer.SetPositions(new Vector3[] { start, end });

        lineRenderer.enabled = true;

        try
        {
            _currentTask = Awaitable.WaitForSecondsAsync(LASER_TTL);
            await _currentTask;

            _currentTask = null;
            lineRenderer.enabled = false;
        }
        catch (OperationCanceledException) { }
    }

    private LineRenderer CreateLineRenderer(GameObject holder)
    {
        var lineRenderer = holder.AddComponent<LineRenderer>();

        lineRenderer.startWidth = LASER_WIDTH;
        lineRenderer.endWidth = LASER_WIDTH;

        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new(color, 0f) },
            new GradientAlphaKey[] { new(LASER_ALPHA, 0f) }
        );

        lineRenderer.colorGradient = gradient;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.enabled = false;

        return lineRenderer;
    }
}