using UnityEngine;
using UnityEngine.InputSystem;

public enum RoadPlacementWay
{
    CONSTRUCTING, PLACED, CANCELLED, NONE
}

public class RoadGenerator : MonoBehaviour
{
    [SerializeField] PlayerInput roadMakerInput;

    [SerializeField] RoadPlacementWay roadPlacementWay = RoadPlacementWay.NONE;

    [SerializeField] Vector2 startPoint;
    [SerializeField] Vector2 endPoint;

    private Camera mainCamera;

    private void Awake()
    {
        roadMakerInput = GetComponent<PlayerInput>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {

    }

    public void CreateRoadInput(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:

                if (roadPlacementWay == RoadPlacementWay.CONSTRUCTING) return;

                startPoint = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                roadPlacementWay = RoadPlacementWay.CONSTRUCTING;

                break;

            case InputActionPhase.Canceled:

                if (roadPlacementWay != RoadPlacementWay.CONSTRUCTING) return;

                endPoint = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                roadPlacementWay = RoadPlacementWay.PLACED;

                break;
        }
    }

    public void Decline(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        if (roadPlacementWay == RoadPlacementWay.CONSTRUCTING) roadPlacementWay = RoadPlacementWay.CANCELLED;
    }
}
