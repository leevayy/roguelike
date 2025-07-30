# Aspect 6: Shop & Economy System

## Purpose and Role

This system manages the game's economy, where the player's `score` is used as currency. It provides the interface for players to spend their score on upgrades between combat encounters. The system is responsible for generating shop inventories, displaying items for purchase, and handling the transaction process.

## Relevant Files and Classes

-   **`ShopManager.cs`**: A singleton that manages the lifecycle of the shop. It creates and destroys the shop items available for purchase.
-   **`BuyBox.cs`**: The script attached to each item that can be purchased. It detects when the player is in range, displays item information, and handles the purchase input (`E` or `F` key).
-   **`utility/StoreItem.cs`**: A data class that defines an item for sale. It holds a `Modification` or a special type (like `Reroll` or `Skip`), its price, name, and description.
-   **`Player.cs`**: The `Player` class contains the `BuyItem` method, which is the final step in a transaction, validating if the player can afford the item and then applying its effects.
-   **`GameManager.cs`**: The `score` property on the `GameManager` serves as the player's currency.

## Interactions with Other Parts

-   **Game State System**: `GameManager` dictates when the shop is active. After a goal is completed, it calls `ShopManager.instance.RefreshStore()` to generate a new set of items.
-   **Player System**: The `BuyBox` needs a reference to the `Player` to initiate a purchase. The `Player`'s `BuyItem` method is the authority on whether a purchase is successful.
-   **Modification & Item System**: The most common type of `StoreItem` is `Modification`. When a modification is purchased, the `Player`'s `AddModification` method is called, integrating the new item into the player's build.
-   **UI**: The `BuyBox` controls a `VisualElement` (price tag) that is displayed when the player is near an item. The `GameManager` is responsible for showing the player's total score.

## Code Snippets and Logic

### Shop Population

The `ShopManager` populates the store with a mix of random modifications and fixed items like "Reroll".

```csharp
// In ShopManager.cs

public void RefreshStore(float minPrice, float maxPrice, int discount = 0)
{
    _ = DestroyStoreItems(0f); // Clear out old items

    CreateSkipStoreItem(); // An option to continue without buying
    
    // Create three random modifications for sale
    for (var i = 0; i < 3; i++)
    {
        CreateModStoreItem(i, (int)Random.Range(minPrice, maxPrice), discount);
    }
    
    // Create the "Reroll" option
    CreateStoreItem(new StoreItem(StoreItemType.Reroll), 3);
}

private void CreateStoreItem(StoreItem storeItem, int order)
{
    // Instantiate the physical prefab for the item
    var buyBox = Instantiate(buyBoxPrefab, /* position */, transform.rotation, transform);
    
    var priceTag = buyBox.GetComponentInChildren<PriceTag>(); // This is a UI component
    
    // Initialize the buy box with the item data and a callback
    priceTag.Init(storeItem, () =>
    {
        _isPurchasedSomething = true;
        GameManager.instance.OnBuy(); // Notify GameManager of a purchase
    });
    
    _buyBoxes.Add(buyBox);
}
```

### The Purchase Process

The purchase flow involves multiple classes working together.

```csharp
// 1. In BuyBox.cs - Detects player input
private void Update()
{
    if (_isWithinRange && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F)))
    {
        // ...
        _ = Buy(_player);
    }
}

private async Awaitable Buy(Player player)
{
    if (_item == null || _item.isSold) return;
    
    // 2. In BuyBox.cs - Calls the player's BuyItem method
    var isBought = player.BuyItem(_item);

    // 3. In BuyBox.cs - Update UI based on success or failure
    if (isBought)
    {
        buySound.Play();
        _onBuy?.Invoke(); // The callback to ShopManager/GameManager
        _priceText.text = "Куплено!";
    }
    else
    {
        _hint.text = "Недостаточно средств! ❌";
    }
}

// 4. In Player.cs - The authoritative transaction method
public bool BuyItem(StoreItem item)
{
    // Check if the player has enough score
    if (GameManager.instance.score >= item.price || item.price == 0)
    {
        GameManager.instance.score -= (int)item.price;
        MoneySpent += (int)item.price;
        
        item.Buy(); // Mark the item as sold

        // Apply the item's effect
        if (item.type == StoreItemType.Modification)
        {
            AddModification(item.modification, item.name);
        }
        else if (item.type == StoreItemType.Reroll)
        {
            GameManager.instance.RerollShop();
        }

        return true; // Success
    }

    return false; // Failure
}
```
