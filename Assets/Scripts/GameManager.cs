using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using utility;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Player player;  
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private DamagePopup damagePopup;
    [SerializeField] private HealthBarManager healthBarManager;
    [SerializeField] private TMP_Text firstFloor;
    [SerializeField] private TMP_Text secondFloor;
    [SerializeField] private GameObject rubberDuckPrefab;
    [FormerlySerializedAs("hit")] [SerializeField] private AudioSource hitSound;
    [SerializeField] private AudioSource takeDamageSound;
    [SerializeField] private AudioSource takeDamage2Sound;
    [SerializeField] private AudioSource winSound;
    
    private GameObject _duck; 
    private readonly Vector3 _defaultMoneyBagScale = new(0.2f, 0.5f, 0.5f);
    private int _goalNumber = 0;

    private int _topDamage = 0;
    private int _totalKills = 0;
    private int _killCount = 0;
    private int killCount
    {
        get => _killCount;
        set
        {
            if (value == 0) _totalKills += _killCount;
                
            _killCount = value;
            
            GameUI.instance.UpdateKillCount(value, GetInterest());

            if (goal.Type == GoalType.KILL_N_ENEMIES && value >= goal.N)
            {
                goal = GetNextGoal(_goalNumber);
            }
        }
    }

    private Goal _goal;
    public Goal goal
    {
        get => _goal;
        private set
        {
            winSound.Play();
            
            if (!_duck && ModManager.instance.HasDuck())
            {
                _duck = Instantiate(rubberDuckPrefab, transform);    
            }

            if (_goalNumber == 9)
            {
                ShowGameOverScreen(true);
            }

            player.Heal();
            
            _goal = value;
            _goalNumber++;
            killCount = 0;
            
            GameUI.instance.UpdateGoal(_goalNumber, value);

            if (_goalNumber > 1)
            {
                enemySpawner.StopSpawning();
                KillAll();
                
                var minPrice = 10f * _goalNumber * 2 + (_goalNumber - 2) * 200;
                ShopManager.instance.RefreshStore(minPrice, minPrice + (_goalNumber - 2) * 300);
                
                _ = DrawToExit();
            }
        }
    }

    private float _timeElapsed;
    private float timeElapsed
    {
        get => _timeElapsed;
        set
        {
            _timeElapsed = value;
            GameUI.instance.UpdateTimer(value);
        }
    }
    
    private int _score;
    public int score
    {
        get => _score;
        set
        {
            if (_score == value) return;
            
            var moneyDiff = value - _score;
            var moneyPosition = new Vector2(60 + Random.Range(0, 15), 15 + Random.Range(0, 15));

            if (moneyDiff > 0)
            {
                StartCoroutine(damagePopup.ShowDamagePopup($"+${moneyDiff}", moneyPosition));
            }
            else
            {
                StartCoroutine(damagePopup.ShowDamagePopup($"-${moneyDiff}", moneyPosition));

            }
            
            _score = value;
            GameUI.instance.UpdateScore(value);

            UpdatePlayerMass(_score);
            
            if (goal.Type == GoalType.GET_SCORE_N && value >= goal.N)
            {
                goal = GetNextGoal(_goalNumber);
            }
        }
    }
    
    public ReadOnlyCollection<Modification> modifications => player.GetModifications();

    public static GameManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        
        Time.timeScale = 1;
    }

    private void Start()
    {
        StartFocus();

        player.moneyBag.transform.localScale = _defaultMoneyBagScale;

        goal = GetNextGoal(_goalNumber);

        firstFloor.gameObject.SetActive(false);
        secondFloor.text = "Купи хотя бы один предмет и возвращайся";
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (player.healthpoints < 0 || (score <= 0 && ModManager.instance.HasMoneyEqualsLife()))
        {
            ShowGameOverScreen(false);
        }
    }

    private void ShowGameOverScreen(bool isWin)
    {
        GameOverScreen.instance.ShowGameOverScreen(isWin, TimeFormatter.FormatTime(timeElapsed), Mathf.Max(_totalKills, _killCount), player.moneySpent, _topDamage);
        Time.timeScale = 0;
    }

    public void OnHit(HitInfo hitInfo, GameHitEntity gameHitEntity, Vector3 hitPoint)
    {
        if (hitInfo.Source == GameHitEntity.Enemy && gameHitEntity == GameHitEntity.Ally)
        {
            var pitch = 0.5f + Random.Range(0f, 1f);

            if (Random.value <= 0.7f)
            {
                takeDamageSound.pitch = pitch;
                takeDamageSound.Play();
            }
            else
            {
                takeDamage2Sound.pitch = pitch;
                takeDamage2Sound.Play();
            }
            
            var moneyLost = Mathf.Max((int)hitInfo.Damage / 10, 8);

            score -= moneyLost;
        }

        if (hitInfo.Source == GameHitEntity.Ally && gameHitEntity == GameHitEntity.Enemy)
        {
            _topDamage = Mathf.Max(_topDamage, (int)hitInfo.Damage);
            
            hitSound.Play();
            
            score += GetInterest();

            StartCoroutine(damagePopup.ShowDamagePopup(hitInfo.Damage, hitPoint));
        }
    }
    
    public void OnKill()
    {
        score += GetInterest() + Random.Range(7, 9);
        
        killCount++;
    }

    private int GetInterest()
    {
        var interest = (int)Math.Floor(score / 100f);
        var interestPerKill = (int)Math.Floor(_killCount / 5f);

        var moneyGained = interestPerKill + interest;
        
        return Mathf.Max(moneyGained, 0);
    }
    
    public void OnBuy()
    {
        StartFocus();
        firstFloor.gameObject.SetActive(false);
    }
    
    public ReadOnlyCollection<Modification> OnDrop(out Transform playerTransform)
    {
        playerTransform = player.transform;
        
        return player.DropModifications();
    }

    private void UpdatePlayerMass(float scoreValue)
    {
        var mass = scoreValue / 500f;
        
        var playerRb = player.GetComponent<Rigidbody>();
            
        playerRb.mass = Mathf.Max(1 + mass, 0.5f);

        player.additionalSpeed = -1 * mass;

        var scaleAddition = mass <= 4f 
            ? new Vector3(Mathf.Max(0.1f * mass, -1f), 0, 0)
            : new Vector3(0.4f, 0, 0) + new Vector3(0.05f, 0.05f, 0.1f) * mass;
        
        player.moneyBag.transform.localScale = _defaultMoneyBagScale + scaleAddition;
    }

    private Goal GetNextGoal(int goalNumber)
    {
        var goalType = (GoalType)Random.Range(0, Enum.GetValues(typeof(GoalType)).Length);
        var nextGoalNumber = goalNumber + 1;
        
        switch (goalType)
        {
            case GoalType.KILL_N_ENEMIES:
                const int startN = 5;
                const int endN = 7;
                
                return new Goal(GoalType.KILL_N_ENEMIES, Random.Range(startN, endN) * nextGoalNumber);
            case GoalType.GET_SCORE_N:
                const int startScore = 40;
                const int endScore = 50;
                
                return new Goal(GoalType.GET_SCORE_N, score + Random.Range(startScore, endScore) * nextGoalNumber);
            
            default:
                return new Goal();
        }
    }

    public int GetGoalNumber()
    {
        return Mathf.Max(_goalNumber, 1);
    }

    private async Awaitable DrawToExit()
    {
        firstFloor.gameObject.SetActive(true);
        firstFloor.SetText("лифт =>");

        for (var i = 0; i < 6; i++)
        {
            await Awaitable.WaitForSecondsAsync(0.15f);
            firstFloor.text += ">";
        }
    }

    private void StartFocus()
    {
        _ = enemySpawner.SpawnEnemies(enemy =>
        {
            healthBarManager.AddHealthBar(100, enemy.gameObject);
            enemy.FocusEntity(player.gameObject);
            enemy.SetLevel(_goalNumber);
        });
    }
    
    [ContextMenu(nameof(KillAll))] private void KillAll()
    {
        foreach (var enemy in enemySpawner.enemies)
        {
            enemy.Kill();
        }
    }
    
    [ContextMenu(nameof(GetRandomMod))] private void GetRandomMod()
    {
        player.BuyModification(new StoreItem(new Modification(), 0));
    }
}
