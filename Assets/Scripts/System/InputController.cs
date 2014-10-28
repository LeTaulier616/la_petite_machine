using UnityEngine;
using System.Collections;
using InControl;

public class InputController : Singleton<InputController> {
	
	private Vector2 leftStickVector = Vector2.zero;
	private Vector2 rightStickVector = Vector2.zero;
	private float leftTriggerValue = 0.0f;
	private float rightTriggerValue = 0.0f;

	private Vector2 previousLeftStickVector = Vector2.zero;
	private Vector2 previousRightStickVector = Vector2.zero;

	private Vector2 dPadVector = Vector2.zero;
	private Vector2 direction = Vector2.zero;

	[Tooltip("Input settings for the left stick")]
	public JoystickParameters leftStick;
	
	[Tooltip("Input settings for the right stick")]
	public JoystickParameters rightStick;
	
	[Tooltip("Input settings for the left trigger")]
	public TriggerParameters leftTrigger;
	
	[Tooltip("Input settings for the right trigger")]
	public TriggerParameters rightTrigger;
	
	[Tooltip("Input controls for the player are nested here:\n\nAction1: A / Cross\nAction2: B / Circle\nAction3: X / Square\nAction4: Y / Triangle\n\nLeftStickX, Y and LeftStickButton\nRightStickX, Y and RightStickButton\n\nDPadUp, Down, Left, Right\n\nRightBumper: RB / R1\nLeftBumper: LB / L1\n\nRightTrigger: RT / R2\nLeftTrigger: LT / L2")]
	public PlayerControls controls;

	private float timeParameter = 0;

	public InputDevice activeDevice;

	// Use this for initialization
	void Start () {
	
		Application.targetFrameRate = -1;
		InputManager.Setup();

		InputManager.OnDeviceAttached += (InputDevice obj) => OnNewDeviceAttached(obj);
		InputManager.OnDeviceDetached += (InputDevice obj) => OnNewDeviceDetached(obj);
		InputManager.OnActiveDeviceChanged += (InputDevice obj) => OnNewDeviceChanged(obj);
	}
	
	// Update is called once per frame
	void Update () 
	{
		InputManager.Update();
		
		activeDevice = InputManager.ActiveDevice;
		leftStickVector = activeDevice.LeftStick.Vector;
		rightStickVector = activeDevice.RightStick.Vector;
		
		leftTriggerValue = activeDevice.LeftTrigger.Value;
		rightTriggerValue = activeDevice.RightTrigger.Value;
				
		dPadVector = activeDevice.DPad.Vector;
		direction = activeDevice.Direction.Vector;
	}

	public Vector2 LeftStick()
	{
		Vector2 vectorToReturn = Vector2.zero;
		float xValue = leftStickVector.x;
		float yValue = leftStickVector.y;

		float rawXValue = xValue;
		float rawYValue = yValue;
		
		if(Mathf.Abs(xValue) < leftStick.deadZone)
		{
			xValue = 0;
		}
		
		if(Mathf.Abs(yValue) < leftStick.deadZone)
		{
			yValue = 0;
		}
		
		//vectorToReturn = new Vector2(xValue * leftStick.sensitivity.x, yValue * leftStick.sensitivity.y);
		vectorToReturn = new Vector2(xValue, yValue);
		vectorToReturn = vectorToReturn.normalized * ((vectorToReturn.magnitude - leftStick.deadZone) / (1 - leftStick.deadZone));
		vectorToReturn.x *= leftStick.sensitivity.x;
		vectorToReturn.y *= leftStick.sensitivity.y;
		
		vectorToReturn = Vector2.ClampMagnitude(vectorToReturn, 1);
		Vector2 vectorWithLerp = vectorToReturn;

		/*if(vectorToReturn.x == 0)
		{

			if(timeParameter < 1)
			{
				timeParameter += Time.deltaTime;
				vectorWithLerp = new Vector2(Mathf.Lerp(previousLeftStickVector.x, 0, timeParameter *0.1f), vectorToReturn.y);
				Debug.Log(vectorWithLerp.x);
			}
			else
			{
				timeParameter = 0;
			}

		}*/

		previousLeftStickVector = vectorWithLerp;

		return vectorWithLerp;
	}


	public Vector2 RightStick()
	{
		Vector2 vectorToReturn = Vector2.zero;
		float xValue = rightStickVector.x;
		float yValue = rightStickVector.y;

		if(Mathf.Abs(xValue) < rightStick.deadZone)
		{
			xValue = 0;
		}

		if(Mathf.Abs(yValue) < rightStick.deadZone)
		{
			yValue = 0;
		}

		//vectorToReturn = new Vector2(xValue * rightStick.sensitivity.x, yValue * rightStick.sensitivity.y);
		vectorToReturn = new Vector2(xValue, yValue);
		vectorToReturn = vectorToReturn.normalized * ((vectorToReturn.magnitude - rightStick.deadZone) / (1 - rightStick.deadZone));
		vectorToReturn.x *= rightStick.sensitivity.x;
		vectorToReturn.y *= rightStick.sensitivity.y;
		
		vectorToReturn = Vector2.ClampMagnitude(vectorToReturn, 1);

		return vectorToReturn;
	}
	
	public float LeftTrigger()
	{
		float valueToReturn = 0.0f;
		
		valueToReturn = leftTriggerValue;
		
		if(valueToReturn < leftTrigger.deadZone)
		{
			valueToReturn = 0.0f;
		}
		
		return valueToReturn;
	}
	
	public float RightTrigger()
	{
		float valueToReturn = 0.0f;
		
		valueToReturn = rightTriggerValue;
		
		if(valueToReturn < rightTrigger.deadZone)
		{
			valueToReturn = 0.0f;
		}
		
		return valueToReturn;
	}

	private void OnNewDeviceAttached(InputDevice newlyAttachedDevice)
	{
	
	}

	private void OnNewDeviceDetached(InputDevice newlyDetachedDevice)
	{

	}

	private void OnNewDeviceChanged(InputDevice newlyChangedDevice)
	{

	}

	[System.Serializable]
	public class JoystickParameters
	{
		public Vector2 sensitivity;
		public float deadZone;
	}
	
	[System.Serializable]
	public class TriggerParameters
	{
		public float deadZone;
	}

	public InputControl GetControl(InputControlType type)
	{
		return activeDevice.GetControl(type);
	
	}
	
	[System.Serializable]
	public class PlayerControls
	{
		[Tooltip("Button used for jump action.")]
		public InputControlType jump;
		
		[Tooltip("Button used for using an object / interaction.")]
		public InputControlType use;
		
		[Tooltip("Button used to aim")]
		public InputControlType aim;
		
		[Tooltip("Button used to fire")]
		public InputControlType fire;
		
		[Tooltip("Button used to change firing item - left")]
		public InputControlType switchLeft;
		
		[Tooltip("Button used to change firing item - right")]
		public InputControlType switchRight;
		
		[Tooltip("Button used to change form - up")]
		public InputControlType switchUp;
		
		[Tooltip("Button used to change form - down")]
		public InputControlType switchDown;
	}
}
