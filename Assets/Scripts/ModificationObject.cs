using System;
using UnityEngine;

public class ModificationObject : MonoBehaviour
{
    [SerializeField] private Material addFlatValueMaterial;
    [SerializeField] private Material addMultiplyValueMaterial;
    [SerializeField] private Material multiplyMultiplyValueMaterial;
    [SerializeField] private Material rubberDuckMaterial;
    [SerializeField] private Material moneyEqualsLifeMaterial;
    
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

        meshRenderer.material = this.mod.type switch
        {
            ModificationType.AddFlatValue => addFlatValueMaterial,
            ModificationType.AddMultiplyValue => addMultiplyValueMaterial,
            ModificationType.MultiplyMultiplyValue => multiplyMultiplyValueMaterial,
            ModificationType.RubberDuck => rubberDuckMaterial,
            ModificationType.MoneyEqualsLife => moneyEqualsLifeMaterial,
            _ => meshRenderer.material
        };
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