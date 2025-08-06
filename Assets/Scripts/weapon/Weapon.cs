using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using utility;
using Random = UnityEngine.Random;

[System.Serializable]
public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject dummy;
    [SerializeField] protected float flatDamage = 10f;
    [SerializeField] protected float cooldown = 0.2f;
    [SerializeField] protected AudioSource shootSound;
    [SerializeField] protected bool isOwnerPlayer;
    // [SerializeField] private AudioSource jamSound;

    protected float _sinceLastShot;
    protected bool _shouldApplyBurn;
    protected bool _shouldApplyGhost;

    public virtual Action OnShoot { protected get; set; } = null;

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

        ProcessShot(rotation, damage, modifications, projectileCount);
    }

    // added for legacy support with MeleeWeapon class

    // for llm:
    // i had #file:MeleeWeapon.cs written as an extension of #file:Weapon.cs , but since then i changed arhitecture a bit
    // can you based on my implementation of #file:HitscanWeapon.cs rewrite #file:MeleeWeapon.cs class to use new architecture?
    // it has 3 layers Shoot - public method to be called, ProcessShot - internal method to handle shot logic, and PerformShot - virtual method to be overridden by derived classes
    public virtual void Shoot(Quaternion rotation, float damage, ReadOnlyCollection<Modification> modifications, int projectileCount = 1)
    {
        ProcessShot(rotation, damage, modifications, projectileCount);
    }

    private void ProcessShot(Quaternion rotation, float damage, ReadOnlyCollection<Modification> modifications, int projectileCount = 1)
    {
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

        var pitch = 3 - _sinceLastShot / (_sinceLastShot + cooldown + 1) * (1 * Random.Range(0f, 1f));
        shootSound.pitch = pitch;

        OnShoot?.Invoke();
        shootSound.Play();

        var shotId = ShotManager.Instance.GenerateNewShotId();

        PerformShot(position, direction, rotation, damage, shotId, ApplyLaserData, modifications, projectileCount);
    }

    // This method is expected to be overridden by derived classes
    protected virtual void PerformShot(Vector3 position, Vector3 direction, Quaternion rotation, float damage, int shotId, Action<GameObject, Laser, Quaternion, float, int> applyLaserData, ReadOnlyCollection<Modification> modifications, int projectileCount = 1)
    {
        foreach (var mod in modifications)
        {
            mod.Strategy.ApplyOnShoot(this, damage);
        }

        for (var i = 0; i < projectileCount; i++)
        {
            var laser = Instantiate(laserPrefab, position + direction.normalized * 1.5f, rotation);

            var laserScript = laser.GetComponent<Laser>();

            applyLaserData(laser, laserScript, rotation, damage, shotId);
        }
    }

    private void ApplyLaserData(GameObject holder, Laser script, Quaternion rotation, float damage, int shotId)
    {
        holder.transform.rotation = rotation;

        script.damage = damage;
        script.shotId = shotId;

        if (_shouldApplyBurn)
        {
            script.isBurn = true;
        }

        if (_shouldApplyGhost)
        {
            script.isSolid = true;
        }
    } 
}
