using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using utility;
using Random = UnityEngine.Random;

[System.Serializable]
public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject dummy;
    [SerializeField] private float flatDamage = 10f;
    [SerializeField] private float cooldown = 0.2f;
    [SerializeField] private List<ModificationObject> mods;
    [SerializeField] private AudioSource shootSound;
    [SerializeField] private bool isOwnerPlayer;
    // [SerializeField] private AudioSource jamSound;
    
    private float _sinceLastShot;
    
    public ReadOnlyCollection<Modification> modifications => mods.AsReadOnly().Select(obj => obj.mod).ToList().AsReadOnly();
    
    private void Start()
    {
        if (!laserPrefab)
        {
            laserPrefab = Resources.Load<GameObject>("Prefabs/Laser");
        }
    }

    private float GetDamage()
    {
        var flatValue = flatDamage;
        var multValue = 1f;

        mods.Sort((mod1, mod2) => mod1.order - mod2.order);

        var damageMods = mods.Where((mod) => mod.mod.type is ModificationType.AddFlatValue or ModificationType.AddMultiplyValue or ModificationType.DoubleDamageAndTaken);

        foreach (var modObject in damageMods)
        {
            var mod = modObject.GetStats();
            
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

    public void AddModification(ModificationObject mod, Modification modification)
    {
        mod.Init(modification, mods.Count);

        mods.Add(mod);
    }

    public void RemoveModificationsOfType(ModificationType type)
    {
        mods.RemoveAll(mod => mod.mod.type == type);
    }
    
    public ReadOnlyCollection<Modification> DropModifications()
    {
        var oldMods = modifications; 
        
        mods.Clear();
        
        return oldMods;
    }

    public void Shoot(Quaternion rotation)
    {
        Shoot(rotation, GetDamage());
    }
    
    public void ShootWithMultiply(Quaternion rotation, float multiplier)
    {
        Shoot(rotation, GetDamage() * multiplier);
    }

    public void Shoot(Quaternion rotation, float damage)
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
            
            var damageMods = mods.Where((mod) => mod.mod.type == ModificationType.MultiplyMultiplyValue);
            
            var projectileCount = 1 * Mathf.Pow(2, damageMods.Count());

            var shotId = ShotManager.Instance.GenerateNewShotId();

            for (var i = 0; i < projectileCount; i++)
            {
                var laser = Instantiate(laserPrefab, position + direction.normalized * 1.5f , rotation);
                
                laser.transform.rotation = rotation;

                var laserScript = laser.GetComponent<Laser>();

                laserScript.damage = damage;
                
                laserScript.shotId = shotId;

                if (!isOwnerPlayer) continue;
                
                if (ModManager.instance.HasMod(ModificationType.BurnEffect))
                {
                    laserScript.isBurn = true;
                }
                    
                if (ModManager.instance.HasMod(ModificationType.GhostLaser))
                {
                    laserScript.isSolid = true;
                }
            }
        }
    }
}
