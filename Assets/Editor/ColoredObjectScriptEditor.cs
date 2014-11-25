using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ColoredObjectScript))]
[CanEditMultipleObjects]
public class ColoredObjectScriptEditor : Editor
{
	ColoredObjectScript _target;

	ColoredObjectScript.ColorChoice newColorChoice;

	GUIStyle boldStyle;
	GUIStyle headerStyle;


	void OnEnable()
	{
		_target = (ColoredObjectScript)target;
	}
	
	public override void OnInspectorGUI()
	{
		newColorChoice = _target.currentColor;

		headerStyle = new GUIStyle();
		headerStyle.fontSize = 14;
		headerStyle.fontStyle = FontStyle.Bold;
		headerStyle.normal.textColor = Color.white;

		boldStyle = new GUIStyle(GUI.skin.button);
		boldStyle.fontStyle = FontStyle.Bold;
		boldStyle.normal.textColor = Color.white;
		
		EditorGUILayout.LabelField("Color", headerStyle);
		
		EditorGUILayout.BeginHorizontal();
		
			GUI.backgroundColor = Color.white;
			
			drawColorButton(ColoredObjectScript.ColorChoice.None, "None");
		
			if(ColorController.Instance != null)
				GUI.backgroundColor = ColorController.Instance.colorParameters.leftColor;

			else
				GUI.backgroundColor = new Color32(207, 140, 255, 255);

			drawColorButton(ColoredObjectScript.ColorChoice.Left, "Left");
			
			if(ColorController.Instance != null)
				GUI.backgroundColor = ColorController.Instance.colorParameters.rightColor;
			
			else
				GUI.backgroundColor = new Color32(255, 255, 128, 255);
			
			drawColorButton(ColoredObjectScript.ColorChoice.Right, "Right");

			/*
			if(ColorController.Instance != null)
				GUI.backgroundColor = ColorController.Instance.colorParameters.upColor;
			
			else
				GUI.backgroundColor = new Color32(255, 165, 168, 255);
			
			drawColorButton(ColoredObjectScript.ColorChoice.Up, "Up");
		
			if(ColorController.Instance != null)
				GUI.backgroundColor = ColorController.Instance.colorParameters.downColor;
			
			else
				GUI.backgroundColor = new Color32(128, 255, 160, 255);
			
			drawColorButton(ColoredObjectScript.ColorChoice.Down, "Down");
			*/
			
		EditorGUILayout.EndHorizontal();

		if(newColorChoice != _target.currentColor)
		{
			_target.currentColor = newColorChoice;
			_target.updateColor(_target.currentColor);
		}
		
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
		
		if(GUI.changed)
		{
			EditorUtility.SetDirty(_target);
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

	public void drawColorButton(ColoredObjectScript.ColorChoice buttonChoice, string label)
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
