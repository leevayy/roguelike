using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum CameraMode {
    Static,
    Dynamic,
}

public class SettingsManager : MonoBehaviour
{
    public float volume = 0.2f;
    public static SettingsManager instance { get; private set; }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        AudioListener.volume = volume;
    }

    public CameraMode cameraType = CameraMode.Static;
}
