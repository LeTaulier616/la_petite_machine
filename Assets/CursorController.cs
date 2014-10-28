using UnityEngine;
using System.Collections;

public class CursorController : Singleton<CursorController>
{
	public float cursorSpeed;
	public AnimationCurve accelerationCurve;
	public AnimationCurve decelerationCurve;
	
	private UICamera uiCamera;
	private UISprite reticle;
	private Vector2 reticleScreenCoordinates;
	
	private Vector2 rightStick;
	private Vector2 previousFrameRightStick;
	
	private Vector3 cursorMovementVector;
	
	private float moveTime;
	private float idleTime;
	
	// Use this for initialization
	void Start ()
	{
		uiCamera = transform.parent.GetComponent<UICamera>();
		reticle = gameObject.GetComponentInChildren<UISprite>();
		
		moveTime = 0.0f;
		idleTime = 0.0f;
		
		Hide();
	}
	
	// Update is called once per frame
	private void Update ()
	{		
		if(reticle.enabled)
		{
			UpdateInput();
			
			UpdateCursorMovement();
			MoveCursor();
			
			reticleScreenCoordinates = getReticleScreenCoordinates();
		}
	}
	
	private void LateUpdate()
	{
		previousFrameRightStick = rightStick;
	}
	
	public void Show()
	{
		reticle.transform.localScale = Vector3.one * 1.5f;
		reticle.alpha = 0.0f;
		
		UITweener scaleTween = TweenScale.Begin(reticle.gameObject, 0.2f, Vector3.one);
		scaleTween.method = UITweener.Method.EaseInOut;
		
		UITweener alphaTween = TweenAlpha.Begin(reticle.gameObject, 0.2f, 1.0f);
		alphaTween.method = UITweener.Method.EaseInOut;
		
		
		reticle.enabled = true;
	}
	
	public void Hide()
	{
		reticle.transform.localScale = Vector3.one;
		reticle.alpha = 1.0f;
		
		UITweener scaleTween = TweenScale.Begin(reticle.gameObject, 0.2f, Vector3.zero);
		scaleTween.method = UITweener.Method.EaseInOut;
		
		UITweener alphaTween = TweenAlpha.Begin(reticle.gameObject, 0.2f, 0.0f);
		alphaTween.method = UITweener.Method.EaseInOut;
		
		
		reticle.enabled = false;
	}
	
	public void ResetCursor()
	{
		transform.localPosition = Vector3.zero;
	}
	
	private Vector2 getReticleScreenCoordinates()
	{
		Vector2 vectorToReturn = Vector2.zero;
		
		vectorToReturn = uiCamera.camera.WorldToScreenPoint(reticle.transform.position);
		
		return vectorToReturn;
	}
	
	public Vector2 ReticleScreenCoordinates()
	{
		return reticleScreenCoordinates;
	}
	
	private void UpdateCursorMovement()
	{
		if(rightStick != Vector2.zero)
		{
			cursorMovementVector = rightStick * cursorSpeed * GameController.DeltaTime();
			
			cursorMovementVector *= EvaluateAccelerationCurve();
		}
	
		else
		{
		 	cursorMovementVector *= EvaluateDecelerationCurve();
		}
		
	}
	
	private void MoveCursor()
	{
		transform.position += cursorMovementVector;
	}
	
	private void UpdateInput()
	{		
		rightStick = InputController.Instance.RightStick();
		
		if(IsCursorMoving())
		{
			idleTime = 0.0f;
			moveTime += GameController.DeltaTime();
		}
		
		else
		{
			moveTime = 0.0f;
			idleTime += GameController.DeltaTime();
		}
	}
	
	private float EvaluateAccelerationCurve()
	{
		float valueToReturn;
		
		valueToReturn = accelerationCurve.Evaluate(moveTime);
		
		return valueToReturn;
	}
	
	private float EvaluateDecelerationCurve()
	{
		float valueToReturn;
		
		valueToReturn = decelerationCurve.Evaluate(idleTime);
		
		return valueToReturn;
	}
	
	private bool IsCursorMoving()
	{
		if(rightStick != Vector2.zero && previousFrameRightStick != Vector2.zero)
			return true;
			
		else if (rightStick == Vector2.zero && previousFrameRightStick != Vector2.zero)
			return false;
			
		else
			return false;
	}
}
