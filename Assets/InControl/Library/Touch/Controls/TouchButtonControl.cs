using UnityEngine;


namespace InControl
{
	public class TouchButtonControl : TouchControl
	{
		[Header("Dimensions")]

		public TouchControlAnchor
			anchor = TouchControlAnchor.BottomRight;
		public Vector2 offset = new Vector2( -0.1f, 0.1f );

		[Range(0,1)] public float buttonSize = 0.125f;

		[Header("Options")]

		public ButtonTarget target = ButtonTarget.Action1;
		public bool allowSlideToggle = true;

		[Header("Sprites")]

		public Sprite buttonDownSprite;
		public Sprite buttonUpSprite;

		[Header("Colors")]

		public Color buttonDownColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		public Color buttonUpColor = new Color( 1.0f, 1.0f, 1.0f, 0.2f );

		GameObject buttonGameObject;
		SpriteRenderer buttonRenderer;
		Color buttonColor;
		float worldButtonSize;

		bool state;

		Touch currentTouch;


		public override void CreateControl()
		{
			CreateButton();
			ConfigureControl();
		}


		public override void DestroyControl()
		{
			Destroy( buttonGameObject );
		}


		public override void ConfigureControl()
		{
			var worldOffset = (Vector3) offset * TouchManager.UnitToWorld;
			transform.position = TouchManager.ViewToWorldPoint( TouchUtility.AnchorToViewPoint( anchor ) ) + worldOffset;

			ScaleSprite( buttonGameObject, buttonRenderer, buttonSize );

			worldButtonSize = buttonSize * TouchManager.HalfUnitToWorld;
		}


		void CreateButton()
		{
			if (buttonDownSprite == null)
			{
				throw new MissingReferenceException( "Button down sprite is not set." );
			}

			if (buttonUpSprite == null)
			{
				throw new MissingReferenceException( "Button up sprite is not set." );
			}

			buttonGameObject = CreateSpriteGameObject( "Button" );
			buttonRenderer = CreateSpriteRenderer( buttonGameObject, buttonUpSprite, 1000 );

			ButtonColor = buttonUpColor;
		}


		public override void DrawGizmos()
		{
			Utility.DrawCircleGizmo( ButtonPosition, worldButtonSize, Color.yellow );
		}


		void Update()
		{
			var maxDelta = 10.0f * Time.deltaTime;

			if (buttonRenderer == null)
			{
				return;
			}

			if (state)
			{
				if (buttonColor != buttonDownColor)
				{
					ButtonColor = Utility.MoveColorTowards( buttonColor, buttonDownColor, maxDelta );
				}

				if (buttonDownSprite != null)
				{
					buttonRenderer.sprite = buttonDownSprite;
				}
			}
			else
			{
				if (buttonColor != buttonUpColor)
				{
					ButtonColor = Utility.MoveColorTowards( buttonColor, buttonUpColor, maxDelta );
				}

				if (buttonUpSprite != null)
				{
					buttonRenderer.sprite = buttonUpSprite;
				}
			}
		}


		public override void SubmitControlState( ulong updateTick )
		{
			if (currentTouch == null && allowSlideToggle)
			{
				state = false;
				var touchCount = TouchManager.TouchCount;
				for (int i = 0; i < touchCount; i++)
				{
					state = state || Contains( TouchManager.ScreenToWorldPoint( TouchManager.GetTouch(i).position ) );
				}
			}

			SubmitButtonState( target, state, updateTick );
		}


		public override void TouchBegan( Touch touch )
		{
			if (currentTouch != null)
			{
				return;
			}

			if (Contains( TouchManager.ScreenToWorldPoint( touch.position ) ))
			{
				state = true;
				currentTouch = touch;
			}
		}


		public override void TouchMoved( Touch touch )
		{
		}


		public override void TouchEnded( Touch touch )
		{
			if (currentTouch != touch)
			{
				return;
			}

			currentTouch = null;
			state = false;
		}


		bool Contains( Vector3 worldPosition )
		{
			return (ButtonPosition - worldPosition).magnitude <= worldButtonSize;
		}


		public Color ButtonColor
		{
			get
			{
				return buttonColor;
			}

			set
			{
				buttonColor = value;
				if (buttonRenderer != null)
				{
					buttonRenderer.sharedMaterial.color = value;
				}
			}
		}


		public Vector3 ButtonPosition
		{
			get
			{
				return buttonGameObject == null ? transform.position : buttonGameObject.transform.position;
			}

			set
			{
				if (buttonGameObject != null)
				{
					buttonGameObject.transform.position = value;
				}
			}
		}
	}
}

