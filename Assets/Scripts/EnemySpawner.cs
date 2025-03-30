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
    private bool _shouldSpawn;

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
        _shouldSpawn = true;
        _cachedOnSpawn = onSpawn;

        while (_isActive)
        {
            var randomInterval = Random.Range(5f, 7f) / GameManager.instance.GetGoalNumber();

            try {

                _currentAction = Awaitable.WaitForSecondsAsync(randomInterval);
            
                await _currentAction;
            }
            // can be cancelled so it's okay
            catch (OperationCanceledException)
            {
                break;
            }
            
            SpawnEnemy(onSpawn);
        }
    }
    
    public void StopSpawning()
    {
        _shouldSpawn = false;
        InternalStopSpawning();
    }
    
    private void InternalStopSpawning()
    {
        _isActive = false;
        
        if (_currentAction == null) return;
        
        if (!_currentAction.IsCompleted)
        {
            _currentAction.Cancel();
        }
    }
 
    private void SpawnEnemy(Action<EnemyInstance> onSpawn)
    {
        var position = GameField.current.GetRandomPointWithin();
        
        var enemy = Instantiate(enemyPrefab, position, transform.rotation);
        
        _instances.Add(enemy);

        enemy.onDispose = () =>
        {
            _instances.Remove(enemy);
        };

        onSpawn(enemy);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!_shouldSpawn) return;
        
        var isPlayer = other.CompareTag("Player");

        if (isPlayer && !_isActive)
        {
            StartCoroutine(SpawnEnemies(_cachedOnSpawn));
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!_shouldSpawn) return;
        
        var isPlayer = other.CompareTag("Player");

        if (isPlayer && _isActive)
        {
            InternalStopSpawning();
        }
    }
}
