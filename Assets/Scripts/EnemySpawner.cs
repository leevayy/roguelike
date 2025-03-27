using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyInstance enemyPrefab;
    private readonly List<EnemyInstance> instances = new List<EnemyInstance>();
    public ReadOnlyCollection<EnemyInstance> enemies => this.instances.AsReadOnly();

    private bool isActive; 

    private void Start()
    {
        if (!enemyPrefab)
        {
            enemyPrefab = Resources.Load<EnemyInstance>("Prefabs/EnemyInstance");
        }
    }

    public async Awaitable SpawnEnemies(Action<EnemyInstance> onSpawn)
    {
        isActive = true;
        
        while (isActive)
        {
            var randomInterval = Random.Range(5f, 7f);
            
            await Awaitable.WaitForSecondsAsync(randomInterval);
            
            SpawnEnemy(onSpawn);
        }
    }
 
    private void SpawnEnemy(Action<EnemyInstance> onSpawn)
    {
        var position = GameField.instance.GetRandomPointWithin();
        
        var enemy = Instantiate(enemyPrefab, position, transform.rotation);
        
        instances.Add(enemy);

        onSpawn(enemy);
    }
}
