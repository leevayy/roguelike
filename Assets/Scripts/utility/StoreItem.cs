using System;
using UnityEngine;
using utility;

public enum StoreItemType
{
    Modification,
    Skip,
    Reroll,
    Heal
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
            name = $"({mod.Strategy.Rarity.ToString()[0]}) {mod.Strategy.Name ?? "Неизвестная модификация"}";
            description = mod.Strategy.Description ?? "Описание отсутствует";
        }
        else
        {
            name = "Неизвестная модификация";
            description = "Описание отсутствует";
            Debug.LogWarning($"StoreItem: Strategy is null for modification type {mod?.Type}");
        }
    }

    public float GetRarityPrice(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return 6f;
            case Rarity.Uncommon:
                return 8f;
            case Rarity.Rare:
                return 10f;
            case Rarity.Epic:
                return 11f;
            case Rarity.Legendary:
                return 12f;
            default:
                return 1f;
        }
    }
    
    public StoreItem(Modification mod, float p, float discount) : this(mod, p)
    {
        price = GetRarityPrice(mod.Strategy.Rarity);

        discount = 0;
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
                price = 1;
                break;
            case StoreItemType.Heal:
                description = "Восстановить здоровье";
                price = 1;
                break;
            case StoreItemType.Modification:
                modification = new Modification();
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
