﻿using UnityEngine;
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
	
	private bool isMoving = false;
	
	[Tooltip("All spawn-related parameters are nested here.")]
	public SpawnParameters spawn;
		
	[Tooltip("All movement-related parameters are nested here.")]
	public MovementParameters movement;

	/*CHARLES*/
	[Tooltip("All Lane-Movement related parameters are nested here.")]
	public LaneParameters lane;
	private float leftStickYMovedDuration = 0f;
	private float leftStickMovedDuration = 0f;
	private Direction startedSwipeDirection = Direction.None;
	private Direction leftStickSwipeDirection = Direction.None;
	private bool leftStickHasBeenReleased = true;
	private bool leftStickYHasBeenReleased = true;
	/*CHARLES*/
	
	private bool jumpWasReleased = false;
	private bool _falling = false;
	
	private float timeInAir = 0;
	private float jumpTime = 0;
	private float currentHeightJumped = 0;
	private float moveTime = 0;
	
	private float leftStickPositiveDuration = 0;
	private float leftStickNegativeDuration = 0;
	private float leftStickZeroDuration = 0;
	
	private bool currentlyGrounded = false;
	private bool previousGroundedState = false;
	private float idleTime = 0;
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
	
	private StateMachine stateMachine;
	public IdleState idle;
	public MoveState moving;
	public JumpState jumping;
	public FallState falling;
	public AimState aiming; 
	
	private bool canMove;
	private bool canJump;
	
	private Direction horizontalDirection;
	private Direction previousHorizontalDirection;
	private Direction jumpDirection;
	
	private Vector2 previousFrameLeftStick;
	private Vector2 leftStickDelta;

	private float timeMovingLeft = 0;
	private float timeMovingRight = 0;
	private bool interruptJump = false;


	private void Start()
	{
		characterController = GetComponent<CharacterController>();
		
		stateMachine = NGUITools.AddMissingComponent<StateMachine>(gameObject);
		
		idle = new IdleState();
		moving = new MoveState();
		jumping = new JumpState();
		falling = new FallState();
		aiming = new AimState(); 
				
		stateMachine.SetState(idle);

		CanMove(true);
		CanJump(true);
		CameraController.Instance.CanLook(true);
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
			if(PlayerController.Instance.AimRequested())
				PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.aiming);
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
			/*CHARLES START*/
			PlayerController.Instance.UpdateLaneChangeMovement();
			/*CHARLES END*/
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
			if(PlayerController.Instance.AimRequested())
				PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.aiming);
				
			// Move to fall state handling
			if(PlayerController.Instance.IsFalling())
			{
				PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.falling);
			}
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
				newJumpDir = Direction.None;
			
			PlayerController.Instance.SetJumpDirection(newJumpDir);
			PlayerController.Instance.CanJump(false);
		}
		
		public override void UpdateState(GameObject go)
		{
			PlayerController.Instance.UpdateJumpMovement();
			PlayerController.Instance.UpdateMovement();

			/*CHARLES START*/
			PlayerController.Instance.UpdateLaneChangeMovement();
			/*CHARLES END*/

			PlayerController.Instance.ApplyAirControlFactor();
			PlayerController.Instance.ApplyMovementDeceleration();
			PlayerController.Instance.MoveCharacter();
		}
		
		public override void LateUpdateState(GameObject go)
		{
			if(PlayerController.Instance.JumpHeightReached() || PlayerController.Instance.interruptJump)
				PlayerController.Instance.stateMachine.SetState(PlayerController.Instance.falling);
			
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

			/*CHARLES START*/
			PlayerController.Instance.UpdateLaneChangeMovement();
			/*CHARLES END*/

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
		}
		
		public override void ExitState(GameObject go)
		{
			
		}
	}
	
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
	
	private void Update()
	{
		UpdateEarlyVariables();
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

		/*CHARLES*/
		CheckChangeLane();
		/*CHARLES*/
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

	/*CHARLES START*/
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
			if((leftStickSwipeDirection == Direction.Up || leftStickSwipeDirection == Direction.Down) || (lane.forceLaneChange && lane.forceLaneChangeDirection != Direction.None))
			{
				newTargetLane = GetLane((lane.forceLaneChange ? lane.forceLaneChangeDirection : leftStickSwipeDirection));


				if(newTargetLane != lane.currentLane && newTargetLane != Lane.None)
				{
					lane.changeLaneRequested = true;
					lane.laneChangeDirection = (lane.forceLaneChange ? lane.forceLaneChangeDirection : leftStickSwipeDirection);

					lane.targetLane = newTargetLane;
					lane.zAtSwitchStart = transform.position.z;

					//Direction directionToCheck = lane.laneChangeDirection;


					lane.targetZ = transform.position.z + (lane.laneChangeDirection == Direction.Up ? lane.zOffsetValue : -lane.zOffsetValue);
				}

				lane.forceLaneChange = false;

				leftStickSwipeDirection = Direction.None;
				startedSwipeDirection = Direction.None;
			}
		}
		else
		{
			if(leftStickSwipeDirection != Direction.None)
			{
				lane.changeLaneOnceAgain = true;
				lane.forceLaneChangeDirection = leftStickSwipeDirection;
			}
		}
		

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

	/*CHARLES END*/

	public void UpdateLaneChangeMovement()
	{
		if(lane.changeLaneRequested)
		{
			float zSpeed = lane.zSpeed;

			if(Vector3.Distance(transform.position, new Vector3(transform.position.x, transform.position.y, lane.targetZ)) > 0.1f)
			{
				zSpeed *= ((lane.laneChangeDirection == Direction.Up ? 1 : -1) * GameController.DeltaTime());
				movementVector += new Vector3(0,0, zSpeed);
			}
			else
			{
				/*if(lane.targetLane != lane.currentLane)
				{*/
					ArrivedAtNewLane();
				//}
			}
		}
	}

	public void ArrivedAtNewLane()
	{
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
		
		if(rightTrigger > 0.0f)
		{
			shootRequested = true;
		}
		
		else if (InputController.Instance.activeDevice.Name == "Keyboard/Mouse")
		{
			if(InputController.Instance.GetControl(InputController.Instance.controls.fire).IsPressed)
			{
				shootRequested = true;
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
	
	public void CanMove(bool state)
	{
		canMove = state;
	}
	
	public void CanJump(bool state)
	{
		canJump = state;
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
	
	private void MoveCharacter()
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
		
		Ray ray = new Ray(transform.position - new Vector3(0, characterController.height *0.48f,0), Vector3.down);
		RaycastHit hit;
		
		
		if(Physics.Raycast(ray, out hit, 0.3f, defaultLayer))
		{
			stickToTheGround = true;
			IsGrounded(true);
			transform.position = new Vector3(transform.position.x, hit.point.y + (characterController.height *0.5f) + 0.1f,transform.position.z);
		}
		else
		{
			stickToTheGround = false;
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
		return ((Physics.gravity * GameController.DeltaTime()) * movement.fallingSpeed) * fallingSpeedMultiplier;
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

	/*CHARLES*/
	[System.Serializable]
	public class LaneParameters
	{
		[HideInInspector]
		public Lane currentLane = Lane.Center;
		public float zOffsetValue = 5;
		public float zSpeed = 2;
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
	}

	public enum Lane
	{
		None,
		Front,
		Center,
		Back
	}

	/*CHARLES*/
	
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
