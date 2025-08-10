using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using utility;

public class Menu : MonoBehaviour
{
    private UIDocument _document;

    private TextElement _playButton;

    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        _document = GetComponent<UIDocument>();

        _playButton = _document.rootVisualElement.Q<Button>("play");

        _playButton.RegisterCallback<ClickEvent>((evt =>
        {
            OnPlay();
        }));
    }

    private void OnPlay()
    {
        SceneManager.LoadScene("Scenes/SampleScene");
    }
}
