using System;
using UnityEngine;

public class SafeFallingPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy")) return;
        
        other.transform.position = GameField.current.GetRandomPointWithin() + Vector3.up * 2f;
    }
}
