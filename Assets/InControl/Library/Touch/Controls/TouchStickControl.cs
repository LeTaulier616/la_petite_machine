using System.IO;
using UnityEngine;


namespace InControl
{
	public class TouchStickControl : TouchControl
	{
		[Header("Dimensions")]

		public TouchControlAnchor
			anchor = TouchControlAnchor.BottomLeft;
		public Vector2 offset = new Vector2( 0.2f, 0.2f );
		public Rect activeArea = new Rect( 0.0f, 0.0f, 0.5f, 1.0f );

		[Range(0,1)] public float ringSize = 0.25f;
		[Range(0,1)] public float knobSize = 0.125f;
		[Range(0,1)] public float knobRange = 0.2f;

		[Header("Options")]

		public AnalogTarget target = AnalogTarget.LeftStick;

		[Range(0,1)] public float lowerDeadZone = 0.1f;
		[Range(0,1)] public float upperDeadZone = 0.9f;

		public AnimationCurve inputCurve = AnimationCurve.Linear( 0.0f, 0.0f, 1.0f, 1.0f );

		public bool allowDragging = false;
		public bool snapToInitialTouch = true;
		public bool resetWhenDone = true;

		[Header("Sprites")]

		public Sprite ringSprite;
		public Sprite knobSprite;

		[Header("Colors")]

		public Color ringActiveColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		public Color ringInactiveColor = new Color( 1.0f, 1.0f, 1.0f, 0.2f );

		public Color knobActiveColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		public Color knobInactiveColor = new Color( 1.0f, 1.0f, 1.0f, 0.2f );

		GameObject ringGameObject;
		GameObject knobGameObject;
		SpriteRenderer knobRenderer;
		SpriteRenderer ringRenderer;
		Color ringColor;
		Color knobColor;

		Vector3 resetPosition;
		Vector3 beganPosition;
		Vector3 movedPosition;
		float resetDistance;

		Rect worldActiveArea;
		float worldRingSize;
		float worldKnobSize;
		float worldKnobRange;

		Vector3 value;

		Touch currentTouch;


		public override void CreateControl()
		{
			CreateRing();
			CreateKnob();
			ConfigureControl();
		}


		public override void DestroyControl()
		{
			Destroy( ringGameObject );
			Destroy( knobGameObject );
		}


		public override void ConfigureControl()
		{
			var worldOffset = (Vector3) offset * TouchManager.UnitToWorld;
			resetPosition = TouchManager.ViewToWorldPoint( TouchUtility.AnchorToViewPoint( anchor ) ) + worldOffset;
			transform.position = resetPosition;

			ScaleSprite( ringGameObject, ringRenderer, ringSize );
			ScaleSprite( knobGameObject, knobRenderer, knobSize );

			worldActiveArea = TouchManager.ViewToWorldRect( activeArea );
			worldRingSize = ringSize * TouchManager.HalfUnitToWorld;
			worldKnobSize = knobSize * TouchManager.HalfUnitToWorld;
			worldKnobRange = knobRange * TouchManager.HalfUnitToWorld;
		}


		void CreateRing()
		{
			if (ringSprite == null)
			{
				throw new MissingReferenceException( "Ring sprite is not set." );
			}

			ringGameObject = CreateSpriteGameObject( "Ring" );
			ringRenderer = CreateSpriteRenderer( ringGameObject, ringSprite, 1000 );

			RingColor = ringInactiveColor;
		}


		void CreateKnob()
		{
			if (knobSprite == null)
			{
				throw new MissingReferenceException( "Knob sprite is not set." );
			}

			knobGameObject = CreateSpriteGameObject( "Knob" );
			knobRenderer = CreateSpriteRenderer( knobGameObject, knobSprite, 1001 );

			KnobColor = knobInactiveColor;
		}


		public override void DrawGizmos()
		{
			Utility.DrawRectGizmo( worldActiveArea, Color.green );
			Utility.DrawCircleGizmo( RingPosition, worldRingSize, Color.yellow );
			Utility.DrawCircleGizmo( KnobPosition, worldKnobSize, Color.yellow );
			Utility.DrawCircleGizmo( RingPosition, worldKnobRange, Color.red );
		}


		void Update()
		{
			float resetSpeed = 10.0f * Time.deltaTime;

			if (IsActive)
			{
				if (ringColor != ringActiveColor)
				{
					RingColor = Utility.MoveColorTowards( ringColor, ringActiveColor, resetSpeed );
				}

				if (knobColor != knobActiveColor)
				{
					KnobColor = Utility.MoveColorTowards( knobColor, knobActiveColor, resetSpeed );
				}
			}
			else
			{
				if (ringColor != ringInactiveColor)
				{
					RingColor = Utility.MoveColorTowards( ringColor, ringInactiveColor, resetSpeed );
				}

				if (knobColor != knobInactiveColor)
				{
					KnobColor = Utility.MoveColorTowards( knobColor, knobInactiveColor, resetSpeed );
				}

				if (resetWhenDone && KnobPosition != resetPosition)
				{
					var delta = KnobPosition - RingPosition;
					RingPosition = Vector3.MoveTowards( RingPosition, resetPosition, resetDistance * resetSpeed );
					KnobPosition = RingPosition + delta;
				}

				if (KnobPosition != RingPosition)
				{
					KnobPosition = Vector3.MoveTowards( KnobPosition, RingPosition, worldRingSize * resetSpeed );
				}
			}
		}


		public override void SubmitControlState( ulong updateTick )
		{
			SubmitAnalogValue( target, value, lowerDeadZone, upperDeadZone, updateTick );
		}


		public override void TouchBegan( Touch touch )
		{
			if (IsActive)
			{
				return;
			}

			beganPosition = TouchManager.ScreenToWorldPoint( touch.position );

			var insideActiveArea = worldActiveArea.Contains( beganPosition );
			var insideControl = (RingPosition - beganPosition).magnitude <= worldRingSize;

			if (snapToInitialTouch && (insideActiveArea || insideControl))
			{
				RingPosition = beganPosition;
				KnobPosition = beganPosition;
				currentTouch = touch;
			}
			else
			if (insideControl)
			{
				KnobPosition = beganPosition;
				beganPosition = RingPosition;
				currentTouch = touch;
			}

			if (IsActive)
			{
				TouchMoved( touch );
			}
		}


		public override void TouchMoved( Touch touch )
		{
			if (currentTouch != touch)
			{
				return;
			}

			movedPosition = TouchManager.ScreenToWorldPoint( touch.position );

			var vector = movedPosition - beganPosition;
			var normal = vector.normalized;
			var length = vector.magnitude;

			if (allowDragging)
			{
				var excess = length - worldKnobRange;
				if (excess < 0.0f)
				{
					excess = 0.0f;
				}
				beganPosition = beganPosition + (excess * normal);
				RingPosition = beganPosition;
			}

			movedPosition = beganPosition + (Mathf.Clamp( length, 0.0f, worldKnobRange ) * normal);

			value = (movedPosition - beganPosition) / worldKnobRange;
			value.x = inputCurve.Evaluate( Mathf.Abs( value.x ) ) * Mathf.Sign( value.x );
			value.y = inputCurve.Evaluate( Mathf.Abs( value.y ) ) * Mathf.Sign( value.y );

			KnobPosition = movedPosition;
		}


		public override void TouchEnded( Touch touch )
		{
			if (currentTouch != touch)
			{
				return;
			}

			value = Vector3.zero;
			resetDistance = (resetPosition - RingPosition).magnitude;
			currentTouch = null;
		}


		public bool IsActive
		{
			get
			{
				return currentTouch != null;
			}
		}


		public bool IsNotActive
		{
			get
			{
				return currentTouch == null;
			}
		}


		public Color RingColor
		{
			get
			{
				return ringColor;
			}

			set
			{
				ringColor = value;
				if (ringRenderer != null)
				{
					ringRenderer.sharedMaterial.color = value;
				}
			}
		}


		public Color KnobColor
		{
			get
			{
				return knobColor;
			}

			set
			{
				knobColor = value;
				if (knobRenderer != null)
				{
					knobRenderer.sharedMaterial.color = value;
				}
			}
		}


		public Vector3 RingPosition
		{
			get
			{
				return ringGameObject == null ? transform.position : ringGameObject.transform.position;
			}
			set
			{
				if (ringGameObject != null)
				{
					ringGameObject.transform.position = value;
				}
			}
		}


		public Vector3 KnobPosition
		{
			get
			{
				return knobGameObject == null ? transform.position : knobGameObject.transform.position;
			}
			set
			{
				if (knobGameObject != null)
				{
					knobGameObject.transform.position = value;
				}
			}
		}
	}
}

