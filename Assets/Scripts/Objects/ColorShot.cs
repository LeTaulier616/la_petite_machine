using UnityEngine;
using System.Collections;

public class ColorShot : MonoBehaviour 
{
	//private ColoredObject.ColorChoice shotColor;

	private RaycastHit hitInfo;

	private Vector3 startPosition;
	private float distanceTraveled;

	// Use this for initialization
	void Start () 
	{
		startPosition = transform.position;

		//UpdateShotColor(PlayerController.Instance.cannonColor);
	}
	
	// Update is called once per frame
	void Update () 
	{
		CheckCollision();

		transform.Translate(Vector3.forward * PlayerController.Instance.shot.travelSpeed * GameController.DeltaTime());

		distanceTraveled = Vector3.Distance(startPosition, transform.position);

		if(distanceTraveled >= PlayerController.Instance.shot.travelDistance)
		{
			Destroy(gameObject);
		}
	}

	private void CheckCollision()
	{
		if(Physics.Raycast(transform.position, transform.forward, out hitInfo, PlayerController.Instance.shot.range))
		{
			if(hitInfo.collider.CompareTag("LeftColor") || hitInfo.collider.CompareTag("RightColor"))
			{
				hitInfo.collider.SendMessage("SwitchChoice");
			}

			Destroy(gameObject);
		}
	}

	/*
	private void UpdateShotColor(ColorController.ColorChoice choice)
	{
		shotColor = choice == ColorController.ColorChoice.Left ? ColoredObject.ColorChoice.Left : ColoredObject.ColorChoice.Right;

		renderer.material.color = shotColor == ColoredObject.ColorChoice.Left ? Color.red : Color.green;
	}
	*/
}
