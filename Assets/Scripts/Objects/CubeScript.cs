using UnityEngine;
using System.Collections;

public class CubeScript : MonoBehaviour {

	private StateMachine stateMachine;
	public IdleState idle;
	public GrabbedState grabbed;

	// Use this for initialization
	void Start () {
		idle = new IdleState();
		grabbed = new GrabbedState();
		stateMachine = NGUITools.AddMissingComponent<StateMachine>(gameObject);
		stateMachine.SetState(idle);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public class IdleState : State
	{
		public override void EnterState(GameObject go)
		{
			Debug.Log("IdleEnter");
		}
		
		public override void UpdateState(GameObject go)
		{
			Debug.Log("IdleUpdate");

		}
		
		public override void LateUpdateState(GameObject go)
		{

		}
		
		public override void ExitState(GameObject go)
		{
			
		}
	}

	public class GrabbedState : State
	{
		public override void EnterState(GameObject go)
		{
			Debug.Log("GrabbedEnter");
		}
		
		public override void UpdateState(GameObject go)
		{
			Debug.Log("GrabbedUpdate");
			
		}
		
		public override void LateUpdateState(GameObject go)
		{
			
		}
		
		public override void ExitState(GameObject go)
		{
			
		}
	}
}
