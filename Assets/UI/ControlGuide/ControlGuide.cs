using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using utility;

public class ControlGuide : MonoBehaviour
{
    public static ControlGuide instance { get; private set; }
    private UIDocument _document;
    
    private VisualElement _hints;

    private TextElement _wasdText;
    private TextElement _rmbText;
    private TextElement _spaceText;
    private TextElement _pText;
    private TextElement _fText;
    private TextElement _gText;
    
    private bool _isWasdActive = true;
    private bool _isRmbActive = true;
    private bool _isSpaceActive = true;
    private bool _isPActive = true;
    private bool _isFActive = true;
    private bool _isGActive = true;

    public bool isActive
    {
        get
        {
            var next = _isWasdActive || _isRmbActive || _isSpaceActive || _isPActive || _isFActive || _isGActive;

            if (next == false)
            {
                _ = RemoveHints();
            }
            
            return next;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    
    
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        _document = GetComponent<UIDocument>();
        
        _hints = _document.rootVisualElement.Q<VisualElement>("hints");
        _wasdText = _document.rootVisualElement.Q<TextElement>("wasd");
        _rmbText = _document.rootVisualElement.Q<TextElement>("rmb");
        _spaceText = _document.rootVisualElement.Q<TextElement>("space");
        _pText = _document.rootVisualElement.Q<TextElement>("p");
        _fText = _document.rootVisualElement.Q<TextElement>("fe");
        _gText = _document.rootVisualElement.Q<TextElement>("g");
    }

    public void CompletedWasd()
    {
        if (!_isWasdActive)
        {
            return;
        }
        
        _isWasdActive = false;
        _wasdText.text += "✅";
        _wasdText.AddToClassList("success");
    }

    public void CompletedRmb()
    {
        if (!_isRmbActive)
        {
            return;
        }
        
        _isRmbActive = false;
        _rmbText.text +=  "✅";
        _rmbText.AddToClassList("success");
    }
    
    public void CompletedSpace()
    {
        if (!_isSpaceActive)
            return;

        _isSpaceActive = false;
        _spaceText.text += "✅";
        _spaceText.AddToClassList("success");
    }

    public void CompletedP()
    {
        if (!_isPActive)
            return;

        _isPActive = false;
        _pText.text += "✅";
        _pText.AddToClassList("success");
    }

    public void CompletedF()
    {
        if (!_isFActive)
            return;

        _isFActive = false;
        _fText.text += "✅";
        _fText.AddToClassList("success");
    }

    public void CompletedG()
    {
        if (!_isGActive)
            return;

        _isGActive = false;
        _gText.text += "✅";
        _gText.AddToClassList("success");
    }

    private async Awaitable RemoveHints()
    {
        await Awaitable.WaitForSecondsAsync(5f);
        
        _hints.AddToClassList("hidden");
    }
}