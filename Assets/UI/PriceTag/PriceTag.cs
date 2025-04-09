using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class PriceTag : MonoBehaviour
{
    [SerializeField] public BuyBox buyBox;
    [SerializeField] private VisualTreeAsset priceTemplate;
    [SerializeField] private UIDocument baseContainerDocument;

    private StoreItem _item;
    private Camera _cam;
    private TemplateContainer _price;
    private VisualElement _baseContainer;
    
    private void Awake()
    {
        _cam = Camera.main;
        
        _baseContainer = baseContainerDocument.rootVisualElement.Q<VisualElement>("BaseContainer");
    }

    private void Update()
    {
        var screenPosition = _cam.WorldToScreenPoint(transform.position);
        
        var panelPosition = RuntimePanelUtils.ScreenToPanel(
            baseContainerDocument.rootVisualElement.panel,
            new Vector2(screenPosition.x, Screen.height - screenPosition.y)
        );

        _price.style.position = Position.Absolute;
        _price.style.left = panelPosition.x;
        _price.style.top = panelPosition.y - 20f;
    }

    public void OnEnable()
    {
        if (_cam == null) return;

        _price = priceTemplate.Instantiate();
        
        _baseContainer.Add(_price);
        
        _price.style.display = DisplayStyle.None;
    }
    
    public void Init(StoreItem item, Action onBuy)
    {
        buyBox.Init(_price, item, onBuy);
    }
    
    public void Init(bool isElevatorButton, Action onElevatorButtonPressed)
    {
        buyBox.Init(_price, isElevatorButton, onElevatorButtonPressed);
    }
}
