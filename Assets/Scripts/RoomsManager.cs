using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    private List<Room> _rooms = new();
    private Player _player;

    private void Awake()
    {
        _player = FindFirstObjectByType<Player>();
    }
    
    private void Start()
    {
        _rooms = FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
    }
}