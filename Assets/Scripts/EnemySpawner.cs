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
    
    public ReadOnlyCollection<EnemyInstance> enemies => _instances.AsReadOnly();

    private bool _isActive;
    private bool _shouldSpawn;
    private bool _playerInside;

    private void Start()
    {
        if (!enemyPrefab)
        {
            enemyPrefab = Resources.Load<EnemyInstance>("Prefabs/EnemyInstance");
        }
    }

    // Step 1: Initialize the spawner, but do not start spawning
    public void SpawnEnemies(Action<EnemyInstance> onSpawn)
    {
        _shouldSpawn = true;
        _cachedOnSpawn = onSpawn;
    }

    // Step 2: Spawning loop (called only when player is inside)
    private async Awaitable StartSpawningLoop()
    {
        _isActive = true;

        while (_isActive && _playerInside)
        {
            var goalNumber = GameManager.instance.GetGoalNumber();
            var randomInterval = Random.Range(5f, 7f) / (0.5f * goalNumber);

            if (goalNumber == 1)
            {
                randomInterval = 4.2f;
            }

            try
            {
                _currentAction = Awaitable.WaitForSecondsAsync(randomInterval);
                await _currentAction;
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (_playerInside) // Ensure player is still inside
            {
                SpawnEnemy(_cachedOnSpawn);
            }
        }

        _isActive = false;
    }

    public void StopSpawning()
    {
        _shouldSpawn = false;
        InternalStopSpawning();
    }

    private void InternalStopSpawning()
    {
        _isActive = false;

        if (_currentAction != null && !_currentAction.IsCompleted)
        {
            _currentAction.Cancel();
        }
    }

    private void SpawnEnemy(Action<EnemyInstance> onSpawn)
    {
        var position = GameField.current.GetRandomPointWithin();
        var enemy = Instantiate(enemyPrefab, position, transform.rotation);

        _instances.Add(enemy);

        enemy.onDispose = () => _instances.Remove(enemy);

        onSpawn?.Invoke(enemy);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_shouldSpawn) return;
        if (!other.CompareTag("Player")) return;

        _playerInside = true;

        if (!_isActive)
        {
            _ = StartSpawningLoop(); // Start spawning loop only when player enters
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_shouldSpawn) return;
        if (!other.CompareTag("Player")) return;

        _playerInside = false;

        if (_isActive)
        {
            InternalStopSpawning();
        }
    }
}
