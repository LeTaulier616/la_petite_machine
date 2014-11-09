using UnityEngine;
using System.Collections;

public class CursorController : Singleton<CursorController>
{
	public float cursorSpeed;
	public AnimationCurve accelerationCurve;
	public AnimationCurve decelerationCurve;
	
	public float fadeInSpeed;
	public float fadeInSize;
	
	public float fadeOutSpeed;
	
	private UICamera uiCamera;
	private UISprite reticle;
	private Vector2 reticleScreenCoordinates;
	
	private Vector2 rightStick;
	private Vector2 previousFrameRightStick;
	
	private Vector3 cursorMovementVector;
	
	private float moveTime;
	private float idleTime;

	private bool shown = true;
	
	// Use this for initialization
	void Start ()
	{
		uiCamera = transform.parent.GetComponent<UICamera>();
		reticle = gameObject.GetComponentInChildren<UISprite>();
		
		moveTime = 0.0f;
		idleTime = 0.0f;
		
		reticle.gameObject.SetActive(false);
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
		if(!shown)
		{
			shown = true;

			StopCoroutine("HideRoutine");
			StopCoroutine("ShowRoutine");

			StartCoroutine("ShowRoutine");
		}
	}
	
	public void Hide()
	{
		if(shown)
		{
			shown = false;

			StopCoroutine("HideRoutine");
			StopCoroutine("ShowRoutine");

			StartCoroutine("HideRoutine");
		}
	}

	private IEnumerator HideRoutine()
	{
		reticle.transform.localScale = Vector3.one;
		reticle.alpha = 1.0f;
		
		UITweener scaleTween = TweenScale.Begin(reticle.gameObject, fadeOutSpeed, Vector3.zero);
		scaleTween.method = UITweener.Method.EaseInOut;
		
		UITweener alphaTween = TweenAlpha.Begin(reticle.gameObject, fadeOutSpeed, 0.0f);
		alphaTween.method = UITweener.Method.EaseInOut;

		yield return new WaitForSeconds(fadeOutSpeed);
		
		reticle.gameObject.SetActive(false);
	}

	private IEnumerator ShowRoutine()
	{
		ResetCursor();
		reticle.gameObject.SetActive(true);
		
		reticle.transform.localScale = Vector3.one * fadeInSize;
		reticle.alpha = 0.0f;
		
		UITweener scaleTween = TweenScale.Begin(reticle.gameObject, fadeInSpeed, Vector3.one);
		scaleTween.method = UITweener.Method.EaseInOut;
		
		UITweener alphaTween = TweenAlpha.Begin(reticle.gameObject, fadeInSpeed, 1.0f);
		alphaTween.method = UITweener.Method.EaseInOut;		

		yield return null;
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
