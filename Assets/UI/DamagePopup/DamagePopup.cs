using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset damagePopupTemplate;

    [SerializeField] private UIDocument baseContainerDocument;
    
    [SerializeField] private Camera cam;

    private VisualElement baseContainer;

    private void Awake()
    {
        baseContainer = baseContainerDocument.rootVisualElement.Q<VisualElement>("BaseContainer");
    }

    public async Awaitable ShowDamagePopup(float damage, Vector3 position)
    {
        var damagePopup = damagePopupTemplate.Instantiate();
        
        var label = damagePopup.Q<Label>("damage-tag") as Label;

        label.text = damage.ToString();
        
        baseContainer.Add(damagePopup);

        var screenPosition = cam.WorldToScreenPoint(position);
        
        var panelPosition = RuntimePanelUtils.ScreenToPanel(
            baseContainerDocument.rootVisualElement.panel,
            new Vector2(screenPosition.x, Screen.height - screenPosition.y)
        );

        damagePopup.style.position = Position.Absolute;
        damagePopup.style.left = panelPosition.x;
        damagePopup.style.top = panelPosition.y - 20f;

        await AnimatePopup(damagePopup);
    }
    
    public async Awaitable ShowDamagePopup(string change, Vector2 panelPosition)
    {
        var damagePopup = damagePopupTemplate.Instantiate();
        
        var label = damagePopup.Q<Label>("damage-tag") as Label;

        if (change[0] == '-')
        {
            label.AddToClassList("moneyLost");
        }
        else
        {
            label.AddToClassList("moneyGained");
        }
        
        label.text = change;
        
        baseContainer.Add(damagePopup);

        damagePopup.style.position = Position.Absolute;
        damagePopup.style.left = panelPosition.x;
        damagePopup.style.bottom =  panelPosition.y;

        await AnimatePopup(damagePopup);
    }
    
    private async Awaitable AnimatePopup(VisualElement popup)
    {
        // Wait 2 seconds before starting the animation
        await Awaitable.WaitForSecondsAsync(2f);

        // Configure how you want the popup to move & fade
        var startTop  = popup.resolvedStyle.top;
        var endTop    = startTop - 20f;  // move upward by 20 px
        var fadeTime  = 1f;
        var elapsed   = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / fadeTime);

            // Move upward
            var newTop = Mathf.Lerp(startTop, endTop, t);
            popup.style.top = newTop;

            // Fade out
            var newOpacity = Mathf.Lerp(1f, 0f, t);
            popup.style.opacity = newOpacity;

            // Yield control until next frame on the main thread
            await Task.Yield();
        }

        // Remove popup from the hierarchy once done
        popup.RemoveFromHierarchy();
    }
}
