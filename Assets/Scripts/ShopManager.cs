using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject buyBoxPrefab;

    private readonly List<GameObject> _buyBoxes = new(); 
    public static ShopManager instance { get; private set; }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    
    public void RefreshStore(float minPrice, float maxPrice)
    {
        _ = DestroyStoreItems(0f);

        CreateSkipStoreItem();
        
        for (var i = 0; i < 3; i++)
        {
            CreateModStoreItem(i, (int)Random.Range(minPrice, maxPrice));
        }
    }

    private void CreateModStoreItem(int order, float price)
    {
        CreateStoreItem(new StoreItem(new Modification(), price), order);
    }

    private void CreateSkipStoreItem()
    {
        CreateStoreItem(new StoreItem(), -1);
    }
    
    private void CreateStoreItem(StoreItem storeItem, int order)
    {
        var buyBox = Instantiate(buyBoxPrefab, transform.position + Vector3.left * (6 * order), transform.rotation, transform);
        
        var priceTag = buyBox.GetComponentInChildren<PriceTag>();
        
        priceTag.Init(storeItem, () =>
        {
            // DeactivateStoreItems();
            _ = DestroyStoreItems(3f);
            GameManager.instance.OnBuy();
        });
        
        _buyBoxes.Add(buyBox);
    }
    
    private void DeactivateStoreItems()
    {
        _buyBoxes.ForEach((buyBox) =>
        {
            buyBox.GetComponentInChildren<PriceTag>().buyBox.Deactivate();
        });
    }

    private async Awaitable DestroyStoreItems(float delay)
    {
        if (delay != 0f)
        {
            await Awaitable.WaitForSecondsAsync(delay);
        }
        
        _buyBoxes.ForEach(Destroy);
        
        _buyBoxes.Clear();
    }
}
