using UnityEngine;
using utility;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Player player;  
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private DamagePopup damagePopup;
    [SerializeField] private HealthBarManager healthBarManager;

    private readonly Vector3 _defaultMoneyBagScale = new(0.2f, 0.5f, 0.5f);
    private int _score;
    public int score
    {
        get => _score;
        private set
        {
            _score = value;
            SimpleRuntimeUI.instance.UpdateScore(value);

            UpdatePlayerMass(Mathf.Floor(_score / 50f));
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

        player.moneyBag.transform.localScale = _defaultMoneyBagScale;
    }

    public void OnHit(HitInfo hitInfo, GameHitEntity gameHitEntity, Vector3 hitPoint)
    {
        if (hitInfo.Source == GameHitEntity.Enemy && gameHitEntity == GameHitEntity.Ally)
        {
            score--;
        }

        if (hitInfo.Source == GameHitEntity.Ally && gameHitEntity == GameHitEntity.Enemy)
        {
            score++;
            damagePopup.ShowDamagePopup(hitInfo.Damage, hitPoint);
        }
    }

    private void UpdatePlayerMass(float mass)
    {
        var playerRb = player.GetComponent<Rigidbody>();
            
        playerRb.mass = Mathf.Max(1 + mass, 0.5f);

        player.additionalSpeed = -1 * mass;

        var scaleAddition = mass <= 4f 
            ? new Vector3(Mathf.Max(0.1f * mass, -1f), 0, 0)
            : new Vector3(0.4f, 0, 0) + new Vector3(0.5f, 0.5f, 1f) * mass;
        
        player.moneyBag.transform.localScale = _defaultMoneyBagScale + scaleAddition;
    }

    private void StartFocus()
    {
        _ = enemySpawner.SpawnEnemies(enemy =>
        {
            healthBarManager.AddHealthBar(100, enemy.gameObject);
            enemy.FocusEntity(player.gameObject);
        });
    }
    
    [ContextMenu(nameof(AddMod))] private void AddMod()
    {
        player.AddModification(new Modification());
    }
}
