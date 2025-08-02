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
    [SerializeField] private float flatDamage = 10f;
    [SerializeField] private float cooldown = 0.2f;
    [SerializeField] private AudioSource shootSound;
    [SerializeField] private bool isOwnerPlayer;
    // [SerializeField] private AudioSource jamSound;
    
    private float _sinceLastShot;
    
    private void Start()
    {
        if (!laserPrefab)
        {
            laserPrefab = Resources.Load<GameObject>("Prefabs/Laser");
        }
    }

    private float GetDamage(ReadOnlyCollection<Modification> modifications)
    {
        var modifiedDamage = flatDamage;
        foreach (var mod in modifications)
        {
            modifiedDamage = mod.Strategy.GetModifiedValue(modifiedDamage);
        }
        return modifiedDamage;
    }

    private void Update()
    {
        if (_sinceLastShot < cooldown + 1)
        {
            _sinceLastShot += Time.deltaTime;
        }
    }

    public void Shoot(Quaternion rotation, ReadOnlyCollection<Modification> modifications)
    {
        var damage = GetDamage(modifications);
        var projectileCount = 1;
        foreach (var mod in modifications)
        {
            projectileCount = mod.Strategy.GetProjectileCount(projectileCount);
        }

        Shoot(rotation, damage, modifications, projectileCount);
    }
    
    public void ShootWithMultiply(Quaternion rotation, float multiplier, ReadOnlyCollection<Modification> modifications)
    {
        var damage = GetDamage(modifications) * multiplier;
        var projectileCount = 1;
        foreach (var mod in modifications)
        {
            projectileCount = mod.Strategy.GetProjectileCount(projectileCount);
        }
        Shoot(rotation, damage, modifications, projectileCount);
    }

    public void Shoot(Quaternion rotation, float damage, ReadOnlyCollection<Modification> modifications, int projectileCount = 1)
    {
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
                
            shootSound.Play();
            
            var shotId = ShotManager.Instance.GenerateNewShotId();

            for (var i = 0; i < projectileCount; i++)
            {
                var laser = Instantiate(laserPrefab, position + direction.normalized * 1.5f , rotation);
                
                laser.transform.rotation = rotation;

                var laserScript = laser.GetComponent<Laser>();

                laserScript.damage = damage;
                
                laserScript.shotId = shotId;

                if (!isOwnerPlayer) continue;

                foreach (var mod in modifications)
                {
                    mod.Strategy.ApplyOnShoot(this, damage);
                }
            }
        }
    }
}
