using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyInstance enemyPrefab;
    private readonly List<EnemyInstance> _instances = new List<EnemyInstance>();
    [CanBeNull] private Action<EnemyInstance> _cachedOnSpawn;
    private Awaitable _currentAction;
    
    public ReadOnlyCollection<EnemyInstance> enemies => this._instances.AsReadOnly();

    private bool _isActive; 

    private void Start()
    {
        if (!enemyPrefab)
        {
            enemyPrefab = Resources.Load<EnemyInstance>("Prefabs/EnemyInstance");
        }
    }

    public async Awaitable SpawnEnemies(Action<EnemyInstance> onSpawn)
    {
        _isActive = true;
        _cachedOnSpawn = onSpawn;
        
        while (_isActive)
        {
            var randomInterval = Random.Range(5f, 7f);
            
            _currentAction = Awaitable.WaitForSecondsAsync(randomInterval);
            
            await _currentAction;
            
            SpawnEnemy(onSpawn);
        }
    }
    
    private void StopSpawning()
    {
        _isActive = false;
        _currentAction.Cancel();
    }
 
    private void SpawnEnemy(Action<EnemyInstance> onSpawn)
    {
        var position = GameField.instance.GetRandomPointWithin();
        
        var enemy = Instantiate(enemyPrefab, position, transform.rotation);
        
        _instances.Add(enemy);

        onSpawn(enemy);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        var isPlayer = other.CompareTag("Player");

        if (isPlayer && !_isActive)
        {
            _ = SpawnEnemies(_cachedOnSpawn);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        var isPlayer = other.CompareTag("Player");

        if (isPlayer && _isActive)
        {
            StopSpawning();
        }
    }
}
