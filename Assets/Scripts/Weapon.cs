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
        var flatValue = flatDamage;
        var multValue = 1f;

        var damageMods = modifications.Where((mod) => mod.type is ModificationType.AddFlatValue or ModificationType.AddMultiplyValue or ModificationType.DoubleDamageAndTaken);

        foreach (var mod in damageMods)
        {
            switch (mod.type)
            {
                case ModificationType.AddFlatValue:
                    flatValue += mod.value;
                    break;
                case ModificationType.AddMultiplyValue:
                    multValue += mod.value;
                    break;
                case ModificationType.DoubleDamageAndTaken:
                    multValue *= mod.value;
                    break;
                // case ModificationType.GlassLens:
                //     multValue += mod.value;
                //     break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        return flatValue * multValue;
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
        Shoot(rotation, GetDamage(modifications), modifications);
    }
    
    public void ShootWithMultiply(Quaternion rotation, float multiplier, ReadOnlyCollection<Modification> modifications)
    {
        Shoot(rotation, GetDamage(modifications) * multiplier, modifications);
    }

    public void Shoot(Quaternion rotation, float damage, ReadOnlyCollection<Modification> modifications)
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
            
            var projectileCount = 1 * Mathf.Pow(2, modifications.Count(mod => mod.type == ModificationType.MultiplyMultiplyValue));

            var shotId = ShotManager.Instance.GenerateNewShotId();

            for (var i = 0; i < projectileCount; i++)
            {
                var laser = Instantiate(laserPrefab, position + direction.normalized * 1.5f , rotation);
                
                laser.transform.rotation = rotation;

                var laserScript = laser.GetComponent<Laser>();

                laserScript.damage = damage;
                
                laserScript.shotId = shotId;

                if (!isOwnerPlayer) continue;

                if (modifications.Any(mod => mod.type == ModificationType.BurnEffect))
                {
                    laserScript.isBurn = true;
                }

                if (modifications.Any(mod => mod.type == ModificationType.GhostLaser))
                {
                    laserScript.isSolid = true;
                }
            }
        }
    }
}
