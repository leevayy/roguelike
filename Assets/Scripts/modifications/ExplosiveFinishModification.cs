using Unity.VisualScripting;
using UnityEngine;
using utility;

public class ExplosiveFinishModification : BaseModification
{
    public override string Name => "Взрывная кончина";
    public override string Description => "Убитые враги взрываются, нанося урон соседним врагам";
    public override Material Material => Resources.Load<Material>("Materials/OtherLens");
    public override Rarity Rarity => Rarity.Uncommon;
    public override void ApplyOnKill(AliveState aliveState)
    {
        // Use player position as fallback (original behavior)
        ApplyOnKill(aliveState, aliveState.Transform.position);
    }

    public void ApplyOnKill(AliveState aliveState, Vector3 enemyPosition)
    {
        var killedPosition = enemyPosition != Vector3.zero ? enemyPosition : aliveState.Transform.position;
        
        // Play explosion sound
        PlayExplosionSound();
        
        // Create explosion visual effect
        CreateExplosionEffect(killedPosition);
        
        var colliders = Physics2D.OverlapCircleAll(killedPosition, 3f);
        
        foreach (var collider in colliders)
        {
            collider.TryGetComponent<Enemy>(out var enemy);

            if (enemy == null) continue;
            
            var enemyInstance = enemy.GetComponentInParent<EnemyInstance>();
            if (enemyInstance != null && enemyInstance.transform.position != killedPosition)
            {
                float explosionDamage = 50f;
                GameManager.instance.OnHit(
                    new HitInfo(GameHitEntity.Ally, explosionDamage, -1),
                    GameHitEntity.Enemy,
                    enemyInstance.transform.position
                );

                Debug.Log($"Explosive Finish: Explosion dealt {explosionDamage} damage to nearby enemy!");
            }
        }
        
        Debug.Log($"Explosive Finish: Enemy exploded at {killedPosition}!");
    }
    
    private void CreateExplosionEffect(Vector3 position)
    {
        // Create explosion visual - orange circle that expands and fades
        var explosionObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosionObject.transform.position = position;
        explosionObject.transform.localScale = Vector3.zero;
        
        // Remove collider so it doesn't interfere with physics
        Object.Destroy(explosionObject.GetComponent<Collider>());
        
        // Make it orange
        var renderer = explosionObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0.5f, 0f, 0.7f); // Orange with transparency
        }
        
        // Animate expansion and destruction
        AnimateExplosion(explosionObject);
    }
    
    private async void AnimateExplosion(GameObject explosionObject)
    {
        const float duration = 1f;
        const float maxScale = 6f; // 3f radius * 2 for diameter
        
        float elapsedTime = 0f;
        var renderer = explosionObject.GetComponent<Renderer>();
        
        while (explosionObject != null && elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            
            // Expand the sphere
            float currentScale = Mathf.Lerp(0f, maxScale, progress);
            explosionObject.transform.localScale = Vector3.one * currentScale;
            
            // Fade out
            if (renderer != null)
            {
                var color = renderer.material.color;
                color.a = Mathf.Lerp(0.7f, 0f, progress);
                renderer.material.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }
        
        // Clean up
        if (explosionObject != null)
        {
            Object.Destroy(explosionObject);
        }
    }

    private void PlayExplosionSound()
    {
        // Load and play explosion sound
        var explosionClip = Resources.Load<AudioClip>("Sounds/explosion");
        if (explosionClip != null)
        {
            // Create a temporary AudioSource to play the sound
            var audioObject = new GameObject("ExplosionSound");
            var audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = explosionClip;
            audioSource.volume = 0.7f; // Adjust volume as needed
            audioSource.Play();
            
            // Destroy the audio object after the clip finishes
            Object.Destroy(audioObject, explosionClip.length);
        }
        else
        {
            Debug.LogWarning("ExplosiveFinish: Could not load explosion.wav from Resources/Sounds/");
        }
    }
}
