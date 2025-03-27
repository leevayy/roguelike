using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleRuntimeUI : MonoBehaviour
{
    public static SimpleRuntimeUI instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private Button button;
    private TextElement scoreText;

    private bool _isMenuOpen;
    private bool isMenuOpen
    {
        get => _isMenuOpen;
        set
        {
            if (_isMenuOpen == value) return;

            _isMenuOpen = value;
            UpdateButton(_isMenuOpen);
        }
    }
    
    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();
    
        button = uiDocument.rootVisualElement.Q("menu-button") as Button;

        button?.RegisterCallback<ClickEvent>(evt =>
        {
            isMenuOpen = !isMenuOpen;
            evt.StopPropagation();
        });
        
        scoreText = uiDocument.rootVisualElement.Q("score") as TextElement;
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    private void UpdateButton(bool isOpen)
    {
        if (isOpen)
        {
            button?.AddToClassList("menu-open");
        }
        else
        {
            button?.RemoveFromClassList("menu-open");
        }
    }
}