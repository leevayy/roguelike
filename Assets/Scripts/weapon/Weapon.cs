using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using utility;
using Random = UnityEngine.Random;

[System.Serializable]
public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject dummy;
    [SerializeField] protected float flatDamage = 10f;
    [SerializeField] protected float cooldown = 0.2f;
    [SerializeField] private AudioSource shootSound;
    [SerializeField] protected bool isOwnerPlayer;
    // [SerializeField] private AudioSource jamSound;
    
    protected float _sinceLastShot;
    protected bool _shouldApplyBurn;
    protected bool _shouldApplyGhost;

    public virtual Action OnShoot { protected get;  set; } = null;
    
    private void Start()
    {
        if (!laserPrefab)
        {
            laserPrefab = Resources.Load<GameObject>("Prefabs/Laser");
        }
    }

    protected float GetDamage(AliveState aliveState, ReadOnlyCollection<Modification> modifications)
    {
        var modifiedDamage = flatDamage;
        foreach (var mod in modifications)
        {
            if (mod.Strategy.IsNotModifyingDamage) continue;

            modifiedDamage = mod.Strategy.GetModifiedValue(aliveState, modifiedDamage);
        }
        return (float)Math.Round(modifiedDamage);
    }

    public void SetBurnEffect(bool shouldBurn)
    {
        _shouldApplyBurn = shouldBurn;
    }

    public void SetGhostEffect(bool shouldGhost)
    {
        _shouldApplyGhost = shouldGhost;
    }

    private void Update()
    {
        if (_sinceLastShot < cooldown + 1)
        {
            _sinceLastShot += Time.deltaTime;
        }
    }

    public virtual void Shoot(AliveState aliveState, Quaternion rotation, ReadOnlyCollection<Modification> modifications)
    {
        var damage = GetDamage(aliveState, modifications);
        var projectileCount = 1;
        foreach (var mod in modifications)
        {
            projectileCount = mod.Strategy.GetProjectileCount(projectileCount);
        }

        Shoot(rotation, damage, modifications, projectileCount);
    }

    public virtual void Shoot(Quaternion rotation, float damage, ReadOnlyCollection<Modification> modifications, int projectileCount = 1)
    {
        // Reset effect flags before applying modifications
        _shouldApplyBurn = false;
        _shouldApplyGhost = false;
        
        if (_sinceLastShot < cooldown)
        {
            // jamSound.Play();
            return;
        }
        
        _sinceLastShot = 0;
        
        var position = transform.position;

        var direction = rotation * Vector3.forward;

        if (dummy)
        {
            dummy.transform.position = position + direction.normalized * 3f;
        }
        
        // Instantiate the laser at the firing point
        if (laserPrefab)
        {
            var pitch = 3 - _sinceLastShot / (_sinceLastShot + cooldown + 1) * (1 * Random.Range(0f, 1f));
            shootSound.pitch = pitch;
                
            OnShoot?.Invoke();
            shootSound.Play();
            
            var shotId = ShotManager.Instance.GenerateNewShotId();

            foreach (var mod in modifications)
            {
                mod.Strategy.ApplyOnShoot(this, damage);
            }

            for (var i = 0; i < projectileCount; i++)
            {
                var laser = Instantiate(laserPrefab, position + direction.normalized * 1.5f , rotation);
                
                laser.transform.rotation = rotation;

                var laserScript = laser.GetComponent<Laser>();

                laserScript.damage = damage;
                laserScript.shotId = shotId;
                
                // Apply burn effect if it's been set
                if (_shouldApplyBurn)
                {
                    laserScript.isBurn = true;
                }
                
                // Apply ghost effect if it's been set
                if (_shouldApplyGhost)
                {
                    laserScript.isSolid = true;
                }
            }
        }
    }
}
