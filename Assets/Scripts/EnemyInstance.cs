using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstance : MonoBehaviour
{
    private GameObject targetEntity;
    private Enemy enemy;

    private void Start()
    {
        enemy = GetFirstChildOfType<Enemy>();
    }
    
    T GetFirstChildOfType<T>() where T : Component
    {
        foreach (Transform child in transform)
        {
            T component = child.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
        }
        return null; 
    }

    public void FocusEntity(GameObject entity)
    {
        if (!enemy)
        {
            return;
        }

        targetEntity = entity;
        enemy.StartMoving();
    }

    private void FixedUpdate()
    {
        if (targetEntity)
        {
            enemy.transform.LookAt(targetEntity.transform.position);
        }
    }
}
