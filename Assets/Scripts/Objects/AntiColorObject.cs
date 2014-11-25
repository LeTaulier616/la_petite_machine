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
		if(!GameController.Instance.anticolorObjectList.Contains(this))
		{
			GameController.Instance.anticolorObjectList.Add(this);
		}
	}
	
	protected virtual void OnDisable()
	{
		if(GameController.Instance.anticolorObjectList.Contains(this))
		{
			GameController.Instance.anticolorObjectList.Remove(this);
		}
	}
}
