using System;
using System.Collections.Generic;
using UnityEngine;


namespace InControl
{
	public class TouchSwipeControl : TouchControl
	{
		[Header("Dimensions")]

		public Rect
			activeArea = new Rect( 0.5f, 0.5f, 0.5f, 0.5f );

		[Range(0,1)] 
		public float sensitivity = 0.1f;
		
		[Header("Analog Target")]
		
		public AnalogTarget target = AnalogTarget.None;
		public SnapAngles snapAngles = SnapAngles.None;

		[Header("Button Targets")]

		public ButtonTarget upTarget = ButtonTarget.None;
		public ButtonTarget downTarget = ButtonTarget.None;
		public ButtonTarget leftTarget = ButtonTarget.None;
		public ButtonTarget rightTarget = ButtonTarget.None;
		public bool oneSwipePerTouch = false;

		Rect worldActiveArea;
		Vector3 currentVector;
		Vector3 lastPosition;
		Touch currentTouch;

		bool fireButtonTarget;
		ButtonTarget nextButtonTarget;
		ButtonTarget lastButtonTarget;


		public override void CreateControl()
		{
			ConfigureControl();
		}


		public override void DestroyControl()
		{
		}
		
		
		public override void ConfigureControl()
		{
			worldActiveArea = TouchManager.ViewToWorldRect( activeArea );
		}
		
		
		public override void DrawGizmos()
		{
			Utility.DrawRectGizmo( worldActiveArea, Color.yellow );
//			Gizmos.color = Color.red;
//			Gizmos.DrawLine( Vector3.zero, currentVector * 2.0f );
		}
		

		public override void SubmitControlState( ulong updateTick )
		{
			var value = SnapTo( currentVector, snapAngles );
			SubmitAnalogValue( target, value, 0.0f, 1.0f, updateTick );

			SubmitButtonState( upTarget, fireButtonTarget && nextButtonTarget == upTarget, updateTick );
			SubmitButtonState( rightTarget, fireButtonTarget && nextButtonTarget == rightTarget, updateTick );
			SubmitButtonState( downTarget, fireButtonTarget && nextButtonTarget == downTarget, updateTick );
			SubmitButtonState( leftTarget, fireButtonTarget && nextButtonTarget == leftTarget, updateTick );

			if (fireButtonTarget && nextButtonTarget != ButtonTarget.None)
			{
				fireButtonTarget = !oneSwipePerTouch;
				lastButtonTarget = nextButtonTarget;
				nextButtonTarget = ButtonTarget.None;
			}
		}
		
		
		public override void TouchBegan( Touch touch )
		{
			if (currentTouch != null)
			{
				return;
			}

			var beganPosition = TouchManager.ScreenToWorldPoint( touch.position );
			if (worldActiveArea.Contains( beganPosition ))
			{
				lastPosition = beganPosition;
				currentTouch = touch;
				currentVector = Vector2.zero;

				fireButtonTarget = true;
				nextButtonTarget = ButtonTarget.None;
				lastButtonTarget = ButtonTarget.None;
			}
		}
		
		
		public override void TouchMoved( Touch touch )
		{
			if (currentTouch != touch)
			{
				return;
			}

			var movedPosition = TouchManager.ScreenToWorldPoint( touch.position );
			var delta = movedPosition - lastPosition;
			if (delta.magnitude > sensitivity)
			{
				lastPosition = movedPosition;
				currentVector = delta.normalized;

				if (fireButtonTarget)
				{
					var thisButtonTarget = GetButtonTargetForVector( currentVector );
					if (thisButtonTarget != lastButtonTarget)
					{
						nextButtonTarget = thisButtonTarget;
					}
				}
			}
		}
		
		
		public override void TouchEnded( Touch touch )
		{
			if (currentTouch != touch)
			{
				return;
			}

			currentTouch = null;
			currentVector = Vector2.zero;

			fireButtonTarget = false;
			nextButtonTarget = ButtonTarget.None;
			lastButtonTarget = ButtonTarget.None;
		}


		ButtonTarget GetButtonTargetForVector( Vector2 vector )
		{
			Vector2 snappedVector = SnapTo( vector, SnapAngles.Four );

			if (snappedVector == Vector2.up)
			{
				return upTarget;
			}

			if (snappedVector == Vector2.right)
			{
				return rightTarget;
			}

			if (snappedVector == -Vector2.up)
			{
				return downTarget;
			}

			if (snappedVector == -Vector2.right)
			{
				return leftTarget;
			}

			return ButtonTarget.None;
		}
	}
}

