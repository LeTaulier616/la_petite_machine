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
	private pb_Object obj;
	
	private bool canChangeColor;
	
	// Use this for initialization
	void Start () 
	{		
		canChangeColor = true;
		
		previousColor = ColorChoice.None;
		
		updateLayer("ColorMask");
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
				
			case ColorChoice.Up:
				switchColor(ColorController.Instance.colorParameters.upColor);
				switchTag("UpColor");
				break;
				
			case ColorChoice.Down:
				switchColor(ColorController.Instance.colorParameters.downColor);
				switchTag("DownColor");
				break;
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
}
