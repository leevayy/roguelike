using UnityEngine;

public class Duck : MonoBehaviour
{
    public float speed = 5f;
    public float zigzagWidth = 20f;
    public float zigzagSpeed = 5f;
    private Vector3 _direction;
    private float _time;
    
    private float _leftLimit, _rightLimit, _lowerLimit, _upperLimit;

    private void Start()
    {
        var limits = GameField.current.GetLimits();
        _leftLimit = limits.Left;
        _rightLimit = limits.Right;
        _lowerLimit = limits.Lower;
        _upperLimit = limits.Upper;
        _direction = (Vector3.right + Vector3.forward).normalized;
    }

    private void Update()
    {
        ZigZagMove();
    }

    private void ZigZagMove()
    {
        // Define how close to the boundary before tweaking the angle (adjust as needed)
        float threshold = 0.5f;
        // Minimum allowed rotation angle (to avoid overshooting when very close)
        float minAngle = 15f;
        // Baseline rotation angle when far from boundaries
        float baseAngle = 45f;

        // Compute the proposed next position
        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = currentPosition + _direction * (speed * Time.deltaTime);
        
        transform.LookAt(nextPosition);

        // Determine if we're near any boundary and calculate the minimum distance to a boundary
        float minDistance = float.MaxValue;
        bool nearBoundary = false;

        // Check X-axis boundaries
        if (nextPosition.x <= _leftLimit + threshold)
        {
            nearBoundary = true;
            minDistance = Mathf.Min(minDistance, Mathf.Abs(nextPosition.x - _leftLimit));
        }
        else if (nextPosition.x >= _rightLimit - threshold)
        {
            nearBoundary = true;
            minDistance = Mathf.Min(minDistance, Mathf.Abs(_rightLimit - nextPosition.x));
        }

        // Check Z-axis boundaries (assuming _lowerLimit and _upperLimit correspond to Z)
        if (nextPosition.z <= _lowerLimit + threshold)
        {
            nearBoundary = true;
            minDistance = Mathf.Min(minDistance, Mathf.Abs(nextPosition.z - _lowerLimit));
        }
        else if (nextPosition.z >= _upperLimit - threshold)
        {
            nearBoundary = true;
            minDistance = Mathf.Min(minDistance, Mathf.Abs(_upperLimit - nextPosition.z));
        }

        // If near a boundary, compute a dynamic angle
        if (nearBoundary)
        {
            // Scale the rotation angle relative to how close we are.
            // When minDistance equals threshold, we use the baseAngle.
            // When minDistance is 0, we use the minAngle.
            float dynamicAngle = Mathf.Lerp(minAngle, baseAngle, minDistance / threshold);

            // Rotate the direction by the computed dynamic angle around the Y axis.
            _direction = Quaternion.Euler(0, dynamicAngle, 0) * _direction;

            // Recalculate the next position with the updated direction
            nextPosition = currentPosition + _direction * (speed * Time.deltaTime);
        }

        // Update the position
        transform.position = nextPosition;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemyScript = collision.gameObject.GetComponentInParent<EnemyInstance>();

            if (!enemyScript.isAlive)
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
