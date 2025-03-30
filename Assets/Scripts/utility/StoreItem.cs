using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public enum StoreItemType
{
    Modification,
    Skip
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

        switch (mod.type)
        {
            case ModificationType.AddFlatValue:
                name = "Синяя линза";
                description = "Добавляет +15 базового урона";
                break;
            case ModificationType.AddMultiplyValue:
                name = "Красная линза";
                description = "Добавляет +2 базового множителя";
                break;
            case ModificationType.MultiplyMultiplyValue:
                name = "Желтая линза";
                description = "Умножает базовый множитель на x2";
                break;
            case ModificationType.RubberDuck:
                name = "Уточка-уборщик";
                description = "В конце раунда появится Уточка-уборщик";
                break;
            case ModificationType.MoneyEqualsLife:
                name = "Деньги - жизнь";
                description = "Использует деньги, вместо HP";
                break;
            default:
                name = "";
                description = "";
                break;
        }
    }
    
    public StoreItem()
    {
        type = StoreItemType.Skip;
        name = "";
        description = "Пропустить";
    }

    public void Buy()
    {
        isSold = true;
        price = 999999999;
    }
}

