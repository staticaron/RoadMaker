using System.Collections.Generic;

using UnityEngine;

public class RoadPiece : MonoBehaviour
{
	[SerializeField] List<RoadPoint> roadPoints;

	[SerializeField] float initialOpacity = 0.5f;

	[SerializeField] SpriteRenderer round1SpriteRenderer;
	[SerializeField] SpriteRenderer round2SpriteRenderer;

	private SpriteRenderer mainRoadSpriteRenderer;

	private void Awake()
	{
		mainRoadSpriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		mainRoadSpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, initialOpacity);
		round1SpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, initialOpacity);
		round2SpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, initialOpacity);
	}

	public void FinishPlacement(List<RoadPoint> points)
	{
		roadPoints = points;

		round2SpriteRenderer.transform.localPosition = new Vector3(0, mainRoadSpriteRenderer.size.y, 0);

		mainRoadSpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		round1SpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		round2SpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	}

	public List<RoadPoint> GetPoints()
	{
		return roadPoints;
	}

	public void KYS()
	{
		Destroy(gameObject);
	}
}
