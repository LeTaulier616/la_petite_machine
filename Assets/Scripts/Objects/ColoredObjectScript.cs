using UnityEngine;
using System.Collections;

public class ColoredObjectScript : MonoBehaviour 
{
	public enum ColorChoice
	{
		None,
		Left,
		Right,
		Up,
		Down
	}
	
	[SerializeField]
	public ColorChoice currentColor;

	[SerializeField]
	private ColorChoice previousColor;
	
	[SerializeField]
	public bool isStatic;
	
	private pb_Object obj;
	
	private bool canChangeColor;
	
	// Use this for initialization
	void Start () 
	{		
		canChangeColor = true;
		
		previousColor = ColorChoice.None;
		
		updateLayer("ColorMask");
		
		if(rigidbody != null && !isStatic)
			rigidbody.isKinematic = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(CanChangeColor())
		{
			if(currentColor != previousColor)
				updateColor(currentColor);
		}
	}
	
	void LateUpdate()
	{
		previousColor = currentColor;
	}
	
	public void updateColor(ColorChoice currentColor)
	{		
		switch(currentColor)
		{
		case ColorChoice.None:
			switchColor(Color.white);
			switchTag("Untagged");
			break;
			
		case ColorChoice.Left:
			switchColor(ColorController.Instance.colorParameters.leftColor);
			switchTag("LeftColor");
			break;
			
		case ColorChoice.Right:
			switchColor(ColorController.Instance.colorParameters.rightColor);
			switchTag("RightColor");
			break;
	/*
		case ColorChoice.Up:
			switchColor(ColorController.Instance.colorParameters.upColor);
			switchTag("UpColor");
			break;
			
		case ColorChoice.Down:
			switchColor(ColorController.Instance.colorParameters.downColor);
			switchTag("DownColor");
			break;
	*/
		}
	}
	
	private void switchColor(Color32 color)
	{
		obj = GetComponent<pb_Object>();
		
		foreach(pb_Face face in obj.faces)
		{
			face.SetColor(color);
			obj.Refresh();
		}
	} 
	
	private void switchTag(string tag)
	{
		gameObject.tag = tag;
	}
	
	private void updateLayer(string layer)
	{
		if(gameObject.layer != LayerMask.NameToLayer(layer))
		{
			gameObject.layer = LayerMask.NameToLayer(layer);
		}
	}
	
	public bool CanChangeColor()
	{
		return canChangeColor;
	}
	
	public void CanChangeColor(bool state)
	{
		canChangeColor = state;
	}
	
	public void Show()
	{
		if(renderer != null && !renderer.enabled)
			renderer.enabled = true;
			
		if(collider != null == !collider.enabled)
			collider.enabled = true;
			
		if(rigidbody != null && !isStatic)
			rigidbody.isKinematic = false;
	}
	
	public void Hide()
	{
		if(renderer != null && renderer.enabled)
			renderer.enabled = false;
		
		if(collider != null == collider.enabled)
			collider.enabled = false;
			
		if(rigidbody != null && !isStatic)
			rigidbody.isKinematic = true;
	}
	/*
	private void OnEnable()
	{
		if(!GameController.Instance.coloredObjectList.Contains(this))
		{
			GameController.Instance.coloredObjectList.Add(this);
		}
	}
	
	private void OnDisable()
	{
		if(GameController.Instance.coloredObjectList.Contains(this))
		{
			GameController.Instance.coloredObjectList.Remove(this);
		}
	}
	*/
}
