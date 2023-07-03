using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public enum RoadGenerationMode
{
    CONSTRUCTING, PLACED, CANCELLED, NONE
}

public struct RoadPoint
{
    public GameObject parentRoadObject;
    public Vector2 position;

    public RoadPoint(GameObject parent, Vector2 position)
    {
        this.parentRoadObject = parent;
        this.position = position;
    }
}

public class RoadGenerator : MonoBehaviour
{
    [Header("Road Settings")]
    [SerializeField] RoadGenerationMode currentRoadGenerationMode = RoadGenerationMode.NONE;

    public RoadGenerationMode CurrentRoadGenerationMode
    {
        get { return currentRoadGenerationMode; }
        private set { currentRoadGenerationMode = value; }
    }

    public Vector2 StartPoint { get; private set; }
    public Vector2 EndPoint { get; private set; }

    [SerializeField] float pointGap;
    [SerializeField] float minEndPointDistance;

    [SerializeField] GameObject pointGO;
    [SerializeField] RoadCursor roadCursor;

    [Header("Road Visuals")]
    [SerializeField] Sprite roadSprite;
    [SerializeField] GameObject roadPrefab;

    private Dictionary<int, List<RoadPoint>> _points;

    private SpriteRenderer _sampleRoad;

    private void Awake()
    {
        _points = new Dictionary<int, List<RoadPoint>>();
    }

    private void Update()
    {
        if (currentRoadGenerationMode == RoadGenerationMode.CONSTRUCTING)
        {
            if (_sampleRoad == null) _sampleRoad = (Instantiate<GameObject>(roadPrefab, StartPoint, Quaternion.identity)).GetComponent<SpriteRenderer>();

            Vector2 directionDelta = (roadCursor.RoadCursorPosition - StartPoint);
            float directionLength = Mathf.Sqrt(Vector2.SqrMagnitude(directionDelta));

            _sampleRoad.transform.up = directionDelta.normalized;
            _sampleRoad.size = new Vector2(1, directionLength);
        }
    }

    public void CreateRoadInput(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:

                if (currentRoadGenerationMode == RoadGenerationMode.CONSTRUCTING) return;

                StartPoint = roadCursor.RoadCursorPosition;
                currentRoadGenerationMode = RoadGenerationMode.CONSTRUCTING;

                break;

            case InputActionPhase.Canceled:

                if (currentRoadGenerationMode != RoadGenerationMode.CONSTRUCTING) return;

                EndPoint = roadCursor.RoadCursorPosition;

                if (Vector2.SqrMagnitude(EndPoint - StartPoint) <= minEndPointDistance * minEndPointDistance)
                {
                    currentRoadGenerationMode = RoadGenerationMode.CANCELLED;
                    return;
                }

                currentRoadGenerationMode = RoadGenerationMode.PLACED;

                List<RoadPoint> newPoints = AddPoints(_sampleRoad.gameObject);

                _sampleRoad.GetComponent<RoadPiece>().FinishPlacement(newPoints);
                _sampleRoad = null;

                break;
        }
    }

    public void Decline(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        if (currentRoadGenerationMode == RoadGenerationMode.CONSTRUCTING)
        {
            currentRoadGenerationMode = RoadGenerationMode.CANCELLED;
            Destroy(_sampleRoad.gameObject);
            _sampleRoad = null;
        }
    }

    private List<RoadPoint> AddPoints(GameObject parent)
    {
        List<RoadPoint> newPoints = new List<RoadPoint>();

        Vector2 roadDirection = (EndPoint - StartPoint).normalized;

        RoadPoint point = new RoadPoint(parent, StartPoint);
        float endPointDistance = Mathf.Sqrt(Vector2.SqrMagnitude(EndPoint - point.position));

        while (endPointDistance > pointGap)
        {
            newPoints.Add(point);

            point = new RoadPoint(parent, point.position + (roadDirection * pointGap));
            endPointDistance = Mathf.Sqrt(Vector2.SqrMagnitude(EndPoint - point.position));
        }

        newPoints.Add(new RoadPoint(parent, EndPoint));

        _points.Add(parent.GetInstanceID(), newPoints);

        return newPoints;
    }

    public Dictionary<int, List<RoadPoint>> GetPoints()
    {
        return _points;
    }

    public void DeleteRoad(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        GameObject currentSnappedPointParent = roadCursor.CurrentlySnappedPoint?.parentRoadObject;

        if (currentSnappedPointParent == null) return;

        _points.Remove(currentSnappedPointParent.GetInstanceID());
        Destroy(currentSnappedPointParent);
    }
}
