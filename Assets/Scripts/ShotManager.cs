using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ShotManager : MonoBehaviour
{
    private static ShotManager _instance;
    public static ShotManager Instance
    {
        get
        {
            if (!_instance)
            {
                // Try to find an existing instance in the scene
                _instance = FindFirstObjectByType<ShotManager>();

                // If not found, create a new GameObject and add the component
                if (!_instance)
                {
                    GameObject singletonObject = new GameObject("ShotManager");
                    _instance = singletonObject.AddComponent<ShotManager>();
                    Debug.Log("ShotManager instance created.");
                }
            }
            return _instance;
        }
    }

    private Dictionary<int, float> _processedShotIds = new Dictionary<int, float>();
    private int _nextShotId = 0;

    [Header("Cleanup Settings")]
    [Tooltip("How often (in seconds) the cleanup routine should run.")]
    public float cleanupInterval = 5.0f;

    [Tooltip("How long (in seconds) a Shot ID should be tracked before being automatically removed.")]
    public float shotIdMaxAge = 10.0f; // e.g., remove IDs after 1 minute

    void Awake()
    {
        // Ensure only one instance exists (part of Singleton pattern)
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        // Start the automatic cleanup routine
        StartCoroutine(CleanupRoutine());
    }

    void OnDestroy()
    {
        // Stop the coroutine when the object is destroyed
        StopAllCoroutines();
    }
    
    public bool TryProcessHit(int shotId)
    {
        if (!_processedShotIds.ContainsKey(shotId))
        {
            // ID not found, add it with the current time and return true
            _processedShotIds.Add(shotId, Time.time); // Use Time.time or Time.unscaledTime depending on your needs
            return true;
        }
        // ID already exists, return false
        return false;
    }

    /// <summary>
    /// Coroutine that runs periodically to remove old Shot IDs.
    /// </summary>
    private IEnumerator CleanupRoutine()
    {
        while (true) // Loop indefinitely while the ShotManager exists
        {
            yield return new WaitForSeconds(cleanupInterval); // Wait for the specified interval
            CleanupOldIds();
        }
    }

    /// <summary>
    /// Removes Shot IDs older than shotIdMaxAge.
    /// </summary>
    private void CleanupOldIds()
    {
        if (_processedShotIds.Count == 0) return; // Nothing to clean

        float currentTime = Time.time;
        // Create a list of IDs to remove because we can't modify the dictionary while iterating directly
        List<int> idsToRemove = new List<int>();

        foreach (KeyValuePair<int, float> entry in _processedShotIds)
        {
            if (currentTime - entry.Value > shotIdMaxAge)
            {
                idsToRemove.Add(entry.Key);
            }
        }

        // Remove the identified old IDs
        if (idsToRemove.Count > 0)
        {
            foreach (int id in idsToRemove)
            {
                _processedShotIds.Remove(id);
            }
            Debug.Log($"ShotManager: Cleaned up {idsToRemove.Count} old Shot IDs.");
        }
    }
    /// <summary>
    /// Generates a unique ID for a new shot. Call this ONCE per shot event (e.g., shotgun blast).
    /// </summary>
    /// <returns>A unique integer Shot ID.</returns>
    public int GenerateNewShotId()
    {
        _nextShotId++;
        // Basic overflow check (unlikely to hit in most games, but good practice)
        if (_nextShotId == int.MaxValue)
        {
             _nextShotId = 1; // Reset or handle differently if needed
             // Consider clearing the _processedShotIds or using a different ID system (like Guid) if wrap-around is a concern.
             Debug.LogWarning("Shot ID counter wrapped around!");
        }
        return _nextShotId;
    }

    /// <summary>
    /// Optional: Call this periodically or when necessary to clean up old IDs
    /// if memory usage becomes a concern over very long play sessions.
    /// For most games, this might not be needed unless you fire millions of shots
    /// without restarting or changing scenes.
    /// </summary>
    public void ClearProcessedIds()
    {
        _processedShotIds.Clear();
        Debug.Log("Cleared processed Shot IDs.");
    }

    // Optional: Method to remove a specific ID if needed (e.g., after a delay)
    public void RemoveProcessedId(int shotId)
    {
        _processedShotIds.Remove(shotId);
    }
}