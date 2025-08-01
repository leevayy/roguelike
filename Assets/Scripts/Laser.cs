using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Laser : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private Rigidbody rb;
    
    public int shotId { get; set; }
    public float damage { get; set; }
    public bool isBurn { get; set; }

    private bool _isSolid;
    public bool isSolid
    {
        get => _isSolid;
        set
        {
            _isSolid = value;

            if (_isSolid)
            {
                GetComponent<Collider>().isTrigger = true;
            }
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        rb.linearVelocity = speed * transform.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isSolid) return;
        
        if (collision.gameObject.CompareTag("AllyProjectile") || collision.gameObject.CompareTag("EnemyProjectile"))
        {
            var laser = collision.gameObject.GetComponent<Laser>();

            if (laser.shotId == shotId)
            {
                return;
            }
        }

        if (collision.gameObject.CompareTag("IgnoreAllyProjectile") && gameObject.CompareTag("AllyProjectile"))
        {
            return;
        }
        
        Destroy(gameObject);
    }
}
