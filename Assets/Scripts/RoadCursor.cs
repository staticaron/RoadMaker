using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class RoadCursor : MonoBehaviour
{
    [SerializeField] Transform roadCursor;

    [SerializeField] RoadGenerator roadGenerator;

    [SerializeField] float snapDistance;

    [SerializeField] Vector2 closestPoint;

    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        List<Vector2> points = roadGenerator.GetPoints();

        if (points.Count > 0)
        {
			#region Get Snapping Point

			closestPoint = points[0];
			float closestDistance = Mathf.Sqrt(Vector2.SqrMagnitude(points[0] - mousePos));

			for (int i = 1; i < points.Count; i++)
			{
				float currentDistance = Mathf.Sqrt(Vector2.SqrMagnitude(points[i] - mousePos));

				if (currentDistance < closestDistance)
				{
					closestPoint = points[i];
					closestDistance = currentDistance;
				}
			}

			#endregion

			#region Snap to Point if in range

			if (closestDistance <= snapDistance) roadCursor.position = closestPoint;
			else roadCursor.position = mousePos; 

			#endregion
		}
		else
		{
            roadCursor.position = mousePos;
        }
    }

    public Vector2 GetRoadCursorPosition()
    {
        return roadCursor.position;
    }
}