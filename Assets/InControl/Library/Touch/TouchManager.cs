using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;


namespace InControl
{
	[ExecuteInEditMode]
	public class TouchManager : MonoBehaviour
	{
		const int MaxTouches = 16;

		public enum GizmoShowOption
		{
			Never,
			WhenSelected,
			UnlessPlaying,
			Always
		}

		[Space(10)]

		public Camera
			touchCamera;
		public GizmoShowOption controlsShowGizmos = GizmoShowOption.Always;

		public static Camera Camera { get; private set; }
		public static InputDevice Device { get; private set; }

		public static Vector3 ViewSize { get; private set; }
		public static float UnitToWorld { get; private set; }
		public static float HalfUnitToWorld { get; private set; }

		public static GizmoShowOption ControlsShowGizmos { get; private set; }

		static List<TouchControl> touchControls = new List<TouchControl>();

		static Touch[] cachedTouches;
		static Touch mouseTouch;
		static List<Touch> activeTouches;
		static ReadOnlyCollection<Touch> readOnlyActiveTouches;

		static Vector2 lastMousePosition;
		static Vector2 knownScreenSize;


		void OnEnable()
		{
			ControlsShowGizmos = controlsShowGizmos;
			Camera = touchCamera;

			UpdateScreenSize();

			if (Application.isPlaying)
			{
				InputManager.OnSetup += Setup;
				InputManager.OnUpdate += UpdateTouches;
			}
		}


		void OnDisable()
		{
			Reset();
		}


		void Setup()
		{
			Input.simulateMouseWithTouches = false;

			CreateDevice();
			CreateTouches();

			var touchControlCount = touchControls.Count;
			for (int i = 0; i < touchControlCount; i++)
			{
				touchControls[i].CreateControl();
			}
		}


		void Reset()
		{
			Device = null;
			mouseTouch = null;
			cachedTouches = null;
			activeTouches = null;
			readOnlyActiveTouches = null;
			touchControls.Clear();
		}


		void Update()
		{
			ControlsShowGizmos = controlsShowGizmos;
			Camera = touchCamera;

			if (knownScreenSize != ScreenSize)
			{
				UpdateScreenSize();
			}
		}


		static void CreateDevice()
		{
			Device = new InputDevice( "TouchDevice" );

			Device.AddControl( InputControlType.LeftStickX, "LeftStickX" );
			Device.AddControl( InputControlType.LeftStickY, "LeftStickY" );
			Device.AddControl( InputControlType.RightStickX, "RightStickX" );
			Device.AddControl( InputControlType.RightStickY, "RightStickY" );
			Device.AddControl( InputControlType.LeftTrigger, "LeftTrigger" );
			Device.AddControl( InputControlType.RightTrigger, "RightTrigger" );
			Device.AddControl( InputControlType.DPadUp, "DPadUp" );
			Device.AddControl( InputControlType.DPadDown, "DPadDown" );
			Device.AddControl( InputControlType.DPadLeft, "DPadLeft" );
			Device.AddControl( InputControlType.DPadRight, "DPadRight" );
			Device.AddControl( InputControlType.Action1, "Action1" );
			Device.AddControl( InputControlType.Action2, "Action2" );
			Device.AddControl( InputControlType.Action3, "Action3" );
			Device.AddControl( InputControlType.Action4, "Action4" );
			Device.AddControl( InputControlType.LeftBumper, "LeftBumper" );
			Device.AddControl( InputControlType.RightBumper, "RightBumper" );
			Device.AddControl( InputControlType.Menu, "Menu" );

			InputManager.AttachDevice( Device );
		}


		static void UpdateScreenSize()
		{
			knownScreenSize = ScreenSize;
			ViewSize = TouchManager.ViewToWorldPoint( Vector2.one ) * 2.0f;
			UnitToWorld = Mathf.Min( ViewSize.x, ViewSize.y );
			HalfUnitToWorld = UnitToWorld * 0.5f;

			var touchControlCount = touchControls.Count;
			for (int i = 0; i < touchControlCount; i++)
			{
				touchControls[i].ConfigureControl();
			}
		}


		public static void AttachControl( TouchControl touchControl )
		{
			touchControls.Add( touchControl );
		}


		public static void DetachControl( TouchControl touchControl )
		{
			touchControls.Remove( touchControl );
		}


		static void SendTouchBegan( Touch touch )
		{
			var touchControlCount = touchControls.Count;
			for (int i = 0; i < touchControlCount; i++)
			{
				touchControls[i].TouchBegan( touch );
			}
		}


		static void SendTouchMoved( Touch touch )
		{
			var touchControlCount = touchControls.Count;
			for (int i = 0; i < touchControlCount; i++)
			{
				touchControls[i].TouchMoved( touch );
			}
		}


		static void SendTouchEnded( Touch touch )
		{
			var touchControlCount = touchControls.Count;
			for (int i = 0; i < touchControlCount; i++)
			{
				touchControls[i].TouchEnded( touch );
			}
		}


		static void CreateTouches()
		{
			cachedTouches = new Touch[MaxTouches];
			for (int i = 0; i < MaxTouches; i++)
			{
				cachedTouches[i] = new Touch( i );
			}
			mouseTouch = cachedTouches[MaxTouches - 1];
			activeTouches = new List<Touch>( MaxTouches );
			readOnlyActiveTouches = new ReadOnlyCollection<Touch>( activeTouches );
		}


		static void UpdateTouches( ulong updateTick, float deltaTime )
		{
			activeTouches.Clear();

			if (mouseTouch.SetWithMouseData( updateTick, deltaTime ))
			{
				activeTouches.Add( mouseTouch );
			}

			for (int i = 0; i < Input.touchCount; i++)
			{
				var unityTouch = Input.GetTouch( i );
				var cacheTouch = cachedTouches[unityTouch.fingerId];
				cacheTouch.SetWithTouchData( unityTouch, updateTick, deltaTime );
				activeTouches.Add( cacheTouch );
			}

			// Find any touches that Unity may have "forgotten" to end properly.
			for (int i = 0; i < MaxTouches; i++)
			{
				var touch = cachedTouches[i];
				if (touch.phase != TouchPhase.Ended && touch.updateTick != updateTick)
				{
					touch.phase = TouchPhase.Ended;
					activeTouches.Add( touch );
				}
			}

			InvokeTouchEvents();

			var touchControlCount = touchControls.Count;
			for (int i = 0; i < touchControlCount; i++)
			{
				touchControls[i].SubmitControlState( updateTick );
			}
		}


		static void InvokeTouchEvents()
		{
			var touchCount = activeTouches.Count;
			for (int i = 0; i < touchCount; i++)
			{
				var touch = activeTouches[i];
				switch (touch.phase)
				{
				case TouchPhase.Began:
					SendTouchBegan( touch );
					break;

				case TouchPhase.Moved:
					SendTouchMoved( touch );
					break;

				case TouchPhase.Ended:
					SendTouchEnded( touch );
					break;

				case TouchPhase.Canceled:
					SendTouchEnded( touch );
					break;
				}
			}
		}


		public static ReadOnlyCollection<Touch> Touches
		{
			get 
			{ 
				return readOnlyActiveTouches; 
			}
		}


		public static int TouchCount
		{
			get 
			{ 
				return activeTouches.Count; 
			}
		}


		public static Touch GetTouch( int touchIndex )
		{
			return activeTouches[touchIndex];
		}


		public static Touch GetTouchByFingerId( int fingerId )
		{
			return cachedTouches[fingerId];
		}


		static Vector2 ScreenSize
		{
			get 
			{ 
				return new Vector2( Screen.width, Screen.height ); 
			}
		}


		public static Vector3 ScreenToWorldPoint( Vector2 point )
		{
			if (Camera == null)
			{
				return Vector3.zero;
			}
			return Camera.ScreenToWorldPoint( new Vector3( point.x, point.y, -Camera.transform.position.z ) );
		}
		
		
		public static Vector3 ViewToWorldPoint( Vector2 point )
		{
			if (Camera == null)
			{
				return Vector3.zero;
			}
			return Camera.ViewportToWorldPoint( new Vector3( point.x, point.y, -Camera.transform.position.z ) );
		}


		public static Rect ViewToWorldRect( Rect rect )
		{
			var halfViewSizeX = ViewSize.x * 0.5f;
			var halfViewSizeY = ViewSize.y * 0.5f;
			return new Rect(
				(rect.xMin * halfViewSizeX) - halfViewSizeX,
				(rect.yMin * halfViewSizeY) - halfViewSizeY,
				rect.width * ViewSize.x,
				rect.height * ViewSize.y
			);
		}


		public static implicit operator bool( TouchManager instance )
		{
			return instance != null;
		}
	}
}

