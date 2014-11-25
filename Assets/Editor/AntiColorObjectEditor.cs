using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AntiColorObject))]
[CanEditMultipleObjects]
public class AntiColorObjectEditor : Editor
{
	AntiColorObject _target;

	ColoredObject.ColorChoice newColorChoice;

	GUIStyle boldStyle;
	GUIStyle headerStyle;

	void OnEnable()
	{
		_target = (AntiColorObject)target;
	}

	public override void OnInspectorGUI()
	{
		newColorChoice = _target.currentColor;

		headerStyle = new GUIStyle();
		headerStyle.fontStyle = FontStyle.Bold;
		headerStyle.normal.textColor = Color.white;
		
		boldStyle = new GUIStyle(GUI.skin.button);
		boldStyle.fontStyle = FontStyle.Bold;
		boldStyle.normal.textColor = Color.white;
		
		EditorGUILayout.LabelField("Color", headerStyle);
		
		EditorGUILayout.BeginHorizontal();
			
			GUI.backgroundColor = Color.white;
			
			drawColorButton(ColoredObject.ColorChoice.None, "None");
			
			//GUI.backgroundColor = ColorController.Instance.colorParameters.leftColor;

			drawColorButton(ColoredObject.ColorChoice.Left, "Left");
			
			//GUI.backgroundColor = ColorController.Instance.colorParameters.rightColor;
						
			drawColorButton(ColoredObject.ColorChoice.Right, "Right");

			/*
				if(ColorController.Instance != null)
					GUI.backgroundColor = ColorController.Instance.colorParameters.upColor;
				
				else
					GUI.backgroundColor = new Color32(255, 165, 168, 255);
				
				drawColorButton(ColoredObject.ColorChoice.Up, "Up");
			
				if(ColorController.Instance != null)
					GUI.backgroundColor = ColorController.Instance.colorParameters.downColor;
				
				else
					GUI.backgroundColor = new Color32(128, 255, 160, 255);
				
				drawColorButton(ColoredObject.ColorChoice.Down, "Down");
			*/
			
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Mode", headerStyle);

		EditorGUILayout.BeginHorizontal();

			if(_target.hideMode == ColoredObject.HideMode.Invisible)
				GUI.backgroundColor = Color.green;

			else
				GUI.backgroundColor = Color.white;

			drawModeButton(ColoredObject.HideMode.Invisible, "Invisible");

			if(_target.hideMode == ColoredObject.HideMode.Transparent)
				GUI.backgroundColor = Color.green;
			
			else
				GUI.backgroundColor = Color.white;
			
			drawModeButton(ColoredObject.HideMode.Transparent, "Transparent");

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Separator();
		
		EditorGUILayout.LabelField("Status", headerStyle);
		
		EditorGUILayout.BeginHorizontal();
		
			if(_target.isStatic)
				GUI.backgroundColor = Color.green;
			
			else
				GUI.backgroundColor = Color.white;
			
			drawStateButton(true, "Static");
			
			if(_target.isStatic)
				GUI.backgroundColor = Color.white;
			
			else
				GUI.backgroundColor = Color.green;
			
			drawStateButton(false, "Non-Static");
			
			
		EditorGUILayout.EndHorizontal();

		if(newColorChoice != _target.currentColor)
		{
			_target.currentColor = newColorChoice;
			_target.updateColor(_target.currentColor);
		}

		if(GUI.changed)
		{
			EditorUtility.SetDirty(_target);
		}

	}

	public void drawModeButton(ColoredObject.HideMode mode, string label)
	{
		if(mode == _target.hideMode)
		{
			if(GUILayout.Button(label, boldStyle))
			{
				_target.hideMode = mode;
			}
		}
		
		else
		{
			if(GUILayout.Button(label))
			{
				_target.hideMode = mode;
			}
		}
	}

	public void drawStateButton(bool state, string label)
	{
		if(state == _target.isStatic)
		{
			if(GUILayout.Button(label, boldStyle))
			{
				_target.isStatic = state;
			}
		}
		
		else
		{
			if(GUILayout.Button(label))
			{
				_target.isStatic = state;
			}
		}
	}
	
	public void drawColorButton(ColoredObject.ColorChoice buttonChoice, string label)
	{
		if(buttonChoice == _target.currentColor)
		{
			if(GUILayout.Button(label, boldStyle))
			{
				newColorChoice = buttonChoice;
			}
		}
		
		else
		{
			if(GUILayout.Button(label))
			{
				newColorChoice = buttonChoice;
			}
		}
	}
}
