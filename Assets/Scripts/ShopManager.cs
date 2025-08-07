using System;
using System.Collections.Generic;
using UnityEngine;
using utility;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject buyBoxPrefab;

    private readonly List<GameObject> _buyBoxes = new();
    private bool _isPurchasedSomething;
        
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

    public void RefreshStore(float minPrice, float maxPrice, int discount = 0)
    {
        _ = DestroyStoreItems(0f);

        CreateSkipStoreItem();

        for (var i = 0; i < 3; i++)
        {
            CreateModStoreItem(i, (int)Random.Range(minPrice, maxPrice), discount);
        }

        CreateStoreItem(new StoreItem(StoreItemType.Reroll), 3);
        
        CreateStoreItem(new StoreItem(StoreItemType.Heal), 4);
    }

    private void CreateModStoreItem(int order, float price, int discount)
    {
        CreateStoreItem(new StoreItem(new Modification(), price, discount), order);
    }

    private void CreateSkipStoreItem()
    {
        CreateStoreItem(new StoreItem(StoreItemType.Skip), -1);
    }
    
    private void CreateStoreItem(StoreItem storeItem, int order)
    {
        var buyBox = Instantiate(buyBoxPrefab, transform.position + Vector3.left * (6 * order), transform.rotation, transform);
        
        var priceTag = buyBox.GetComponentInChildren<PriceTag>();
        
        priceTag.Init(storeItem, () =>
        {
            _isPurchasedSomething = true;
            GameManager.instance.OnBuy();
        });
        
        _buyBoxes.Add(buyBox);
    }
    
    // private void DeactivateStoreItems()
    // {
    //     _buyBoxes.ForEach((buyBox) =>
    //     {
    //         buyBox.GetComponentInChildren<PriceTag>().buyBox.Deactivate();
    //     });
    // }

    private async Awaitable DestroyStoreItems(float delay)
    {
        if (delay != 0f)
        {
            await Awaitable.WaitForSecondsAsync(delay);
        }
        
        _buyBoxes.ForEach(Destroy);
        
        _buyBoxes.Clear();
        
        _isPurchasedSomething = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isPurchasedSomething && other.CompareTag("Player"))
        {
            _ = DestroyStoreItems(5f);
        }
    }
}
