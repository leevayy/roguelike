using UnityEngine;
using UnityEngine.Serialization;

public enum CameraMode {
    Static,
    Dynamic,
}

public class SettingsManager : MonoBehaviour
{
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

    public CameraMode cameraType = CameraMode.Static;
}
