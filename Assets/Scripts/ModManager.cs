using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModManager : MonoBehaviour
{
    [SerializeField] private GameObject buyBoxPrefab;

    private readonly List<GameObject> _buyBoxes = new(); 
    public static ModManager instance { get; private set; }

    public bool HasMod(ModificationType modType)
    {
        return GameManager.instance.modifications.ToList().Exists(mod => mod.type == modType);
    }
    
    public int CountMod(ModificationType modType)
    {
        return GameManager.instance.modifications.Count(mod => mod.type == modType);
    }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) {
            DropItems();
        }
    }

    private void DropItems()
    {
        var mods = GameManager.instance.OnDrop(out var playerTransform);

        var i = 0;
        foreach (var mod in mods)
        {
            CreateModStoreItem(mod, i, playerTransform);
            i++;
        }
    }

    private void CreateModStoreItem(Modification mod, int order, Transform playerTransform)
    {
        CreateStoreItem(new StoreItem(mod, 0), order, playerTransform);
    }
    
    private void CreateStoreItem(StoreItem storeItem, int order, Transform playerTransform)
    {
        var buyBox = Instantiate(buyBoxPrefab, playerTransform.position - Vector3.back + Vector3.left * (2 * order), playerTransform.rotation, transform);
        
        var priceTag = buyBox.GetComponentInChildren<PriceTag>();
        
        priceTag.Init(storeItem, () =>
        {
            _ = DestroyItem(buyBox, 0f);
        });
        
        _buyBoxes.Add(buyBox);
    }

    private async Awaitable DestroyItem(GameObject item, float delay)
    {
        await Awaitable.WaitForSecondsAsync(delay);
        
        Destroy(item);
    }
}
