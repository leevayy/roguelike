using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject[] uiDocuments;

    public static GameOverScreen instance { get; private set; }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            
            return;
        }
        
        instance = this;
    }

    private UIDocument _document;
    
    private VisualElement _container;
    private TextElement _result;
    private TextElement _totalTime;
    private TextElement _totalKills;
    private TextElement _moneySpent;
    private TextElement _topDamage;
    private Button _restartButton;

    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        foreach (var uiDocument in uiDocuments)
        {
            uiDocument.SetActive(false);
        }
        
        // The UXML is already instantiated by the UIDocument component
        _document = GetComponent<UIDocument>();
    
        _document.rootVisualElement.pickingMode = PickingMode.Position;
    
        _container = _document.rootVisualElement.Q<VisualElement>("container");
    
        _result = _container.Q<TextElement>("result");
    
        _totalTime = _container.Q<TextElement>("time");
    
        _totalKills = _container.Q<TextElement>("kills");
    
        _moneySpent = _container.Q<TextElement>("moneySpent");
    
        _topDamage = _container.Q<TextElement>("topDamage");
    
        _restartButton = _document.rootVisualElement.Q<Button>("restart");
    
        _restartButton.clicked += OnRestart;
    
        _document.rootVisualElement.pickingMode = PickingMode.Ignore;
    }

    public void ShowGameOverScreen(bool isWin, string finalTime, int totalKills, int moneySpent, float topDamage)
    {
        _document.rootVisualElement.pickingMode = PickingMode.Position;

        if (isWin)
        {
            _container.AddToClassList("win");
        }
        
        _container.RemoveFromClassList("hidden");
        
        _result.text = isWin ? "Победа" : "Поражение";
        
        _totalTime.text = finalTime;
        
        _totalKills.text = "☠️" + totalKills.ToString();
        
        _moneySpent.text = "💸$" + moneySpent.ToString();
        
        _topDamage.text = "💥" + ((int)topDamage).ToString();
    }

    private void OnRestart()
    {
        Time.timeScale = 1;
        foreach (var uiDocument in uiDocuments)
        {
            uiDocument.SetActive(true);
        }
        SceneManager.LoadScene("Scenes/SampleScene");
    }
}