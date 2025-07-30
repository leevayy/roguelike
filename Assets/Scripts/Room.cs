using System;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private int RoomNumber = -1; 
    public Action<int> OnPlayerRoomEnter { private get; set; }
    
    private void OnTriiggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerRoomEnter?.Invoke(RoomNumber);
        }
    }
}
