using System.Collections.Generic;

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

    [SerializeField] float pointGap;
    [SerializeField] float minEndPointDistance;

    [SerializeField] GameObject pointGO;
    [SerializeField] RoadCursor roadCursor;

    private List<Vector2> points;

    private Camera mainCamera;

    private void Awake()
    {
        roadMakerInput = GetComponent<PlayerInput>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        points = new List<Vector2>();
    }

    public void CreateRoadInput(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:

                if (roadPlacementWay == RoadPlacementWay.CONSTRUCTING) return;

                startPoint = roadCursor.GetRoadCursorPosition();
                roadPlacementWay = RoadPlacementWay.CONSTRUCTING;

                break;

            case InputActionPhase.Canceled:

                if (roadPlacementWay != RoadPlacementWay.CONSTRUCTING) return;

                endPoint = roadCursor.GetRoadCursorPosition();

                if (Vector2.SqrMagnitude(endPoint - startPoint) <= minEndPointDistance * minEndPointDistance)
                {
                    roadPlacementWay = RoadPlacementWay.CANCELLED;
                    return;
                }

                roadPlacementWay = RoadPlacementWay.PLACED;

                AddPoints();

                break;
        }
    }

    public void Decline(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        if (roadPlacementWay == RoadPlacementWay.CONSTRUCTING) roadPlacementWay = RoadPlacementWay.CANCELLED;
    }

    private void AddPoints()
    {
        Vector2 roadDirection = (endPoint - startPoint).normalized;

        Vector2 point = startPoint;
        float endPointDistance = Mathf.Sqrt(Vector2.SqrMagnitude(endPoint - point));

        while (endPointDistance > pointGap)
        {
            points.Add(point);

			point += (roadDirection * pointGap);
            endPointDistance = Mathf.Sqrt(Vector2.SqrMagnitude(endPoint - point));
        }

        points.Add(endPoint);

        SpawnObjects();
    }

    private void SpawnObjects()
    {
        foreach (Vector2 point in points)
        {
            Instantiate<GameObject>(pointGO, point, Quaternion.identity);
        }
    }

    public List<Vector2> GetPoints()
    {
        return points;
    }
}
