using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameController : Singleton<GameController> {

	private float deltaTime = 0;
	private bool isPaused = false;

	private PlayerController playerController;
	private CameraController cameraController;
	
	[HideInInspector]
	public List<ColoredObjectScript> coloredObjectList;

	public static bool IsPaused()
	{
		return GameController.Instance.isPaused;
	}

	public static float DeltaTime()
	{
		return GameController.Instance.deltaTime;
	}

	private void Update()
	{
		deltaTime = Time.deltaTime;
	}
	
	private void Start () 
	{
		StartCoroutine("StartLevelRoutine");
		
		coloredObjectList = new List<ColoredObjectScript>();
		coloredObjectList = GetAllColoredObjects();
		
	}

	private IEnumerator StartLevelRoutine()
	{
		yield return new WaitForSeconds(0.1f);
		SpawnPlayer();
		yield return new WaitForSeconds(0.1f);
		InitializeCamera();
	}

	private void SpawnPlayer()
	{
		if(Object.FindObjectOfType(typeof(PlayerController)) == null)
		{
			playerController = PlayerController.Instance;
			playerController.SetPosition(playerController.spawn.position);
		}
	}

	private void InitializeCamera()
	{
		if(Object.FindObjectOfType(typeof(CameraController)) == null)
		{
			cameraController = CameraController.Instance;
			cameraController.ResetToFollowPlayer();
		}
	}
	
	private List<ColoredObjectScript> GetAllColoredObjects()
	{
		ColoredObjectScript[] coloredObjectsArray;
		GameObject coloredObjectsParent;
		
		coloredObjectsParent = GameObject.Find("Objects");
		
		coloredObjectsArray = coloredObjectsParent.GetComponentsInChildren<ColoredObjectScript>();
		
		return coloredObjectsArray.ToList();
	}
	
	public void ShowObjects(ColoredObjectScript.ColorChoice choice)
	{
		foreach(ColoredObjectScript obj in coloredObjectList)
		{
			if(choice == ColoredObjectScript.ColorChoice.None)
			{
				obj.Show();
			}
			
			else if(obj.currentColor == choice)
			{
				obj.Show();
			}
			
			else
			{
				obj.Hide();
			}
		}
	}
	
	public void HideObjects(ColoredObjectScript.ColorChoice choice)
	{
		foreach(ColoredObjectScript obj in coloredObjectList)
		{
			if(obj.currentColor == choice)
			{
				obj.Hide();
			}
			
			else
			{
				obj.Show();
			}
		}
	}
}
