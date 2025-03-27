using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Laser : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private Rigidbody rb;
    
    private void Start()
    {
        // Destroy the laser after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        rb.linearVelocity = speed * transform.forward;
    }

    private void OnCollisionEnter()
    {
        Destroy(gameObject);
    }
}
