using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstance : MonoBehaviour
{
    private GameObject targetEntity;
    private Enemy enemy;
    
    public void FocusEntity(GameObject entity)
    {
        targetEntity = entity;
    }

    private void FixedUpdate()
    {
        if (targetEntity)
        {
            enemy.transform.LookAt(targetEntity.transform.position);
        }
    }
}
