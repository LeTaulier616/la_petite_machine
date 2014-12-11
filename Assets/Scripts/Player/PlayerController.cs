using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : Singleton<PlayerController> {
	
	[HideInInspector]
	public CharacterController characterController;
	
	[HideInInspector]
	public LayerMask defaultLayer;
		
	[Tooltip("All spawn-related parameters are nested here.")]
	public SpawnParameters spawn;
		
	[Tooltip("All movement-related parameters are nested here.")]
	public MovementParameters movement;

	[Tooltip("All Lane-Movement related parameters are nested here.")]
	public LaneParameters lane;

	[Tooltip("All shooting related parameters are nested here.")]
	public ShotParameters shot;
	
	private bool isMoving = false;
	private bool isAiming = false;
	
	private float leftStickYMovedDuration = 0f;
	private float leftStickMovedDuration = 0f;
	private Direction startedSwipeDirection = Direction.None;
	private Direction leftStickSwipeDirection = Direction.None;
	private bool leftStickHasBeenReleased = true;
	private bool leftStickYHasBeenReleased = true;

	private bool applyGravity = true;
	private bool jumpWasReleased = false;
	private bool _falling = false;

	private bool shootWasReleased = false;
	
	private float timeInAir = 0;
	private float jumpTime = 0;
	private float currentHeightJumped = 0;
	private float moveTime = 0;
	private bool canAim = true;
	private bool canShoot = false;
	
	private float leftStickPositiveDuration = 0;
	private float leftStickNegativeDuration = 0;
	private float leftStickZeroDuration = 0;
	
	private bool currentlyGrounded = false;
	private bool previousGroundedState = false;
	private float idleTime = 0;
	private float previousFallingSpeedMultiplier = 0;
	private float fallingSpeedMultiplier = 0;
	private float risingSpeedMultiplier = 0;
	private float horizontalSpeedMultiplier = 0;
	private float horizontalGroundDecelerationMultiplier = 0;
	private float horizontalAirDecelerationMultiplier = 0;
	private float currentHorizontalMovementFactor = 0;
	
	private Vector3 currentPosition;
	private Vector3 previousFramePosition;
	private Vector3 deltaPosition;
	
	private Vector3 previousMovementVector;
	private Vector3 movementVector;
	private float timeParameter = 0;
	
	private bool stickToTheGround = false;
	
	private Vector2 leftStick;
	private float leftTrigger; 
	private float rightTrigger;
	
	[HideInInspector]
	public StateMachine stateMachine;
	public IdleState idle;
	public MoveState moving;
	public JumpState jumping;
	public FallState falling;
	//public AimState aiming;
	
	private bool canMove;
	private bool canJump;
	
	[HideInInspector]
	public Direction horizontalDirection;
	[HideInInspector]
	public Direction previousHorizontalDirection;
	[HideInInspector]
	public Direction jumpDirection;
	
	private Vector2 previousFrameLeftStick;
	private Vector2 leftStickDelta;

	private float timeMovingLeft = 0;
	private float timeMovingRight = 0;
	[HideInInspector]
	public bool interruptJump = false;

	private bool inColorZone = false;

	//[HideInInspector]
	//public ColorController.ColorChoice cannonColor;

	[HideInInspector]
	public GameObject playerMesh;

	private void Start()
	{
		characterController = GetComponent<CharacterController>();
		
		stateMachine = NGUITools.AddMissingComponent<StateMachine>(gameObject);
		
		idle = new IdleState();
		moving = new MoveState();
		jumping = new JumpState();
		falling = new FallState();
		//aiming = new AimState();
				
		stateMachine.SetState(idle);

		CanMove(true);
		CanJump(true);
		CameraController.Instance.CanLook(true);

		playerMesh = transform.Find("Mesh").gameObject;
	}
	

	private void Update()
	{
		UpdateEarlyVariables();
		Ray ray1 = new Ray(transform.position + new Vector3(characterController.radius - 0.1f, characterController.height *0.4f,0), transform.TransformDirection(Vector3.forward));
		Ray ray2 = new Ray(transform.position + new Vector3(-characterController.radius +0.1f, characterController.height *0.4f,0), transform.TransformDirection(Vector3.forward));
		Ray ray3 = new Ray(transform.position + new Vector3(characterController.radius -0.1f, -characterController.height *0.4f,0), transform.TransformDirection(Vector3.forward));
		Ray ray4 = new Ray(transform.position + new Vector3(-characterController.radius +0.1f, -characterController.height *0.4f,0), transform.TransformDirection(Vector3.forward));

		Debug.DrawRay(ray1.origin, ray1.direction, Color.red);
		Debug.DrawRay(ray2.origin, ray2.direction, Color.red);
		Debug.DrawRay(ray3.origin, ray3.direction, Color.red);
		Debug.DrawRay(ray4.origin, ray4.direction, Color.red);

	}
	
	
	private void UpdateEarlyVariables()
	{
		leftStickDelta = new Vector2(Mathf.Abs(InputController.Instance.LeftStick().x) - Mathf.Abs(leftStick.x), Mathf.Abs(InputController.Instance.LeftStick().y) - Mathf.Abs(leftStick.y));
		leftStick = InputController.Instance.LeftStick();
		leftTrigger = InputController.Instance.LeftTrigger();
		rightTrigger = InputController.Instance.RightTrigger();
		movementVector = Vector3.zero;
		currentPosition = transform.position;
		deltaPosition = currentPosition - previousFramePosition;
		
		// REPLACE BY RAYCAST
		currentlyGrounded = IsGrounded();
		
		if(!InputController.Instance.GetControl(InputController.Instance.controls.jump).IsPressed)
		{
			jumpWasReleased = true;
		}

		if(!InputController.Instance.GetControl(InputController.Instance.controls.fire).IsPressed)
		{
			shootWasReleased = true;
		}
		
		if(!currentlyGrounded)
		{
			timeInAir += GameController.DeltaTime();
		}
		else 
		{
			timeInAir = 0;
		}
		
		if(leftStick.x != 0)
		{
			idleTime = 0;
			
			leftStickZeroDuration = 0;
			if(leftStick.x > 0)
			{
				leftStickPositiveDuration += GameController.DeltaTime();
				leftStickNegativeDuration = 0;
			}
			else if(leftStick.x < 0)
			{
				leftStickNegativeDuration += GameController.DeltaTime();
				leftStickPositiveDuration = 0;
			}
		}
		else
		{
			idleTime += GameController.DeltaTime();
			ResetMovementValues();
			leftStickNegativeDuration = 0;
			leftStickPositiveDuration = 0;
			leftStickZeroDuration += GameController.DeltaTime();
		}
		
		
		currentHorizontalMovementFactor = 0;
		if(leftStick.x == 0 && leftStickZeroDuration > 0.1f)
		{
			idleTime += GameController.DeltaTime();
			currentHorizontalMovementFactor += (GameController.DeltaTime() * (IsGrounded() ? movement.horizontalGroundStopSpeed : movement.horizontalAirStopSpeed));
			if(currentHorizontalMovementFactor >= 1)
			{
				currentHorizontalMovementFactor = 1;
			}
		}
		else
		{
			if(leftStickNegativeDuration > 0.1f || leftStickPositiveDuration > 0.1f)
				currentHorizontalMovementFactor = 1;
			idleTime = 0;
		}
		
		fallingSpeedMultiplier = movement.fallingAcceleration.Evaluate(timeInAir);
		risingSpeedMultiplier = movement.riseAcceleration.Evaluate(GetCurrentHeightJumpedRatio());
		horizontalSpeedMultiplier = movement.horizontalAcceleration.Evaluate(moveTime);

		CheckChangeLane();
	}
	
	public void SetJumpDirection(Direction newJumpDirection)
	{
		jumpDirection = newJumpDirection;
	}
	
	public bool MovementRequested()
	{
		bool movementRequested = false;
		
		if(Mathf.Abs(PlayerController.Instance.leftStick.x) > 0)
		{
			movementRequested = true;
		}
		
		return movementRequested;
	}

	private void CheckChangeLane()
	{
		// Check left stick swipe
		Vector2 currentLeftStickValue = InputController.Instance.LeftStick();
		//lane.changeLaneRequested = false;

		if(currentLeftStickValue != Vector2.zero)
		{
			leftStickMovedDuration += GameController.DeltaTime();

			if(Mathf.Abs(currentLeftStickValue.y) > 0.4f)
			{
				leftStickYMovedDuration += GameController.DeltaTime();
			}
			else
			{
				leftStickYMovedDuration = 0f;
				leftStickYHasBeenReleased = true;
			}

			if((leftStickSwipeDirection == PlayerController.Direction.None) && (Mathf.Abs(currentLeftStickValue.y) > 0.4f))
			{
				startedSwipeDirection = (currentLeftStickValue.y > 0 ? PlayerController.Direction.Up : PlayerController.Direction.Down);
			}

			leftStickSwipeDirection = PlayerController.Direction.None;


			if(leftStickYMovedDuration < 0.5f && leftStickYMovedDuration > 0.05f && (Mathf.Abs(currentLeftStickValue.y) > 0.4f) && leftStickYHasBeenReleased)
			{
				if(startedSwipeDirection != PlayerController.Direction.None)
				{
					leftStickSwipeDirection = startedSwipeDirection;
					startedSwipeDirection = Direction.None;
					leftStickYHasBeenReleased = false;
					leftStickYMovedDuration = 0f;
				}
			}
		}
		else
		{
			
			leftStickYMovedDuration = 0;
			leftStickMovedDuration = 0f;
			leftStickSwipeDirection = Direction.None;
			startedSwipeDirection = PlayerController.Direction.None;
			leftStickHasBeenReleased = true;
			leftStickYHasBeenReleased = true;
		}


		Direction newChangeLaneDrection = lane.laneChangeDirection;
		PlayerController.Lane newTargetLane = lane.targetLane;

		if(!lane.changeLaneRequested)
		{
			//lane.changeLaneOnceAgain = false;
			// If change lane requested
			if(((leftStickSwipeDirection == Direction.Up || leftStickSwipeDirection == Direction.Down) && lane.manualLaneChangeAllowed) || (lane.forceLaneChange && lane.forceLaneChangeDirection != Direction.None))
			{
				newTargetLane = GetLane((lane.forceLaneChange ? lane.forceLaneChangeDirection : leftStickSwipeDirection));
				StopCoroutine("AutomaticLaneChangeRoutine");

				if(newTargetLane != lane.currentLane && newTargetLane != Lane.None && (lane.forceLaneChange || LaneIsFree(newTargetLane)))
				{
					lane.changeLaneRequested = true;
					lane.laneChangeDirection = (lane.forceLaneChange ? lane.forceLaneChangeDirection : leftStickSwipeDirection);

					lane.targetLane = newTargetLane;
					lane.zAtSwitchStart = transform.position.z;

					lane.targetZ = transform.position.z + (lane.laneChangeDirection == Direction.Up ? lane.zOffsetValue : -lane.zOffsetValue);
				}

				lane.forceLaneChange = false;

				leftStickSwipeDirection = Direction.None;
				startedSwipeDirection = Direction.None;
			}
		}
		else
		{
			if(leftStickSwipeDirection != Direction.None && lane.automaticLaneChangeAllowed)
			{
				lane.changeLaneOnceAgain = true;
				lane.forceLaneChangeDirection = leftStickSwipeDirection;
			}
		}
		

	}


	public bool LaneIsFree(Lane laneToCheck)
	{
		bool laneIsFree = true;
		Direction directionToCheck = Direction.None;

		directionToCheck = (lane.forceLaneChange ? lane.forceLaneChangeDirection : leftStickSwipeDirection );

		/*if(directionToCheck != Direction.None)
		{*/
			Vector3 raycastDirection = Vector3.zero;

			switch (directionToCheck)
			{
			case Direction.Up:
				raycastDirection = transform.TransformDirection(Vector3.forward);
				break;
			case Direction.Down:
				raycastDirection = transform.TransformDirection(Vector3.back);
				break;
			default:
				break;
			}

			lane.blockedAreaFeet = LaneParameters.BlockArea.None;
			lane.blockedAreaHead = LaneParameters.BlockArea.None;


			float distanceToCheck = 3;

			Vector3 rayOffset = Vector3.zero;

			for(int i = 0; i < 4; i++)
			{
				if(i == 0)
					rayOffset = new Vector3(characterController.radius -0.1f, characterController.height *0.5f,0);
				else if(i == 1)
					rayOffset = new Vector3(-characterController.radius +0.1f, characterController.height *0.5f,0);
				else if(i == 2)
					rayOffset = new Vector3(characterController.radius -0.1f, -characterController.height *0.5f,0);
				else if(i == 3)
					rayOffset = new Vector3(-characterController.radius +0.1f, -characterController.height *0.5f,0);

				Ray ray = new Ray(transform.position + rayOffset, raycastDirection);
				RaycastHit hit;
				
				if(Physics.Raycast(ray, out hit, distanceToCheck, defaultLayer))
				{
					laneIsFree = false;
					switch(i)
					{
					case 0:
						lane.blockedAreaHead = LaneParameters.BlockArea.Right;
						break;
					case 1:
						if(lane.blockedAreaHead == LaneParameters.BlockArea.None)
							lane.blockedAreaHead = LaneParameters.BlockArea.Left;
						else
							lane.blockedAreaHead = LaneParameters.BlockArea.Both;
						break;
					case 2:
						lane.blockedAreaFeet = LaneParameters.BlockArea.Right;
						break;
					default:
						if(lane.blockedAreaFeet == LaneParameters.BlockArea.None)
							lane.blockedAreaFeet = LaneParameters.BlockArea.Left;
						else
							lane.blockedAreaFeet = LaneParameters.BlockArea.Both;
						break;
					}
				}
				else
				{
				}
			}
		/*}
		else laneIsFree = false;*/

		return laneIsFree;
	}

	public void AutomaticLaneChangeCheck()
	{
		if(lane.automaticLaneChangeAllowed)
		{

			List<float> validPointsHeights = new List<float>();
			List<Lane> validLanes = new List<Lane>();
			List<Direction> validDirections = new List<Direction>();

			for(int i = 0; i < 3; i++)
			{
				Lane automaticTargetLane = Lane.Front;
				Direction automaticTargetLaneDirection = Direction.None;
				bool validAutomaticLaneSwitching = false;

				if(i == 1)
					automaticTargetLane = Lane.Center;
				else if(i == 2)
					automaticTargetLane = Lane.Back;

				automaticTargetLaneDirection = GetDirectionForLane(automaticTargetLane);

				if(automaticTargetLaneDirection != Direction.None)
				{
					if(lane.currentLane != automaticTargetLane && LaneIsFree(automaticTargetLane))
					{
						Ray targetLaneHeightRay = new Ray(new Vector3(transform.position.x, (transform.position.y + (characterController.height*0.5f) + movement.jumpHeight), GetLaneTargetZ(automaticTargetLane)),
						                                  Vector3.down);
						RaycastHit hit;

						if(Physics.Raycast(targetLaneHeightRay, out hit, 100, defaultLayer))
						{
							if(hit.point.y > transform.position.y)
							{
								if(Vector3.Distance(new Vector3 (0, hit.point.y, 0), new Vector3(0,(transform.position.y - (characterController.height*0.5f)),0)) > 0.1f &&
								   Vector3.Distance(new Vector3 (0, hit.point.y, 0), new Vector3(0,(transform.position.y - (characterController.height*0.5f)),0)) < (movement.jumpHeight - currentHeightJumped) +0.1f)
								{
									bool enoughSpace = true;
									Ray enoughSpaceToFitRay = new Ray(hit.point, Vector3.up);

									RaycastHit hit2;
									if(Physics.Raycast(enoughSpaceToFitRay, out hit2, characterController.height+0.08f, defaultLayer))
									{
										enoughSpace = false;
									}
							
									if(enoughSpace)
									{
										validAutomaticLaneSwitching = true;

										AutoLaneSwitchSpecific specificBehavior = hit.collider.GetComponent<AutoLaneSwitchSpecific>();
										if(specificBehavior != null)
										{
											if(specificBehavior.ignoredByAutoSwitch)
												validAutomaticLaneSwitching = false;
										}

									}
								}
							}
						}

						if(validAutomaticLaneSwitching)
						{
							validPointsHeights.Add(hit.point.y);
							validLanes.Add(automaticTargetLane);
							validDirections.Add(automaticTargetLaneDirection);
						}
					}
				}
			}

			if(validPointsHeights != null && validPointsHeights.Count > 0)
			{
				int highestPointIndex = 0;
				float currentHighestPoint = -100f;
				int u = -1;

				foreach(float currentValidHeight in validPointsHeights)
				{
					u++;

					if(currentValidHeight > currentHighestPoint)
					{
						currentHighestPoint = currentValidHeight;
						highestPointIndex = u;
					}

					Debug.Log(validLanes[u] + " is a valid lane with height of " + currentValidHeight);
				}

				Debug.Log("Highest: " + validLanes[highestPointIndex] + " with " + currentHighestPoint);

				Debug.Log("Automatic change to: " + validLanes[highestPointIndex].ToString());
				 
				lane.automaticChangeLaneDirection = validDirections[highestPointIndex];

				StopCoroutine("AutomaticLaneChangeRoutine");
				StartCoroutine("AutomaticLaneChangeRoutine");
			}
		}
	}

	public Direction GetDirectionForLane(Lane targetedLane)
	{
		Direction dirToReturn = Direction.None;

		switch (targetedLane)
		{
		case Lane.Back:
			switch(lane.currentLane)
			{
			case Lane.Center:
				dirToReturn = Direction.Up;
				break;
			default:
				break;
			}
			break;
		case Lane.Center:
			switch(lane.currentLane)
			{
			case Lane.Back:
				dirToReturn = Direction.Down;
				break;
			case Lane.Front:
				dirToReturn = Direction.Up;
				break;
			default:
				break;
			}
			break;
		case Lane.Front:
			switch(lane.currentLane)
			{
			case Lane.Center:
				dirToReturn = Direction.Down;
				break;
			default:
				break;
			}
			break;
		default:
			break;
		}

		return dirToReturn;
	}

	private IEnumerator AutomaticLaneChangeRoutine()
	{
		//Debug.Log("Automatic Lane Change now!");
		AuthorizeAutomaticLaneChange(false);
		lane.changeLaneOnceAgain = false;
		lane.changeLaneRequested = false;
		lane.laneChangeDirection = Direction.None;
		lane.forceLaneChangeDirection = Direction.None;

		yield return new WaitForSeconds(0.05f);
		lane.forceLaneChange = true;
		lane.forceLaneChangeDirection = lane.automaticChangeLaneDirection;
		AuthorizeManualLaneChange(false);
	}

	public void AuthorizeAutomaticLaneChange(bool newState)
	{
		lane.automaticLaneChangeAllowed = newState;
	}

	public void AuthorizeManualLaneChange(bool newState)
	{
		lane.manualLaneChangeAllowed = newState;
	}


	public Lane GetLane(Direction relativeDirection)
	{
		Lane laneToReturn = lane.currentLane;

		switch(laneToReturn)
		{

		case Lane.Back:
			if(relativeDirection == Direction.Down)
			{
				laneToReturn = Lane.Center;
			}
			break;

		case Lane.Center:
			if(relativeDirection == Direction.Up)
			{
				laneToReturn = Lane.Back;
			}
			else if(relativeDirection == Direction.Down)
			{
				laneToReturn = Lane.Front;
			}
			break;

		case Lane.Front:
			if(relativeDirection == Direction.Up)
			{
				laneToReturn = Lane.Center;
			}
			break;
		default:
			laneToReturn = Lane.None;
			break;
		}

		return laneToReturn;
	}


	public void UpdateLaneChangeMovement()
	{
		if(lane.changeLaneRequested)
		{

			float zSpeed = lane.zSpeed;

			if(Vector3.Distance(transform.position, new Vector3(transform.position.x, transform.position.y, lane.targetZ)) > 0.1f)
			{
				bool wentTooFar = false;

				zSpeed *= (((transform.position.z - lane.targetZ < 0) ? 1 : -1) * GameController.DeltaTime());

				previousFallingSpeedMultiplier = fallingSpeedMultiplier;
				//fallingSpeedMultiplier = 0.005f;

				/*if(zSpeed > 0)
				{
					if(transform.position.z - lane.targetZ >= 0)
						wentTooFar = true;
				}
				else if(zSpeed < 0)
				{
					if(Mathf.Abs(transform.position.z) - Mathf.Abs(lane.targetZ) >= 0)
						wentTooFar = true;
				}*/

				/*if(!LaneIsFree(lane.targetLane))
				{
					CancelCurrentLineChange();
				}*/

				if(!wentTooFar)
				{
					movementVector += new Vector3(0,0, zSpeed);
				}
				else
					ArrivedAtNewLane();
			}
			else
			{
				ArrivedAtNewLane();
			}
		}
	}

	public void CancelCurrentLineChange()
	{
		Debug.Log("CancelCurrentLineChange!");
		lane.targetZ = GetLaneTargetZ(lane.currentLane);
		/*lane.changeLaneRequested = false;
		lane.forceLaneChange = true;
		lane.targetLane = lane.currentLane;*/
		//lane.forceLaneChangeDirection = GetOppositeChangeLaneDirection();

		//lane.targetZ += (lane.forceLaneChangeDirection == Direction.Up ? lane.zOffsetValue : -lane.zOffsetValue);
		
	}

	public float GetLaneTargetZ(Lane laneToGet)
	{
		float zToReturn = 0f;
		switch(laneToGet)
		{
		case Lane.Back:
			zToReturn = 3;
			break;
		case Lane.Center:
			zToReturn = 0;
			break;
		case Lane.Front:
			zToReturn = -3;
			break;
		default:
			break;
		}

		return zToReturn;
	}

	public Direction GetOppositeChangeLaneDirection()
	{
		Direction dirToReturn = Direction.Up;
		if(lane.laneChangeDirection == Direction.Up)
			dirToReturn = Direction.Down;
		return dirToReturn;

	}

	public void ArrivedAtNewLane()
	{
		fallingSpeedMultiplier = previousFallingSpeedMultiplier;

		lane.currentLane = lane.targetLane;

		lane.changeLaneRequested = false;

		if(lane.changeLaneOnceAgain)
		{
			lane.forceLaneChange = true;
			lane.changeLaneOnceAgain = false;

			//lane.laneChangeDirection = Direction.None;
		}
		else
		{
			lane.targetLane = Lane.None;
			lane.laneChangeDirection = Direction.None;
		}
	}

	private float GetCurrentHeightJumpedRatio()
	{
		return (currentHeightJumped / movement.jumpHeight);
	}

	public bool JumpRequested()
	{
		bool jumpRequested = false;
		
		if(InputController.Instance.GetControl(InputController.Instance.controls.jump).IsPressed && jumpWasReleased)
		{
			jumpRequested = true;
			jumpWasReleased = false;
		}
		
		return jumpRequested;
	}
	
	public bool AimRequested()
	{
		bool aimRequested = false;
		
		if(leftTrigger > 0.0f)
		{
			aimRequested = true;
		}
		
		else if (InputController.Instance.activeDevice.Name == "Keyboard/Mouse")
		{
			if(InputController.Instance.GetControl(InputController.Instance.controls.aim).IsPressed)
			{
				aimRequested = true;
			}
		}
		
		return aimRequested;
	}

	public bool ShootRequested()
	{
		bool shootRequested = false;
		
		if(rightTrigger > 0.0f && shootWasReleased)
		{
			shootRequested = true;
			shootWasReleased = false;
		}
		
		else if (InputController.Instance.activeDevice.Name == "Keyboard/Mouse")
		{
			if(InputController.Instance.GetControl(InputController.Instance.controls.fire).IsPressed && shootWasReleased)
			{
				shootRequested = true;
				shootWasReleased = false;
			}
		}
		
		return shootRequested;
	}
	
	public bool CanMove()
	{
		return canMove;
	}
	
	public bool CanJump()
	{
		if(!IsGrounded())
			canJump = false;
		return canJump;
	}

	public bool CanAim()
	{
		if(lane.changeLaneRequested)
			canAim = false;
		else
			canAim = true;
		return canAim;
	}

	public bool CanShoot()
	{
		return canShoot;
	}

	public bool InColorZone()
	{
		return inColorZone;
	}

	public void CanShoot(bool state)
	{
		canShoot = state;
	}

	public void CanAim(bool state)
	{
		canAim = state;
	}
	
	public void CanMove(bool state)
	{
		canMove = state;
	}
	
	public void CanJump(bool state)
	{
		canJump = state;
	}

	public void InColorZone(bool state)
	{
		inColorZone = state;
	}

	public void SetCannonColor()
	{
		/*
		cannonColor = choice;

		switch(cannonColor)
		{
			case ColorController.ColorChoice.None:
			playerMesh.renderer.material.color = Color.white;
			break;

			case ColorController.ColorChoice.Left:
			playerMesh.renderer.material.color = Color.red;
			break;

			case ColorController.ColorChoice.Right:
			playerMesh.renderer.material.color = Color.green;
			break;
		}
		*/

		if(canShoot)
			playerMesh.renderer.material.color = Color.yellow;
		
		else
			playerMesh.renderer.material.color = Color.white;
	}

	public void ResetJumpValues()
	{
		jumpTime = 0;
		currentHeightJumped = 0;
		timeInAir = 0;
		fallingSpeedMultiplier = 0;
		risingSpeedMultiplier = 0;
	}
	
	public void ResetMovementValues()
	{
		moveTime = 0;
		horizontalSpeedMultiplier = 0;
		
	}
	
	public void MoveCharacter()
	{

		characterController.Move(movementVector);
	}
	
	private void LateUpdate()
	{
		//MoveCharacter();
		UpdateLateVariables();
	}
	
	private void UpdateLateVariables()
	{
		previousGroundedState = currentlyGrounded;
		previousFramePosition = transform.position;
		previousFrameLeftStick = leftStick;
	}
	
	public void StickToGround()
	{
		stickToTheGround = false;
				
		/*if(Physics.Raycast(ray, out hit, 0.5f, defaultLayer))
		{
			stickToTheGround = true;
			IsGrounded(true);
			transform.position = new Vector3(transform.position.x, hit.point.y + (characterController.height * 0.52f),transform.position.z);
		}
		else
		{
			stickToTheGround = false;
		}*/

		Ray ray = new Ray(transform.position - new Vector3(characterController.radius, characterController.height *0.45f,0), Vector3.down);
		RaycastHit hit;
		
		if(Physics.Raycast(ray, out hit, 0.4f, defaultLayer))
		{
			stickToTheGround = true;
			IsGrounded(true);
			transform.position = new Vector3(transform.position.x, hit.point.y + (characterController.height * 0.55f), transform.position.z);
		}
		else
		{
			ray = new Ray(transform.position - new Vector3(-characterController.radius, characterController.height *0.45f,0), Vector3.down);
			
			if(Physics.Raycast(ray, out hit, 0.4f, defaultLayer))
			{
				stickToTheGround = true;
				IsGrounded(true);
				transform.position = new Vector3(transform.position.x, hit.point.y + (characterController.height * 0.55f), transform.position.z);
			}
		}
		
	}
	
	public bool IsGrounded()
	{
		bool isGrounded = false;
		
		Ray ray = new Ray(transform.position - new Vector3(characterController.radius, characterController.height *0.4f,0), Vector3.down);
		RaycastHit hit;
		
		if(Physics.Raycast(ray, out hit, 0.35f, defaultLayer))
		{
			isGrounded = true;
		}

		else
		{
			ray = new Ray(transform.position - new Vector3(-characterController.radius, characterController.height *0.4f,0), Vector3.down);
			
			if(Physics.Raycast(ray, out hit, 0.35f, defaultLayer))
			{
				isGrounded = true;
			}
		}

		currentlyGrounded = isGrounded;

		if(currentlyGrounded)
		{
			AuthorizeManualLaneChange(true);
			AuthorizeAutomaticLaneChange(true);
		}

		return currentlyGrounded;
	}
	
	public void IsGrounded(bool newState)
	{
		currentlyGrounded = newState;
	}
	
	public bool IsFalling()
	{
		_falling = !IsGrounded();
		return _falling;
	}

	public bool IsJumping()
	{
		bool _jumping = false;
		if(stateMachine.currentState == jumping)
			_jumping = true;
		return _jumping;
	}
	
	public void UpdateMovement()
	{
		
		if(leftStick.x > 0)
		{
			if(leftStickPositiveDuration > 0.1f)
			{
				MovementTowardsRight();
			}
		}
		else if(leftStick.x < 0)
		{
			if(leftStickNegativeDuration > 0.1f)
			{
				MovementTowardsLeft();
			}
		}
		else if(leftStickZeroDuration > 0.1f)
		{
			ResetMovementValues();
		}
	}

	public float GetLeftStickPositiveDuration()
	{
		return leftStickPositiveDuration;
	}

	public float GetLeftStickNegativeDuration()
	{
		return leftStickNegativeDuration;
	}
	
	private void MovementTowardsRight()
	{
		Vector3 targetHorizontalMovementVector = GetHorizontalMovementValue();
		
		movementVector += new Vector3(targetHorizontalMovementVector.x, targetHorizontalMovementVector.y, targetHorizontalMovementVector.z);
		SetHorizontalDirection(Direction.Right);
		moveTime += GameController.DeltaTime();
	}
	
	private void MovementTowardsLeft()
	{
		Vector3 targetHorizontalMovementVector = GetHorizontalMovementValue();
		
		movementVector += new Vector3(-(Mathf.Abs(targetHorizontalMovementVector.x)), targetHorizontalMovementVector.y, targetHorizontalMovementVector.z);
		SetHorizontalDirection(Direction.Left);
		moveTime += GameController.DeltaTime();
	}
	
	public void SetHorizontalDirection(Direction newDirection)
	{
		if(horizontalDirection != newDirection)
		{
			if((newDirection == Direction.Left || newDirection == Direction.Right))
			{
				ResetMovementValues();
			}
			
			previousHorizontalDirection = horizontalDirection;
			horizontalDirection = newDirection;
		}
	}
	
	public Direction GetHorizontalDirection()
	{
		return horizontalDirection;
	}
	
	public void UpdateJumpMovement()
	{
		movementVector += GetVerticalJumpValue();
		currentHeightJumped += movementVector.y;
		jumpTime += GameController.DeltaTime();
	}
	
	
	public void UpdateGravity()
	{
		movementVector += GetGravityValue();
	}
	
	public void ApplyAirControlFactor()
	{
		movementVector = new Vector3 (
			movementVector.x * movement.airControlFactor,
			movementVector.y,
			movementVector.z * movement.airControlFactor);

	}
	
	public void ApplyMovementDeceleration()
	{
		
		float targetXMovementValue = movementVector.x;
		float lerpedXMovementValue = Mathf.Lerp(characterController.velocity.x * GameController.DeltaTime(), targetXMovementValue, currentHorizontalMovementFactor);
		
		if(Mathf.Abs(lerpedXMovementValue) > 0.00005f)
		{
			movementVector = 
				new Vector3(lerpedXMovementValue, movementVector.y, movementVector.z);
		}
		
	}
	
	private void ResetDecelerationValues()
	{
		idleTime = 0;
	}
	
	public Vector3 GetHorizontalMovementValue()
	{
		float horizontalMovementValue = 0;
		
		horizontalMovementValue = InputController.Instance.LeftStick().x;
		horizontalMovementValue *= movement.horizontalSpeed;
		
		horizontalMovementValue *= horizontalSpeedMultiplier;
		horizontalMovementValue *= GameController.DeltaTime();
		
		return new Vector3(horizontalMovementValue, 0, 0);
	}
	
	public Vector3 GetVerticalJumpValue()
	{
		float verticalJumpMovementValue = 1;
		
		verticalJumpMovementValue *= movement.jumpRiseSpeed;
		verticalJumpMovementValue *= risingSpeedMultiplier;
		verticalJumpMovementValue *= GameController.DeltaTime();

		if(characterController.collisionFlags == CollisionFlags.CollidedAbove)
		{
			verticalJumpMovementValue = 0;
			interruptJump = true;
		}
		
		return new Vector3(0, verticalJumpMovementValue, 0);
	}
	
	public Vector3 GetGravityValue()
	{
		if(applyGravity)
			return ((Physics.gravity * GameController.DeltaTime()) * movement.fallingSpeed) * fallingSpeedMultiplier;
		else
			return Vector3.zero;
	}
	
	public bool JumpHeightReached()
	{
		bool jumpHeightReached = false;
		
		if(currentHeightJumped >= movement.jumpHeight)
		{
			jumpHeightReached = true;
		}
		
		return jumpHeightReached;
	}
	
	public void SetPosition(Vector3 newPosition)
	{
		transform.position = newPosition;
	}
	
	public float GetTimeInAir()
	{
		return timeInAir;
	}

	public void Shoot()
	{
		/*
		if(cannonColor == ColorController.ColorChoice.None)
		{
			Debug.LogWarning("cannonColor is set to None !");
			return;
		}
		*/
		//else
		{
			Vector3 shootOffset = horizontalDirection == Direction.Left ? -transform.right : transform.right;
			GameObject shootInstance = Instantiate(Resources.Load("ColorShot"),
			                                       transform.position + shootOffset,
			                                       Quaternion.LookRotation(shootOffset)) as GameObject;
		}
	}
	
	[System.Serializable]
	public class MovementParameters
	{
		[Tooltip("Maximum horizontal speed while moving")]
		public float horizontalSpeed;
		[Tooltip("Acceleration from zero to Horizontal Speed")]
		public AnimationCurve horizontalAcceleration;

		public float horizontalGroundStopSpeed;
		public float horizontalAirStopSpeed;
		
		[Tooltip("Maximum falling speed\nCan be considered as 'gravity speed'")]
		public float fallingSpeed;
		[Tooltip("Acceleration from zero to Falling Speed over time:\n  - X: Time elapsed since falling\n  - Y: Speed coefficient between 0 and 1")]
		public AnimationCurve fallingAcceleration;
		
		[Tooltip("Height of a jump - 0 means no jump")]
		public float jumpHeight;
		[Tooltip("Maximum vertical jump rise speed")]
		public float jumpRiseSpeed;
		[Tooltip("Acceleration for the jump rising, depending on the current height jumped:\n  - X: Current height jumped\n  - Y: Speed coefficient between 0 and 1")]
		public AnimationCurve riseAcceleration;
		[Tooltip("Determines how much the player controls his horizontal movement while he's in the air:\n0 means no control at all.\n1 means total control as if he were on the ground.")]
		public float airControlFactor;
	}
	
	[System.Serializable]
	public class AccelerationVector2
	{
		public AnimationCurve start;
		public AnimationCurve stop;
	}
	
	[System.Serializable]
	public class SpawnParameters
	{
		[Tooltip("Spawn position for the character.\nUsed only if the character (PlayerController GameObject) is not present at start of the scene.\nOtherwise, the present PlayerController will be used.")]
		public Vector3 position;
		
	}

	[System.Serializable]
	public class LaneParameters
	{
		public enum BlockArea
		{
			None,
			Top,
			Bottom,
			Left,
			Right,
			Both,
			All
		}
		[HideInInspector]
		public Lane currentLane = Lane.Center;
		public float zOffsetValue = 5;
		public float zSpeed = 2;
		[HideInInspector]
		public bool automaticLaneChangeAllowed = true;
		[HideInInspector]
		public bool manualLaneChangeAllowed = true;
		[HideInInspector]
		public Direction automaticChangeLaneDirection = Direction.None;
		[HideInInspector]
		public PlayerController.Direction laneChangeDirection = PlayerController.Direction.None;
		[HideInInspector]
		public PlayerController.Lane targetLane = PlayerController.Lane.None;
		[HideInInspector]
		public bool changeLaneRequested = false;
		[HideInInspector]
		public float zAtSwitchStart = 0f;
		[HideInInspector]
		public float targetZ = 0f;
		[HideInInspector]
		public bool changeLaneOnceAgain = false;
		[HideInInspector]
		public bool forceLaneChange = false;
		[HideInInspector]
		public PlayerController.Direction forceLaneChangeDirection = PlayerController.Direction.None;

		public BlockArea blockedAreaHead;
		public BlockArea blockedAreaFeet;
	}

	[System.Serializable]
	public class ShotParameters
	{
		public float travelSpeed;
		public float range;
		public float travelDistance;
	}

	public enum Lane
	{
		None,
		Front,
		Center,
		Back
	}

	
	public enum Direction
	{
		None,
		Any,
		Left,
		Right,
		Up,
		Down
	}
}

public class IdleState : State
{
	public override void EnterState(GameObject go)
	{
		PlayerController.Instance.CanJump(true);
		CameraController.Instance.CanLook(true);
	}
	
	public override void UpdateState(GameObject go)
	{
		PlayerController.Instance.StickToGround();
		PlayerController.Instance.UpdateMovement();
		PlayerController.Instance.UpdateLaneChangeMovement();
		PlayerController.Instance.UpdateGravity();
		PlayerController.Instance.ApplyMovementDeceleration();
		PlayerController.Instance.MoveCharacter();
		
		
	}
	
	public override void LateUpdateState(GameObject go)
	{
		// Idle to move state handling
		if(PlayerController.Instance.MovementRequested() && PlayerController.Instance.CanMove())
			PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.moving);
		
		// Idle to jump state handling
		if(PlayerController.Instance.JumpRequested() && PlayerController.Instance.CanJump())
			PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.jumping);
		
		//Idle to aim state handling
		//if(PlayerController.Instance.AimRequested() && PlayerController.Instance.CanAim())
			//PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.aiming);

		if(PlayerController.Instance.ShootRequested() && PlayerController.Instance.CanShoot() && PlayerController.Instance.InColorZone())
			PlayerController.Instance.Shoot();


	}
	
	public override void ExitState(GameObject go)
	{
		
	}
}


public class MoveState : State
{
	public override void EnterState(GameObject go)
	{
		PlayerController.Instance.CanJump(true);
		CameraController.Instance.CanLook(true);
	}
	
	public override void UpdateState(GameObject go)
	{
		PlayerController.Instance.StickToGround();
		PlayerController.Instance.UpdateMovement();
		PlayerController.Instance.UpdateLaneChangeMovement();
		PlayerController.Instance.UpdateGravity();
		PlayerController.Instance.ApplyMovementDeceleration();
		PlayerController.Instance.MoveCharacter();
	}
	
	public override void LateUpdateState(GameObject go)
	{
		// Move to idle state handling
		if(!PlayerController.Instance.MovementRequested() || !PlayerController.Instance.CanMove())
		{
			PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.idle);
		}
		
		// Move to jump state handling
		if(PlayerController.Instance.JumpRequested() && PlayerController.Instance.CanJump())
		{
			PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.jumping);
		}
		
		//Idle to aim state handling
		//if(PlayerController.Instance.AimRequested())
			//PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.aiming);
		
		// Move to fall state handling
		if(PlayerController.Instance.IsFalling())
		{
			PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.falling);
		}

		if(PlayerController.Instance.ShootRequested() && PlayerController.Instance.CanShoot() && PlayerController.Instance.InColorZone())
			PlayerController.Instance.Shoot();
	}
	
	public override void ExitState(GameObject go)
	{
		
	}
}

public class JumpState : State
{
	public override void EnterState(GameObject go)
	{
		PlayerController.Instance.interruptJump = false;
		PlayerController.Instance.ResetJumpValues();
		
		// If player jumps while Idle, no jump direciton must be set
		PlayerController.Direction newJumpDir = PlayerController.Instance.horizontalDirection;
		if(PlayerController.Instance.stateMachine.previousState == PlayerController.Instance.idle)
			newJumpDir = PlayerController.Direction.None;
		
		PlayerController.Instance.SetJumpDirection(newJumpDir);
		PlayerController.Instance.CanJump(false);
	}
	
	public override void UpdateState(GameObject go)
	{
		PlayerController.Instance.UpdateJumpMovement();
		PlayerController.Instance.UpdateMovement();

		PlayerController.Instance.AutomaticLaneChangeCheck();
		PlayerController.Instance.UpdateLaneChangeMovement();
		
		PlayerController.Instance.ApplyAirControlFactor();
		PlayerController.Instance.ApplyMovementDeceleration();
		PlayerController.Instance.MoveCharacter();
	}
	
	public override void LateUpdateState(GameObject go)
	{
		if(PlayerController.Instance.JumpHeightReached() || PlayerController.Instance.interruptJump)
			PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.falling);

		if(PlayerController.Instance.ShootRequested() && PlayerController.Instance.CanShoot() && PlayerController.Instance.InColorZone())
			PlayerController.Instance.Shoot();
	}
	
	public override void ExitState(GameObject go)
	{
		PlayerController.Instance.ResetJumpValues();
	}
}

public class FallState : State
{
	public override void EnterState(GameObject go)
	{
		PlayerController.Instance.CanJump(false);
	}
	
	public override void UpdateState(GameObject go)
	{
		PlayerController.Instance.StickToGround();
		PlayerController.Instance.UpdateMovement();
		
		PlayerController.Instance.UpdateLaneChangeMovement();
		
		PlayerController.Instance.ApplyAirControlFactor();
		PlayerController.Instance.UpdateGravity();
		PlayerController.Instance.ApplyMovementDeceleration();
		PlayerController.Instance.MoveCharacter();
	}
	
	public override void LateUpdateState(GameObject go)
	{
		if(PlayerController.Instance.IsGrounded())
		{
			PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.moving);
		}

		/*if(PlayerController.Instance.ShootRequested() && PlayerController.Instance.CanShoot() && PlayerController.Instance.InColorZone())
			PlayerController.Instance.Shoot();*/
	}
	
	public override void ExitState(GameObject go)
	{
		
	}
}
/*
public class AimState : State
{
	public override void EnterState(GameObject go)
	{
		PlayerController.Instance.CanJump(false);
		PlayerController.Instance.CanMove(false);
		CameraController.Instance.CanLook(false);
		CursorController.Instance.Show();
	}
	
	public override void UpdateState(GameObject go)
	{
		PlayerController.Instance.UpdateGravity();
		
		if(!PlayerController.Instance.AimRequested())
		{
			PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.idle);
		}
		
		if(PlayerController.Instance.ShootRequested())
		{
			
		}
		
		PlayerController.Instance.MoveCharacter();
	}
	
	public override void LateUpdateState(GameObject go)
	{
		
	}
	
	public override void ExitState(GameObject go)
	{
		CursorController.Instance.Hide();
	}
}
*/

