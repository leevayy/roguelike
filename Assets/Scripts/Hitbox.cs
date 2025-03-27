using System;
using JetBrains.Annotations;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [CanBeNull] private Action<Collider, Hitbox> onTriggerEnterHandler;

    public void SetOnTriggerEnterHandler(Action<Collider, Hitbox> handler)
    {
        onTriggerEnterHandler = handler;
    }

    public void RemoveHitbox()
    {
        Destroy(gameObject);
    }
        
    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnterHandler?.Invoke(other, this);
    }
}
