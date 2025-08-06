using UnityEngine;
using utility;

public class PredictionModification : BaseModification
{
    public override string Name => "Провидец";
    public override string Description => "Предсказывает движения врагов";
    public override Material Material => Resources.Load<Material>("Materials/OtherLens");
    public override Rarity Rarity => Rarity.Rare;

    public override void ApplyOnPickUp(AliveState aliveState)
    {
        UpdatePrediction(aliveState);
    }

    public override void ApplyOnDrop(AliveState aliveState)
    {
        UpdatePrediction(aliveState);
    }
    
    private void UpdatePrediction(AliveState aliveState)
    {
        var enemySpawner = Object.FindFirstObjectByType<EnemySpawner>();

        if (!enemySpawner) return;

        enemySpawner.SetOnSpawnBeforeInitialize(enemyInstance =>
        {
            enemyInstance.EnablePrediction(aliveState.ModManager);
        });
    }
}