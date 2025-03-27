using UnityEngine;

public class EnemyInstance : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    private bool isAlive = true;

    private void Awake()
    {
        enemy.GetHitbox().SetOnTriggerEnterHandler((other, hitbox) =>
        {
            if (!isAlive)
            {
                return;
            }
        
            var isHitByAlly = other.CompareTag("AllyProjectile");
        
            if (isHitByAlly)
            {
                var collisionPoint = other.GetComponent<Collider>().ClosestPoint(enemy.transform.position);
                
                GameManager.instance.OnHit(HitTarget.Ally, HitTarget.Enemy, collisionPoint);
            
                isAlive = false;
                
                enemy.PickTarget(null);
                
                enemy.StopMoving();
                
                hitbox.enabled = false;

                var enemyRigidbody = enemy.GetComponent<Rigidbody>();

                enemyRigidbody.constraints = RigidbodyConstraints.None;

                var hitDirection = other.GetComponent<Rigidbody>().linearVelocity.normalized;
                
                enemyRigidbody.AddForceAtPosition(hitDirection, enemy.transform.position + Vector3.up, ForceMode.Impulse);
                
                var rotationAxis = Vector3.Cross(hitDirection, Vector3.up) * -1;
                
                enemyRigidbody.AddTorque(rotationAxis, ForceMode.Force);
            }
        });
    }

    public void FocusEntity(GameObject entity)
    {
        if (!enemy)
        {
            return;
        }

        enemy.StartMoving();
        enemy.PickTarget(entity);
    }
}
