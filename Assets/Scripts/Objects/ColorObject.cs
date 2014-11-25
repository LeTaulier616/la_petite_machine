using UnityEngine;
using System.Collections;

public class ColorObject : ColoredObject 
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
		if(!GameController.Instance.colorObjectList.Contains(this))
		{
			GameController.Instance.colorObjectList.Add(this);
		}
	}
	
	protected virtual void OnDisable()
	{
		if(GameController.Instance.colorObjectList.Contains(this))
		{
			GameController.Instance.colorObjectList.Remove(this);
		}
	}
}
