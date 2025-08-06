using System;
using UnityEngine;
using utility;

public enum StoreItemType
{
    Modification,
    Skip,
    Reroll
}

public class StoreItem
{
    public StoreItemType type { get; private set; }
    public Modification modification { get; private set; }
    public float price { get; private set; }
    public bool isSold { get; private set; }
    public string name { get; private set; }
    public string description { get; private set; }

    public StoreItem(Modification mod, float p)
    {
        type = StoreItemType.Modification;
        modification = mod;
        price = p;

        // Safety checks to prevent null reference exceptions
        if (mod?.Strategy != null)
        {
            name = mod.Strategy.Name ?? "Неизвестная модификация";
            description = mod.Strategy.Description ?? "Описание отсутствует";
        }
        else
        {
            name = "Неизвестная модификация";
            description = "Описание отсутствует";
            Debug.LogWarning($"StoreItem: Strategy is null for modification type {mod?.Type}");
        }
    }
    public float GetRarityPriceModificator(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return 1f;
            case Rarity.Uncommon:
                return 1.2f;
            case Rarity.Rare:
                return 1.5f;
            case Rarity.Epic:
                return 2f;
            case Rarity.Legendary:
                return 3f;
            default:
                return 1f;
        }
    }
    
    public StoreItem(Modification mod, float p, float discount) : this(mod, p)
    {
        var modifier = GetRarityPriceModificator(mod.Strategy.Rarity);

        price = Mathf.Round(p * modifier * (100 - discount) / 100);
        
        if (discount > 0)
        {
            name += $"(-{discount}%)";
        }
    }

    public StoreItem(StoreItemType itemType)
    {
        type = itemType;
        name = "";
        switch (itemType)
        {
            case StoreItemType.Skip:
                description = "Пропустить";
                break;
            case StoreItemType.Reroll:
                description = "Обновить";
                price = 50;
                break;
            case StoreItemType.Modification:
                this.modification = new Modification();
                price = 9999;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }
    }

    public void Buy()
    {
        isSold = true;
        price = 999999999;
    }
}
