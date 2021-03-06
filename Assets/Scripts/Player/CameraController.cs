﻿using UnityEngine;
using System.Collections;

public class CameraController : Singleton<CameraController>
{
	public CameraParameters cameraParameters;
	
	public enum Side
	{
		Left,
		Right,
		Center
	}

	private Side currentSide;

	private Vector3 currentCameraOffset;
	private Vector3 defaultEulerAngles;

	private Transform previousFrameTransform;
	private Vector3 previousFramePosition;

	private bool isMoving = false;
	private float newX, newY, newZ;
	private Vector3 cameraDifferenceFromTargetPos;
	private Vector3 targetCamPos;
	private float cameraOrientationLerpTime = 0f;

	private float previousNewY = 0;
	private float previousNewX = 0;
	private float leftThresholdScreenVal = 0;
	private float rightThresholdScreenVal = 0; 
	private float durationInNoMovementZone = 0;
	private float durationInLeftZone = 0f;
	private float durationInRightZone = 0f;
	
	private Vector3 rightStickCamOffset;
	private PlayerController.Direction rightStickSwipeDirection;
	private PlayerController.Direction startedSwipeDirection;
	private float rightStickMovedDuration = 0;
	
	private bool canLook;
		
	void Start()
	{
		rightStickSwipeDirection = PlayerController.Direction.None;
		rightStickMovedDuration = 0;
		startedSwipeDirection = PlayerController.Direction.None;

		leftThresholdScreenVal = (Screen.width *0.5f) - cameraParameters.leftThreshold;
		rightThresholdScreenVal = (Screen.width *0.5f) + cameraParameters.rightThreshold; 
		currentSide = Side.Center;
		newX = transform.position.x;
		newY = transform.position.y;
		previousNewY = newY;
		previousNewX = newX;
		newZ = transform.position.z;
		cameraDifferenceFromTargetPos = Vector3.zero;
		targetCamPos = transform.position;
		currentCameraOffset = new Vector3(0,cameraParameters.cameraOffsetMovingLeft.y,0);
		defaultEulerAngles = transform.eulerAngles;
		cameraOrientationLerpTime = 0f;
		
		canLook = true;
		
		if(cameraParameters.target == null)
		{
			ResetToFollowPlayer();
		}
	}

	void Update()
	{
		if(canLook)
			CheckRightStickSwipe();
			
		UpdateCameraMovement();
	}

	void LateUpdate()
	{

	}

	private void UpdateCameraMovement()
	{
		if(cameraParameters.target != null)
		{
			if(canLook)
			{
				rightStickCamOffset = new Vector3(InputController.Instance.RightStick().x * cameraParameters.rightStickLookDistance.x,
				                                  InputController.Instance.RightStick().y * cameraParameters.rightStickLookDistance.y,
				                                  cameraParameters.rightStickLookDistance.z);
				 
				if(InputController.Instance.RightStick().y < 0.0f)
				{
					rightStickCamOffset.y *= cameraParameters.rightStickDownMultiplier;
				} 
				
				rightStickCamOffset *= GameController.DeltaTime();
			}
			 
			Vector3 movementVector = Vector3.zero;
			Vector3 targetWorldPos = cameraParameters.target.position;
			Vector3 targetScreenPoint = camera.WorldToScreenPoint(targetWorldPos);
			targetScreenPoint = new Vector3(targetScreenPoint.x, Screen.height - targetScreenPoint.y, targetScreenPoint.z);

			targetCamPos = ((targetWorldPos + currentCameraOffset) - new Vector3(0,0, cameraParameters.zDistanceFromTarget));
			cameraDifferenceFromTargetPos = targetCamPos - transform.position;
			
			newX = transform.position.x;
			newY = transform.position.y;

			bool followTarget = false;

			if(currentSide == Side.Center)
			{
				currentSide = Side.Right;
				currentCameraOffset = cameraParameters.cameraOffsetMovingRight;
			}
		
			if((targetScreenPoint.x < leftThresholdScreenVal || targetScreenPoint.x > rightThresholdScreenVal) || rightStickSwipeDirection != PlayerController.Direction.None)
			{
				durationInNoMovementZone = 0;
				
				if((targetScreenPoint.x > rightThresholdScreenVal && Mathf.Abs(cameraDifferenceFromTargetPos.x) > cameraParameters.horizontalDeadZone) || rightStickSwipeDirection == PlayerController.Direction.Right)
				{

					if((PlayerController.Instance.GetHorizontalDirection() == PlayerController.Direction.Right && PlayerController.Instance.GetLeftStickPositiveDuration() > 0.25f) || rightStickSwipeDirection == PlayerController.Direction.Right)
						{
							if(currentSide != Side.Right)
							{
								currentSide = Side.Right;
							currentCameraOffset = cameraParameters.cameraOffsetMovingRight;
							}
						}

				}
				else if((targetScreenPoint.x < leftThresholdScreenVal && Mathf.Abs(cameraDifferenceFromTargetPos.x) > cameraParameters.horizontalDeadZone) || rightStickSwipeDirection == PlayerController.Direction.Left)
				{
				
					if((PlayerController.Instance.GetHorizontalDirection() == PlayerController.Direction.Left && PlayerController.Instance.GetLeftStickNegativeDuration() > 0.25f) || rightStickSwipeDirection == PlayerController.Direction.Left)
						{
							if(currentSide != Side.Left)
							{
								currentSide = Side.Left;
							currentCameraOffset = cameraParameters.cameraOffsetMovingLeft;
							}
						}
					
				}

				leftThresholdScreenVal = targetScreenPoint.x - cameraParameters.leftThreshold;
				rightThresholdScreenVal = targetScreenPoint.x + cameraParameters.rightThreshold;
				rightStickSwipeDirection = PlayerController.Direction.None;
			}

			newX = transform.position.x + (NGUIMath.SpringLerp(0, (Mathf.Abs(cameraDifferenceFromTargetPos.x) > cameraParameters.horizontalDeadZone ? cameraDifferenceFromTargetPos.x : 0f), 0.8f, GameController.DeltaTime() *3));

			if(Mathf.Abs(cameraDifferenceFromTargetPos.y) <= cameraParameters.topThreshold)
			{
				if(!isMoving && InputController.Instance.RightStick() == Vector2.zero)
				{
					newY = previousNewY;
				}
				isMoving = false;
			}
			else
			{
				isMoving = true;
				if(InputController.Instance.RightStick() == Vector2.zero)
					newY = transform.position.y + (NGUIMath.SpringLerp(0, cameraDifferenceFromTargetPos.y, 0.3f, GameController.DeltaTime())* ((cameraDifferenceFromTargetPos.y > 0 ? cameraParameters.verticalUpSpeed : cameraParameters.verticalDownSpeed) * 4f));
				else
					newY = transform.position.y + (NGUIMath.SpringLerp(0, cameraDifferenceFromTargetPos.y, 0.3f, GameController.DeltaTime())* cameraParameters.horizontalMovementSpeed);
				previousNewY = newY;
			}

			movementVector += new Vector3((newX - transform.position.x), 0, 0);
			movementVector += new Vector3(0, newY - transform.position.y, 0);
			movementVector += new Vector3(0, 0, cameraDifferenceFromTargetPos.z * (GameController.DeltaTime() * cameraParameters.depthMovementSpeed));

			transform.position += movementVector;

			if(InputController.Instance.RightStick() != Vector2.zero)
				transform.position += rightStickCamOffset;
		}
	}

	private void CheckRightStickSwipe()
	{
		Vector2 currentRightStickValue = InputController.Instance.RightStick();
		if(currentRightStickValue != Vector2.zero)
		{
			rightStickMovedDuration += GameController.DeltaTime();
			if(rightStickSwipeDirection == PlayerController.Direction.None && startedSwipeDirection == PlayerController.Direction.None)
			{
				startedSwipeDirection = (currentRightStickValue.x > 0 ? PlayerController.Direction.Right : PlayerController.Direction.Left);
			}
		}
		else
		{
			rightStickSwipeDirection = PlayerController.Direction.None;

			if(rightStickMovedDuration > 0.02f && rightStickMovedDuration < 0.25f)
			{
				if(startedSwipeDirection != PlayerController.Direction.None)
				{
					rightStickSwipeDirection = startedSwipeDirection; 
				}
			}

			rightStickMovedDuration = 0;
			startedSwipeDirection = PlayerController.Direction.None;
		}
	}

	public void SetTarget(Transform newTarget)
	{
		if(newTarget != cameraParameters.target)
		{
			cameraParameters.target = newTarget;
		}
	}

	public void ResetToFollowPlayer()
	{
		SetTarget(PlayerController.Instance.transform);
		isMoving = false;
	}
	
	public void CanLook(bool state)
	{
		canLook = state;
	}
	
	[System.Serializable]
	public class CameraParameters
	{
		public Transform target;
		public float zDistanceFromTarget = 12.0f;
		
		public Vector3 cameraOffsetMovingRight;
		public Vector3 cameraOffsetMovingLeft;
		
		public float horizontalMovementSpeed = 8.0f;
		public float verticalUpSpeed = 6.0f;
		public float verticalDownSpeed = 6.0f;
		public float depthMovementSpeed = 5.0f;
		
		public float topThreshold;
		public float bottomThreshold;
		public float rightThreshold;
		public float leftThreshold;
		
		public float horizontalDeadZone = 1.0f;
		
		public Vector3 rightStickLookDistance;
		public float rightStickDownMultiplier = 0.5f;
	}
}
