using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    private UIDocument _document;
    
    private VisualElement _container;
    private Button _resumeButton;
    private Button _restartButton;
    
    private bool _isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (_isPaused)
                OnResume();
            else
                OnPause();
        }
    }

    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        _document = GetComponent<UIDocument>();
        
        _document.rootVisualElement.pickingMode = PickingMode.Position;
        
        _container = _document.rootVisualElement.Q<VisualElement>("container");
        
        _resumeButton = _document.rootVisualElement.Q<Button>("resume");
        
        _restartButton = _document.rootVisualElement.Q<Button>("restart");
        
        _resumeButton.clicked += OnResume;
        _restartButton.clicked += OnRestart;
    }

    private void OnPause()
    {
        Time.timeScale = 0;
        _isPaused = true;
        _container.RemoveFromClassList("hidden");
    }

    private void OnResume()
    {
        Time.timeScale = 1;
        _isPaused = false;
        _container.AddToClassList("hidden");
    }

    private void OnRestart()
    {
        OnResume();
        SceneManager.LoadScene("Scenes/SampleScene");
    }
}