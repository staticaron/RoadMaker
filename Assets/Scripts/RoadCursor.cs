using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RoadGenerator))]
public class RoadCursor : MonoBehaviour
{
	[SerializeField] Transform roadCursor;
	[SerializeField] float snapDistance;

	[SerializeField] bool isSnappedToPoint;
	[SerializeField] RoadPoint? currentlySnappedTo;

	private RoadGenerator roadGenerator;

	private Camera mainCam;

	private void Awake()
	{
		mainCam = Camera.main;
		roadGenerator = GetComponent<RoadGenerator>();
	}

	private void Update()
	{
		Vector2 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

		List<RoadPoint> points = new List<RoadPoint>();

		foreach(List<RoadPoint> currentPoints in roadGenerator.GetPoints().Values)
		{
			points.AddRange(currentPoints);
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
				isSnappedToPoint = true;
				currentlySnappedTo = closestPoint;
			}
			else
			{
				roadCursor.position = mousePos;
				isSnappedToPoint = false;
				currentlySnappedTo = null;
			}

			#endregion
		}
		else
		{
			roadCursor.position = mousePos;
			isSnappedToPoint = false;
			currentlySnappedTo = null;
		}
	}

	public Vector2 GetRoadCursorPosition()
	{
		return roadCursor.position;
	}

	public RoadPoint? GetCurrentlySnappedPoint()
	{
		return currentlySnappedTo;
	}
}
