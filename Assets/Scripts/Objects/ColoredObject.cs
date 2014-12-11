using UnityEngine;
using System.Collections;

public class ColoredObject : MonoBehaviour 
{

	public enum ColorChoice
	{
		None,
		Left,
		Right
	}

	public enum HideMode
	{
		Invisible,
		Transparent
	}

	[SerializeField]
	public ColorChoice currentColor;

	[SerializeField]
	public HideMode hideMode;

	[SerializeField]
	protected ColorChoice previousColor;

	[SerializeField]
	public bool isStatic;

	[SerializeField]
	protected bool isHidden;
	
	protected pb_Object obj;
	
	protected bool canChangeColor;

	// Use this for initialization
	protected virtual void Start () 
	{
		previousColor = ColorChoice.None;
		
		updateLayer("ColorMask");
		updateColor(currentColor);

		if(rigidbody != null && !isStatic)
		{
			rigidbody.isKinematic = false;
		}

		canChangeColor = true;
	}

	// Update is called once per frame
	protected virtual void Update () 
	{
		if(CanChangeColor())
		{
			if(currentColor != previousColor)
			{
				updateColor(currentColor);
			}
		}
	}

	protected virtual void LateUpdate()
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
				switchColor(isHidden ? ColorController.Instance.colorParameters.leftTransparentColor : ColorController.Instance.colorParameters.leftColor);
				switchTag("LeftColor");
				break;
				
			case ColorChoice.Right:
				switchColor(isHidden ? ColorController.Instance.colorParameters.rightTransparentColor : ColorController.Instance.colorParameters.rightColor);
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

	protected virtual void switchColor(Color32 color)
	{
		obj = this.gameObject.GetComponent<pb_Object>();
		
		foreach(pb_Face face in obj.faces)
		{
			face.SetColor(color);
			obj.Refresh();
		}
	} 

	protected void switchTag(string tag)
	{
		this.gameObject.tag = tag;
	}

	protected void updateLayer(string layer)
	{
		if(gameObject.layer != LayerMask.NameToLayer(layer))
		{
			this.gameObject.layer = LayerMask.NameToLayer(layer);
		}
	}

	protected bool CanChangeColor()
	{
		return this.canChangeColor;
	}

	protected void CanChangeColor(bool state)
	{
		this.canChangeColor = state;
	}

	public void Show()
	{
		isHidden = false;

		if(renderer != null)
		{
			if(hideMode == HideMode.Invisible)
			{
				updateColor(currentColor);
				renderer.enabled = true;
			}

			else if(hideMode == HideMode.Transparent)
				updateColor(currentColor);
		}
		
		if(rigidbody != null)
		{
			if(!isStatic)
				rigidbody.isKinematic = false;
		}
		
		if(collider != null)
		{
			//collider.enabled = true;
			collider.isTrigger = false;
		}
	}
	
	public void Hide()
	{
		isHidden = true;

		if(renderer != null)
		{
			if(hideMode == HideMode.Invisible)
			{
				renderer.enabled = false;
			}

			else if (hideMode == HideMode.Transparent)
			{
				updateColor(currentColor);
			}
		}
		
		if(rigidbody != null)
		{
			if(!isStatic)
				rigidbody.isKinematic = true;
		}
		
		if(collider != null)
		{
			//collider.enabled = false;
			collider.isTrigger = true;
		}
	}
}
