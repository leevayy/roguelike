using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptWithEmptyOnGUI : MonoBehaviour
{
    void OnGUI()
    {
    }

    private void Awake()
    {
        UIToolkitInputConfiguration.SetRuntimeInputBackend(UIToolkitInputBackendOption.InputSystemCompatibleBackend);
    }
}