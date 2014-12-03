using UnityEngine;
using System.Collections;

public class ColorCannon : MonoBehaviour 
{
	[HideInInspector]
	public ColorController.ColorChoice cannonColor;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.Rotate(Vector3.up, 100.0f * GameController.DeltaTime(), Space.World);
	}

	public void SetColor(ColorController.ColorChoice choice)
	{
		if(choice == ColorController.ColorChoice.Left)
		{
			cannonColor = ColorController.ColorChoice.Right;
			renderer.material.color = Color.green;
		}

		else if (choice == ColorController.ColorChoice.Right)
		{
			cannonColor = ColorController.ColorChoice.Left;
			renderer.material.color = Color.red;
		}

		else
		{
			Debug.Log(gameObject.name + "has a color zone set on None. Destroying " + gameObject.name);
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			if(PlayerController.Instance.InColorZone())
			{
				PlayerController.Instance.CanShoot(true);
				PlayerController.Instance.SetCannonColor(cannonColor);
			}

			else
			{
				Debug.LogWarning("Player is not in a color zone. Destroying cannon");
				Destroy(gameObject);
			}
		}
	}
}
