using UnityEngine;
using System.Collections;

public class AntiColorObject : ColoredObject 
{

	// Use this for initialization
	protected override void Start () 
	{
		base.Start();
	}
	
	// Update is called once per frame
	protected override void Update () 
	{
		base.Update();
	}

	protected override void LateUpdate ()
	{
		base.LateUpdate();
	}

	protected virtual void OnEnable()
	{
		if(GameController.Instance != null)
		{
			GameController.Instance.UpdateColorObjectList(this, true);
		}
	}
	
	protected virtual void OnDisable()
	{
		if(GameController.Instance != null)
		{
			GameController.Instance.UpdateColorObjectList(this, false);
		}
	}
}
