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
	public List<ColorObject> colorObjectList;
	public List<AntiColorObject> anticolorObjectList;

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
	
	private void Awake () 
	{
		StartCoroutine("StartLevelRoutine");
		
		colorObjectList = new List<ColorObject>();
		colorObjectList = GetColoredObjects();

		anticolorObjectList = new List<AntiColorObject>();
		anticolorObjectList = GetAntiColoredObjects();
		
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
	
	private List<ColorObject> GetColoredObjects()
	{
		ColorObject[] coloredObjectsArray;
		GameObject coloredObjectsParent;
		
		coloredObjectsParent = GameObject.Find("Objects");
		
		coloredObjectsArray = coloredObjectsParent.GetComponentsInChildren<ColorObject>();
		
		return coloredObjectsArray.ToList();
	}

	private List<AntiColorObject> GetAntiColoredObjects()
	{
		AntiColorObject[] anticoloredObjectsArray;
		GameObject anticoloredObjectsParent;
		
		anticoloredObjectsParent = GameObject.Find("Objects");
		
		anticoloredObjectsArray = anticoloredObjectsParent.GetComponentsInChildren<AntiColorObject>();
		
		return anticoloredObjectsArray.ToList();
	}

	public void UpdateObjects(ColoredObject.ColorChoice choice)
	{
		UpdateColorObjects(choice);
		UpdateAntiColorObjects(choice);
	}

	private void UpdateColorObjects(ColoredObject.ColorChoice choice)
	{
		foreach(ColorObject obj in colorObjectList)
		{
			if(choice == ColoredObject.ColorChoice.None)
			{
				obj.Show();
			}

			else if(choice == obj.currentColor)
			{
				obj.Hide();
			}

			else
			{
				obj.Show ();
			}
		}
	}

	private void UpdateAntiColorObjects(ColorObject.ColorChoice choice)
	{
		foreach(AntiColorObject obj in anticolorObjectList)
		{
			if(choice == ColoredObject.ColorChoice.None)
			{
				obj.Hide();
			}

			else if (choice == obj.currentColor)
			{
				obj.Show();
			}

			else
			{
				obj.Hide();
			}
		}
	}
}
