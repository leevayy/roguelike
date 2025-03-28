using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform uiTransform;
    [SerializeField] private EnemyInstance enemyInstance;

    private VisualElement _healthBar;
    private UIDocument _baseContainerDocument;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void Init(VisualElement healthBar, UIDocument baseContainerDocument)
    {
        _healthBar = healthBar;
        _baseContainerDocument = baseContainerDocument;
        
        enemyInstance.onHealthPointsChange = UpdateHealth;
    }

    private void Update()
    {
        SetHealthBarPosition();
    }

    private void SetHealthBarPosition()
    {
        var screenPosition = cam.WorldToScreenPoint(uiTransform.transform.position);

        var newPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
        
        var panelPosition = RuntimePanelUtils.ScreenToPanel(
            _baseContainerDocument.rootVisualElement.panel,
            newPosition
        );
        
        _healthBar.style.position = Position.Absolute;
        _healthBar.style.left = panelPosition.x;
        _healthBar.style.top = panelPosition.y;
        
        var cursorPosition = Input.mousePosition;
        var distance = Vector2.Distance(new Vector2(screenPosition.x, screenPosition.y), cursorPosition);

        _healthBar.style.display = distance <= 200 ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateHealth(float health)
    {
        if (_healthBar == null) return;

        _healthBar.Q<Label>("health-tag").text = health > 0 ? health.ToString(CultureInfo.CurrentCulture) : "💀";
    }
}
