using System;
using UnityEngine;

public class ModificationObject : MonoBehaviour
{
    [SerializeField] private Material addFlatValueMaterial;
    [SerializeField] private Material addMultiplyValueMaterial;
    [SerializeField] private Material multiplyMultiplyValueMaterial;
    
    private Modification _mod;
    private bool _isInitialized;
    public int order { get; private set; }

    public void Init(Modification mod, int newOrder)
    {
        if (_isInitialized)
        {
            throw new Exception("Modification is already initialized");
        }
                
        _mod = mod;
        ChangeOrder(newOrder);
        
        var meshRenderer = GetComponent<MeshRenderer>();

        if (!meshRenderer)
        {
            return;
        }

        meshRenderer.material = _mod.type switch
        {
            ModificationType.AddFlatValue => addFlatValueMaterial,
            ModificationType.AddMultiplyValue => addMultiplyValueMaterial,
            ModificationType.MultiplyMultiplyValue => multiplyMultiplyValueMaterial,
            _ => meshRenderer.material
        };
    }

    public void ChangeOrder(int newOrder)
    {
        order = newOrder;
        transform.localScale = Vector3.one + Vector3.one / 10f * order;
    }
    
    public Modification GetStats()
    {
        return _mod;
    }
}