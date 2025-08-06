using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Laser : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private Rigidbody rb;
    
    public int shotId { get; set; }
    public float damage { get; set; }
    public bool isBurn { get; set; }
    public float ttl { get; set; } = 5f;

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
        Destroy(gameObject, ttl);
    }

    private void Update()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        rb.linearVelocity = speed * transform.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {        
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

        if (isSolid) return;
        
        Destroy(gameObject);
    }
}
