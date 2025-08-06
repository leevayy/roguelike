using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyInstance commonEnemyPrefab;
    [SerializeField] private EnemyInstance rusherEnemyPrefab;

    private readonly List<EnemyInstance> _instances = new List<EnemyInstance>();
    [CanBeNull] private Action<EnemyInstance> _onSpawnAfterInitialize;
    [CanBeNull] private Action<EnemyInstance> _onSpawnBeforeInitialize;

    private Awaitable _currentAction;
    
    public ReadOnlyCollection<EnemyInstance> enemies => _instances.AsReadOnly();

    private bool _isActive;
    private bool _shouldSpawn;
    private bool _playerInside;

    // Step 1: Initialize the spawner, but do not start spawning
    public void SpawnEnemies(Action<EnemyInstance> onSpawn)
    {
        _shouldSpawn = true;
        _onSpawnAfterInitialize = onSpawn;
    }

    public void SetOnSpawnBeforeInitialize(Action<EnemyInstance> onSpawn)
    {
        _onSpawnBeforeInitialize = onSpawn;
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
                SpawnEnemy(_onSpawnAfterInitialize, _onSpawnBeforeInitialize);
            }
        }

        _isActive = false;
    }

    [ContextMenu(nameof(StopSpawning))] public void StopSpawning()
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

    private EnemyInstance GetRandomEnemyPrefab()
    {
        var enemyPrefabs = new List<EnemyInstance>
        {
            commonEnemyPrefab,
            rusherEnemyPrefab
        };
        
        return enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
    }

    [ContextMenu(nameof(SpawnEnemy))] private void SpawnEnemy(Action<EnemyInstance> onSpawnAfterInitialize, Action<EnemyInstance> onSpawnBeforeInitialize)
    {
        var position = GameField.current.GetRandomPointWithin();
        var enemy = Instantiate(GetRandomEnemyPrefab(), position, transform.rotation);

        onSpawnBeforeInitialize?.Invoke(enemy);

        enemy.Initialize();

        onSpawnAfterInitialize?.Invoke(enemy);

        _instances.Add(enemy);

        enemy.onDispose = () => _instances.Remove(enemy);
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
