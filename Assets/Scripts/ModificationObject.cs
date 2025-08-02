using System;
using UnityEngine;
using utility;

public class ModificationObject : MonoBehaviour
{
    public Modification mod { get; private set; }
    private bool _isInitialized;
    public int order { get; private set; }
    
    public void Init(Modification mod, int newOrder)
    {
        if (_isInitialized)
        {
            throw new Exception("Modification is already initialized");
        }
                
        this.mod = mod;
        ChangeOrder(newOrder);
        
        var meshRenderer = GetComponent<MeshRenderer>();

        if (!meshRenderer)
        {
            return;
        }

        // Safety check to prevent null reference exceptions
        if (this.mod?.Strategy?.Material != null)
        {
            meshRenderer.material = this.mod.Strategy.Material;
        }
        else
        {
            Debug.LogWarning($"ModificationObject: Material is null for modification type {this.mod?.Type}");
        }
    }

    public void ChangeOrder(int newOrder)
    {
        order = newOrder;
        transform.localScale = Vector3.one + (Vector3.one - Vector3.up) / 10f * order;
    }
    
    public Modification GetStats()
    {
        return mod;
    }
}