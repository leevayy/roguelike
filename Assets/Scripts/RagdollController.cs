using UnityEngine;
using System.Collections.Generic;

public class RagdollController : MonoBehaviour
{
    public Animator characterAnimator;
    public Rigidbody mainDeathForceRigidbody;

    private List<Rigidbody> _ragdollRigidbodies = new List<Rigidbody>();
    private List<Collider> _ragdollColliders = new List<Collider>();

    private void Awake()
    {
        if (characterAnimator == null)
        {
            characterAnimator = GetComponent<Animator>();
        }

        _ragdollRigidbodies.AddRange(GetComponentsInChildren<Rigidbody>());
        _ragdollColliders.AddRange(GetComponentsInChildren<Collider>());

        SetRagdollState(false);
    }

    private void EnableRagdoll()
    {
        SetRagdollState(true);
    }

    private void SetRagdollState(bool enableRagdoll)
    {
        if (characterAnimator != null)
        {
            characterAnimator.enabled = !enableRagdoll;
        }

        foreach (Rigidbody rb in _ragdollRigidbodies)
        {
            if (rb != null && rb.CompareTag("Ragdoll"))
            {
                rb.isKinematic = !enableRagdoll;
                rb.useGravity = enableRagdoll;
            }
        }

        foreach (Collider col in _ragdollColliders)
        {
            if (col != null && col.CompareTag("Ragdoll"))
            {
                col.enabled = enableRagdoll;
            }
        }
    }

    public void Die(Vector3 forceDirection = default, float forceMagnitude = 0f)
    {
        EnableRagdoll();

        if (forceMagnitude > 0f)
        {
            Rigidbody targetRigidbody = mainDeathForceRigidbody; 
            if (targetRigidbody == null && _ragdollRigidbodies.Count > 0)
            {
                targetRigidbody = _ragdollRigidbodies[0];
            }

            if (targetRigidbody != null)
            {
                targetRigidbody.AddForce(forceDirection.normalized * forceMagnitude, ForceMode.Impulse);
            }
        }
    }
}