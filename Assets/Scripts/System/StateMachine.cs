using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class State
{
	protected StateMachine machine;
	
	public void SetStateMachine(StateMachine m)
	{
		this.machine = m;
	}
	
	virtual public void EnterState(GameObject it)
	{
	}
	
	virtual public void UpdateState(GameObject it)
	{
	}
	
	virtual public void LateUpdateState(GameObject it)
	{
	}
	
	virtual public void ExitState(GameObject it)
	{
	}
}

public class StateMachine : MonoBehaviour
{
	public State currentState { get; private set; }
	public State previousState;
	
	public StateMachine()
	{
		this.currentState = null;
	}
	
	void Update()
	{
		if (this.currentState != null)
			this.currentState.UpdateState(this.gameObject);
	}
	
	void LateUpdate()
	{
		if (this.currentState != null)
			this.currentState.LateUpdateState(this.gameObject);
	}
	
	public void SetState(State state)
	{	
		previousState = currentState;

		if (this.currentState != null)
			this.currentState.ExitState(this.gameObject);
		if (state != null)
		{
			state.SetStateMachine(this);
			state.EnterState(this.gameObject);
		}
		
		this.currentState = state;
	}
}