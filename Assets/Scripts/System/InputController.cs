using UnityEngine;
using System.Collections;
using InControl;

public class InputController : Singleton<InputController> {
	
	private Vector2 leftStickVector = Vector2.zero;
	private Vector2 rightStickVector = Vector2.zero;

	private Vector2 previousLeftStickVector = Vector2.zero;
	private Vector2 previousRightStickVector = Vector2.zero;

	private Vector2 dPadVector = Vector2.zero;
	private Vector2 direction = Vector2.zero;

	public JoystickParameters leftStick;
	public JoystickParameters rightStick;

	private float timeParameter = 0;

	InputDevice activeDevice;

	// Use this for initialization
	void Start () {
	
		Application.targetFrameRate = -1;
		InputManager.Setup();

		InputManager.OnDeviceAttached += (InputDevice obj) => OnNewDeviceAttached(obj);
		InputManager.OnDeviceDetached += (InputDevice obj) => OnNewDeviceDetached(obj);
		InputManager.OnActiveDeviceChanged += (InputDevice obj) => OnNewDeviceChanged(obj);
	}
	
	// Update is called once per frame
	void Update () {
		InputManager.Update();

		activeDevice = InputManager.ActiveDevice;
		leftStickVector = activeDevice.LeftStick.Vector;
		rightStickVector = activeDevice.RightStick.Vector;
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
		
		if(Mathf.Abs(xValue) < leftStick.deadZone.x)
		{
			xValue = 0;
		}
		
		if(Mathf.Abs(yValue) < leftStick.deadZone.y)
		{
			yValue = 0;
		}
		
		vectorToReturn = new Vector2(xValue * leftStick.sensitivity.x, yValue * leftStick.sensitivity.y);
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

		if(Mathf.Abs(xValue) < rightStick.deadZone.x)
		{
			xValue = 0;
		}

		if(Mathf.Abs(yValue) < rightStick.deadZone.y)
		{
			yValue = 0;
		}

		vectorToReturn = new Vector2(xValue * rightStick.sensitivity.x, yValue * rightStick.sensitivity.y);
		vectorToReturn = Vector2.ClampMagnitude(vectorToReturn, 1);

		return vectorToReturn;
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
		public Vector2 deadZone;
	}

	public InputControl GetControl(InputControlType type)
	{
		return activeDevice.GetControl(type);
	
	}
}
