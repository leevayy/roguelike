using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class HealthBarManager : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset healthBarTemplate;

    [SerializeField] private UIDocument baseContainerDocument;
    
    [SerializeField] private Camera cam;

    private VisualElement _baseContainer;

    private void Awake()
    {
        _baseContainer = baseContainerDocument.rootVisualElement.Q<VisualElement>("BaseContainer");
    }

    public void AddHealthBar(float health, GameObject holder)
    {
        AddHealthBar(health, holder, holder.name);
    }
    public void AddHealthBar(float health, GameObject holder, string nameTag)
    {
        var healthBar = healthBarTemplate.Instantiate();
     
        var nameLabel = healthBar.Q<Label>("name-tag");

        nameLabel.text = nameTag;
        
        var label = healthBar.Q<Label>("health-tag") as Label;

        label.text = health.ToString(CultureInfo.CurrentCulture);
        
        _baseContainer.Add(healthBar);
        
        var holderScript = holder.GetComponentInChildren<HealthBar>();
        
        holderScript.Init(healthBar, baseContainerDocument);
    }
}
