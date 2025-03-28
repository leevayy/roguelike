using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject dummy;
    [SerializeField] private float flatDamage = 10f;
    [SerializeField] private List<ModificationObject> modifications;
    
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

        modifications.Sort((mod1, mod2) => mod1.order - mod2.order);

        foreach (var modObject in modifications)
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
                case ModificationType.MultiplyMultiplyValue:
                    multValue *= mod.value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        return flatValue * multValue;
    }

    public void AddModification(ModificationObject mod, Modification modification)
    {
        mod.Init(modification, modifications.Count);

        modifications.Add(mod);
    }

    public void Shoot(Quaternion rotation)
    {
        var position = transform.position;

        var direction = rotation * Vector3.forward;

        if (dummy)
        {
            dummy.transform.position = position + direction.normalized * 3f;
        }
        
        // Instantiate the laser at the firing point
        if (laserPrefab)
        {
            var laser = Instantiate(laserPrefab, position + direction.normalized * 1.5f , rotation);
            
            laser.transform.rotation = rotation;

            var laserScript = laser.GetComponent<Laser>();
            
            laserScript.damage = GetDamage();
        }
    }
}
