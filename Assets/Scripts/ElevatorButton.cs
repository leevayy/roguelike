using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    [SerializeField] private GameObject elevatorButtonPrefab;
    [SerializeField] private Transform elevatorButtonTransform;
    [SerializeField] private Elevator elevator;
    [SerializeField] private int level;
    
    private void Start()
    {
        var buyBox = Instantiate(elevatorButtonPrefab, elevatorButtonTransform);
        
        var priceTag = buyBox.GetComponentInChildren<PriceTag>();
        
        priceTag.Init(true, () =>
        {
            _ = elevator.GoTo(level);
        });
    }
}
