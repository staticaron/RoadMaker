using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RoadGenerator))]
public class RoadCursor : MonoBehaviour
{
    [SerializeField] Transform roadCursor;

    public Vector2 RoadCursorPosition
    {
        get { return roadCursor.position; }
        private set { }
    }

    public RoadPoint? CurrentlySnappedPoint
    {
        get { return currentlySnappedTo; }
        private set { }
    }

    [SerializeField] float snapDistance = 0.5f;

    [SerializeField] bool _isSnappedToPoint = false;
    [SerializeField] RoadPoint? currentlySnappedTo;

    [SerializeField] Vector2 directionValue;

    private bool IsSnappedToPoint
    {
        get { return _isSnappedToPoint; }
        set
        {
            if (value == false) currentlySnappedTo = null;
            _isSnappedToPoint = value;
        }
    }

    [SerializeField] bool linearMode = false;

    private RoadGenerator _roadGenerator;

    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
        _roadGenerator = GetComponent<RoadGenerator>();
    }

    public void ToggleLinearMovement(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) linearMode = true;
        else if (context.phase == InputActionPhase.Canceled) linearMode = false;
    }

    private void Update()
    {
        Vector2 mousePos = _mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        List<RoadPoint> points = new List<RoadPoint>();

        foreach (List<RoadPoint> currentPoints in _roadGenerator.GetPoints().Values)
        {
            points.AddRange(currentPoints);
        }

        if (linearMode && _roadGenerator.CurrentRoadGenerationMode == RoadGenerationMode.CONSTRUCTING)
        {
            directionValue = (mousePos - _roadGenerator.StartPoint).normalized;

            if (Mathf.Abs(directionValue.x) > Mathf.Abs(directionValue.y))
            {
                roadCursor.position = new Vector2(mousePos.x, _roadGenerator.StartPoint.y);
            }
            else if (Mathf.Abs(directionValue.y) > Mathf.Abs(directionValue.x))
            {
                roadCursor.position = new Vector2(_roadGenerator.StartPoint.x, mousePos.y);
            }

            IsSnappedToPoint = false;
        }

        if (points.Count > 0)
        {
            #region Get Snapping Point

            RoadPoint closestPoint = points[0];
            float closestDistance = Mathf.Sqrt(Vector2.SqrMagnitude(points[0].position - mousePos));

            for (int i = 1; i < points.Count; i++)
            {
                float currentDistance = Mathf.Sqrt(Vector2.SqrMagnitude(points[i].position - mousePos));

                if (currentDistance < closestDistance)
                {
                    closestPoint = points[i];
                    closestDistance = currentDistance;
                }
            }

            #endregion

            #region Snap to Point if in range

            if (closestDistance <= snapDistance)
            {
                roadCursor.position = closestPoint.position;

                IsSnappedToPoint = true;
                currentlySnappedTo = closestPoint;
            }
            else if (linearMode == false)
            {
                roadCursor.position = mousePos;
                IsSnappedToPoint = false;
            }

            #endregion
        }
        else if (linearMode == false)
        {
            roadCursor.position = mousePos;
            IsSnappedToPoint = false;
        }
    }
}
