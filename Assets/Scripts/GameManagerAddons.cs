using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using utility;

[System.Serializable]
public class GameManagerAddons : MonoBehaviour
{
    [ContextMenu(nameof(BumpGoal))] private void BumpGoal()
    {
        for (var i = 0; i < 15; i++)
        {
            GameManager.instance.OnKill();
        }
    }
}
