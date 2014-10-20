using UnityEngine;
using System.Collections;

public class GameInitializer : MonoBehaviour {
	
	private GameController gameController;
	private InputController inputController;
		
	void Awake()
	{
		gameController = GameController.Instance;
		inputController = InputController.Instance;

		gameController.transform.parent = this.transform;
		inputController.transform.parent = this.transform;
	}
}
