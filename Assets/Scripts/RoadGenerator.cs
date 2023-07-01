using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public enum RoadPlacementWay
{
	CONSTRUCTING, PLACED, CANCELLED, NONE
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

	private List<Vector2> points;

	private Camera mainCamera;
	private SpriteRenderer sampleRoad;

	private void Awake()
	{
		if (mainCamera == null)
			mainCamera = Camera.main;

		points = new List<Vector2>();
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
			sampleRoad.color = new Color(sampleRoad.color.r, sampleRoad.color.g, sampleRoad.color.b, 0.5f);
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

				AddPoints();

				sampleRoad.color = new Color(sampleRoad.color.r, sampleRoad.color.g, sampleRoad.color.b, 1.0f);
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
	}

	public List<Vector2> GetPoints()
	{
		return points;
	}
}
