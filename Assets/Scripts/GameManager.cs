using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;  
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private DamagePopup damagePopup;

    private int _score;
    public int score
    {
        get => _score;
        private set
        {
            _score = value;
            SimpleRuntimeUI.instance.UpdateScore(value);
        }
    }

    public static GameManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        StartFocus();
    }

    public void OnHit(HitTarget hitSource, HitTarget hitTarget, Vector3 hitPoint)
    {
        if (hitSource == HitTarget.Enemy && hitTarget == HitTarget.Ally)
        {
            score--;
        }

        if (hitSource == HitTarget.Ally && hitTarget == HitTarget.Enemy)
        {
            damagePopup.ShowDamagePopup(999, hitPoint);
            score++;
        }
    }

    [ContextMenu(nameof(StartFocus))] private void StartFocus()
    {
        _ = enemySpawner.SpawnEnemies(enemy => enemy.FocusEntity(player));
    }
}
