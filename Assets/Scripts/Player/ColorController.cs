using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;


public class ColorController : Singleton<ColorController> 
{
	public ColorParameters colorParameters;
	
	private AmplifyColorEffect colorEffect;
	
	public enum ColorChoice
	{
		None,
		Left,
		Right,
		Up,
		Down
	}
	
	private ColorChoice currentColor;
	private ColorChoice previousColor;
	
	private bool canChangeColor;
	private bool leftWasReleased;
	private bool rightWasReleased;
	private bool upWasReleased;
	private bool downWasReleased;
	
	// Use this for initialization
	void Start ()
	{
		colorEffect = GetComponent<AmplifyColorEffect>();
		
		currentColor = ColorChoice.None;
		previousColor = ColorChoice.None;
		
		canChangeColor = true;
		leftWasReleased = true;
		rightWasReleased = true;
		upWasReleased = true;
		downWasReleased = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		UpdateEarlyVariables();
		
		if(CanChangeColor())
		{
			if(leftColorRequested())
			{
				currentColor = currentColor == ColorChoice.Left ? ColorChoice.None : ColorChoice.Left;
			}
			
			else if(rightColorRequested())
			{
				currentColor = currentColor == ColorChoice.Right ? ColorChoice.None : ColorChoice.Right;
			}
			
			else if(upColorRequested())
			{
				currentColor = currentColor == ColorChoice.Up ? ColorChoice.None : ColorChoice.Up;
			}
			
			else if(downColorRequested())
			{
				currentColor = currentColor == ColorChoice.Down ? ColorChoice.None : ColorChoice.Down;
			}
			
			switchColor(currentColor);
		}
	}
	
	void LateUpdate()
	{
		previousColor = currentColor;
	}
	
	private void UpdateEarlyVariables()
	{
		if(!InputController.Instance.GetControl(InputController.Instance.controls.switchLeft).IsPressed)
			leftWasReleased = true;
			
		if(!InputController.Instance.GetControl(InputController.Instance.controls.switchRight).IsPressed)
			rightWasReleased = true;
			
		if(!InputController.Instance.GetControl(InputController.Instance.controls.switchUp).IsPressed)
			upWasReleased = true;
			
		if(!InputController.Instance.GetControl(InputController.Instance.controls.switchDown).IsPressed)
			downWasReleased = true;
	}
	
	private void switchColor(ColorChoice currentColor)
	{
		if(currentColor == previousColor)
		{
			return;
		}
		
		switch(currentColor)
		{
			case ColorChoice.None:
			switchTexture(null);
			GameController.Instance.ShowObjects(ColoredObjectScript.ColorChoice.None);
			break;
			
			case ColorChoice.Left:
			switchTexture(colorParameters.leftTexture);
			GameController.Instance.HideObjects(ColoredObjectScript.ColorChoice.Left);
			break;
			
			case ColorChoice.Right:
			switchTexture(colorParameters.rightTexture);
			GameController.Instance.HideObjects(ColoredObjectScript.ColorChoice.Right);
			
			break;
			
			case ColorChoice.Up:
			switchTexture(colorParameters.upTexture);
			GameController.Instance.HideObjects(ColoredObjectScript.ColorChoice.Up);
			break;
			
			case ColorChoice.Down:
			switchTexture(colorParameters.downTexture);
			GameController.Instance.HideObjects(ColoredObjectScript.ColorChoice.Down);
			break;
		}
	}
	
	private void switchTexture(Texture2D texture)
	{
		colorEffect.LutTexture = texture;
	}
	
	public bool leftColorRequested()
	{
		bool leftColorRequested = false;
		
		if(InputController.Instance.GetControl(InputController.Instance.controls.switchLeft).IsPressed && leftWasReleased)
		{
			leftColorRequested = true;
			leftWasReleased = false;
		}
		
		return leftColorRequested;
	}
	
	public bool rightColorRequested()
	{
		bool rightColorRequested = false;
		
		if(InputController.Instance.GetControl(InputController.Instance.controls.switchRight).IsPressed && rightWasReleased)
		{
			rightColorRequested = true;
			rightWasReleased = false;
		}
		
		return rightColorRequested;
	}
	
	public bool upColorRequested()
	{
		bool upColorRequested = false;
		
		if(InputController.Instance.GetControl(InputController.Instance.controls.switchUp).IsPressed && upWasReleased)
		{
			upColorRequested = true;
			upWasReleased = false;
		}
		
		return upColorRequested;
	}
	
	public bool downColorRequested()
	{
		bool downColorRequested = false;
		
		if(InputController.Instance.GetControl(InputController.Instance.controls.switchDown).IsPressed && downWasReleased)
		{
			downColorRequested = true;
			downWasReleased = false;
		}
		
		return downColorRequested;
	}
	
	public bool CanChangeColor()
	{
		return canChangeColor;
	}
	
	public void CanChangeColor(bool state)
	{
		canChangeColor = state;
	}
	
	[System.Serializable]
	public class ColorParameters
	{
		public Texture2D leftTexture, rightTexture, upTexture, downTexture;	
		public Color32 leftColor, rightColor, upColor, downColor;		
	}
}
