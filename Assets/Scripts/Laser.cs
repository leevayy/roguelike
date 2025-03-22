using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Laser : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    
    private void Start()
    {
        // Destroy the laser after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate( speed * Time.deltaTime * Vector3.forward);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision with {collision.gameObject.name}");
        Destroy(gameObject);
    }
}
