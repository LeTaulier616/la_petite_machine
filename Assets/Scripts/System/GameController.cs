using UnityEngine;
using System.Collections;

public class GameController : Singleton<GameController> {

	private float deltaTime = 0;

	private PlayerController playerController;
	private CameraController cameraController;

	public static float DeltaTime()
	{
		return GameController.Instance.deltaTime;
	}

	private void Update()
	{
		deltaTime = Time.deltaTime;
	}
	
	private void Start () {

		StartCoroutine("StartLevelRoutine");

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

}
