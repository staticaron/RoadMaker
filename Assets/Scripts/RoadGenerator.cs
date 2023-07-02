using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public enum RoadPlacementWay
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
	[SerializeField] RoadPlacementWay roadPlacementWay = RoadPlacementWay.NONE;

	[SerializeField] Vector2 startPoint;
	[SerializeField] Vector2 endPoint;

	[SerializeField] float pointGap;
	[SerializeField] float minEndPointDistance;

	[SerializeField] GameObject pointGO;
	[SerializeField] RoadCursor roadCursor;

	[Header("Road Visuals")]
	[SerializeField] Sprite roadSprite;
	[SerializeField] GameObject roadPrefab;

	private Dictionary<int, List<RoadPoint>> points;

	private Camera mainCamera;
	private SpriteRenderer sampleRoad;

	private void Awake()
	{
		if (mainCamera == null)
			mainCamera = Camera.main;

		points = new Dictionary<int, List<RoadPoint>>();
	}

	private void Update()
	{
		if (roadPlacementWay == RoadPlacementWay.CONSTRUCTING)
		{
			if (sampleRoad == null) sampleRoad = (Instantiate<GameObject>(roadPrefab, startPoint, Quaternion.identity)).GetComponent<SpriteRenderer>();

			Vector2 directionDelta = (roadCursor.GetRoadCursorPosition() - startPoint);
			float directionLength = Mathf.Sqrt(Vector2.SqrMagnitude(directionDelta));

			sampleRoad.transform.up = directionDelta.normalized;
			sampleRoad.size = new Vector2(1, directionLength);
		}
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

				List<RoadPoint> newPoints = AddPoints(sampleRoad.gameObject);

				sampleRoad.GetComponent<RoadPiece>().FinishPlacement(newPoints);
				sampleRoad = null;

				break;
		}
	}

	public void Decline(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Performed) return;

		if (roadPlacementWay == RoadPlacementWay.CONSTRUCTING)
		{
			roadPlacementWay = RoadPlacementWay.CANCELLED;
			Destroy(sampleRoad.gameObject);
			sampleRoad = null;
		}
	}

	private List<RoadPoint> AddPoints(GameObject parent)
	{
		List<RoadPoint> newPoints = new List<RoadPoint>();

		Vector2 roadDirection = (endPoint - startPoint).normalized;

		RoadPoint point = new RoadPoint(parent, startPoint);
		float endPointDistance = Mathf.Sqrt(Vector2.SqrMagnitude(endPoint - point.position));

		while (endPointDistance > pointGap)
		{
			newPoints.Add(point);

			point = new RoadPoint(parent, point.position + (roadDirection * pointGap));
			endPointDistance = Mathf.Sqrt(Vector2.SqrMagnitude(endPoint - point.position));
		}

		newPoints.Add(new RoadPoint(parent, endPoint));

		points.Add(parent.GetInstanceID(), newPoints);

		return newPoints;
	}

	public Dictionary<int, List<RoadPoint>> GetPoints()
	{
		return points;
	}

	public void DeleteRoad(InputAction.CallbackContext context)
	{
		if (context.phase != InputActionPhase.Performed) return;

		GameObject currentSnappedPointParent = roadCursor.GetCurrentlySnappedPoint()?.parentRoadObject;

		if (currentSnappedPointParent == null) return;

		points.Remove(currentSnappedPointParent.GetInstanceID());
		Destroy(currentSnappedPointParent);
	}
}
