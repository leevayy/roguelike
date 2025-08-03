using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using utility;

public class GameUI : MonoBehaviour
{
    public static GameUI instance { get; private set; }

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
    
    private TextElement _scoreText;
    private TextElement _timerText;
    private TextElement _goalText;
    private TextElement _hpText;
    private TextElement _killText;
    private List<TextElement> _mods = new();
    
    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        _document = GetComponent<UIDocument>();
        
        _scoreText = _document.rootVisualElement.Q("score") as TextElement;
        
        _timerText = _document.rootVisualElement.Q("timer") as TextElement;
        
        _goalText = _document.rootVisualElement.Q("goal") as TextElement;
        
        _hpText = _document.rootVisualElement.Q("hp") as TextElement;
        
        _killText = _document.rootVisualElement.Q("kills") as TextElement;

        for (var i = 0; i < 5; i++)
        {
            _mods.Add(_document.rootVisualElement.Q<TextElement>($"lens-{i}"));
        }
    }

    public void UpdateScore(int score)
    {
        _scoreText.text = $"💵: ${score}";
    }

    public void UpdateTimer(float timeInSeconds)
    {
        _timerText.text = TimeFormatter.FormatTime(timeInSeconds);
    }
    
    public void UpdateHp(int current, int max)
    {
        _hpText.text = $"❤️: {current}/{max}";
    }
    
    public void UpdateKillCount(int killCount, int interest)
    {
        _killText.text = $"☠️: {killCount} (+${interest})";
    }
    
    public void UpdateGoal(int goalNumber, Goal goal)
    {
        if (goalNumber > 1)
        {
            CloneTop();
        }
        
        _goalText.text = $"({goalNumber}/9)" + goal.Type switch
        {
            GoalType.GET_SCORE_N => $"Заработай ${goal.N}",
            GoalType.KILL_N_ENEMIES => $"Убей врагов: {goal.N}",
            _ => "Нет цели"
        };
    }
    
    public void UpdateMods(int lensIndex, string lensName)
    {
        if (_mods[lensIndex] != null)
        {
            _mods[lensIndex].text = lensName;
        }
    }
    
    public void ClearMods()
    {
        foreach (var t in _mods)
        {
            t.text = "_";
        }
    }

    private void CloneTop()
    {
        var historyGoal = _document.rootVisualElement.Q<TextElement>("history-goal");
        var historyTimer = _document.rootVisualElement.Q<TextElement>("history-timer");
        
        historyGoal.text = "<s>" + _goalText.text + "</s>";
        historyTimer.text = _timerText.text;
        
        historyGoal.RemoveFromClassList("hidden");
        historyTimer.RemoveFromClassList("hidden");
    }
}