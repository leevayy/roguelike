using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private List<EnemyInstance> enemies;

    private void Start()
    {
        StartFocus();
    }

    [ContextMenu(nameof(StartFocus))] private void StartFocus()
    {
        enemies.ForEach((instance => instance.FocusEntity(player)));
    }
}
