using System;
using System.Collections.Generic;
using UnityEngine;


namespace InControl
{
	public class TouchTrackControl : TouchControl
	{
		[Header("Dimensions")]

		public Rect
			activeArea = new Rect( 0.5f, 0.5f, 0.5f, 0.5f );

		[Header("Analog Target")]

		public AnalogTarget target = AnalogTarget.LeftStick;
		public float scale = 1.0f;

		Rect worldActiveArea;
		Vector3 lastPosition;
		Vector3 thisPosition;
		Touch currentTouch;


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
		}


		public override void SubmitControlState( ulong updateTick )
		{
			var delta = thisPosition - lastPosition;
			if (delta != Vector3.zero)
			{
				SubmitRawAnalogValue( target, delta * scale, updateTick );
				lastPosition = thisPosition;
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
				thisPosition = beganPosition;
				lastPosition = beganPosition;
				currentTouch = touch;
			}
		}


		public override void TouchMoved( Touch touch )
		{
			if (currentTouch != touch)
			{
				return;
			}

			thisPosition = TouchManager.ScreenToWorldPoint( touch.position );
		}


		public override void TouchEnded( Touch touch )
		{
			if (currentTouch != touch)
			{
				return;
			}

			thisPosition = Vector3.zero;
			lastPosition = Vector3.zero;
			currentTouch = null;
		}
	}
}

