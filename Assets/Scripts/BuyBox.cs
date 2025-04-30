using System;
using UnityEngine;
using UnityEngine.UIElements;

public class BuyBox : MonoBehaviour
{
    [SerializeField] private ModificationObject mod;
    [SerializeField] private AudioSource buySound;

    private bool _isElevatorButton;
    private Action _onElevatorButtonPressed;
    
    private Action _onBuy;
    private StoreItem _item;
    
    private VisualElement _priceTag;
    private TextElement _name;
    private TextElement _description;
    private TextElement _priceText;
    private TextElement _hint;
    
    private bool _isWithinRange;
    private Player _player;
    private bool _isActive = true;

    public void Init(VisualElement priceTag, StoreItem item, Action onBuy)
    {
        if (item.type == StoreItemType.Modification)
        {
            mod.Init(item.modification, 0);
        } 
        
        _item = item;
        _priceTag = priceTag;
        _onBuy = onBuy;
        
        _name = priceTag.Q<TextElement>("name"); 
        _description = priceTag.Q<TextElement>("description"); 
        _priceText = priceTag.Q<TextElement>("PriceText"); 
        _hint = priceTag.Q<TextElement>("hint");

        _name.text = item.name;
        _description.text = item.description;
        _priceText.text = $"${item.price}";
    }
    
    public void Init(VisualElement priceTag, bool isElevatorButton, Action onElevatorButtonPressed)
    {
        _priceTag = priceTag;
        _isElevatorButton = isElevatorButton;
        _onElevatorButtonPressed = onElevatorButtonPressed;
        
        _name = priceTag.Q<TextElement>("name"); 
        _description = priceTag.Q<TextElement>("description"); 
        _priceText = priceTag.Q<TextElement>("PriceText"); 
        _hint = priceTag.Q<TextElement>("hint"); 

        _priceText.text = "Вызвать лифт - F";
        _hint.text = "";
    }

    private void Update()
    {
        if (_isWithinRange && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F)))
        {
            ControlGuide.instance.CompletedF();
            
            if (_isElevatorButton)
            {
                _onElevatorButtonPressed?.Invoke();
                
                return;
            }
            
            _ = Buy(_player);
        }
    }

    public void Deactivate()
    {
        _isActive = false;
    }

    private async Awaitable Buy(Player player)
    {
        if (_item == null || _item.isSold || !_isActive)
        {
            _hint.text = "Уже куплено! ❌";

            return;
        }
        
        var isBought = player.BuyItem(_item);

        if (isBought)
        {
            buySound.Play();
            _onBuy?.Invoke();
            _priceText.text = "Куплено! ";
            _hint.text = "";

            for (var i = 0; i < 3; i++)
            {
                await Awaitable.WaitForSecondsAsync(0.15f);
            
                _priceText.text += "💸";
            }
        }
        else
        {
            _hint.text = "Недостаточно средств! ❌";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var isPlayer = other.CompareTag("Player");
        
        if (isPlayer)
        {
            if (!_player) _player = other.GetComponent<Player>();
            
            _isWithinRange = true;
            _priceTag.style.display = DisplayStyle.Flex;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        var isPlayer = other.CompareTag("Player");
        
        if (isPlayer)
        {
            _isWithinRange = false;
            _priceTag.style.display = DisplayStyle.None;
        }
    }
}
