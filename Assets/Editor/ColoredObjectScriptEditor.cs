using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ColoredObjectScript))]
[CanEditMultipleObjects]
public class ColoredObjectScriptEditor : Editor
{
	ColoredObjectScript _target;

	void OnEnable()
	{
		_target = (ColoredObjectScript)target;
	}
	
	public override void OnInspectorGUI()
	{
		ColoredObjectScript.ColorChoice newColorChoice = _target.currentColor;
		
		GUIStyle boldStyle = new GUIStyle();
		
		boldStyle.fontStyle = FontStyle.Bold;
		boldStyle.normal.textColor = Color.white;
		
		EditorGUILayout.LabelField("Color", boldStyle);
		
		EditorGUILayout.BeginHorizontal();
		
			GUI.backgroundColor = Color.white;
			if(GUILayout.Button("None"))
			{
				newColorChoice = ColoredObjectScript.ColorChoice.None;
			}
			
			GUI.backgroundColor = ColorController.Instance.colorParameters.leftColor;
			if(GUILayout.Button("Left"))
			{
				newColorChoice = ColoredObjectScript.ColorChoice.Left;
			}
			
			GUI.backgroundColor = ColorController.Instance.colorParameters.rightColor;
			if(GUILayout.Button("Right"))
			{
				newColorChoice = ColoredObjectScript.ColorChoice.Right;
			}
			
			GUI.backgroundColor = ColorController.Instance.colorParameters.upColor;
			if(GUILayout.Button("Up"))
			{
				newColorChoice = ColoredObjectScript.ColorChoice.Up;
			}
			
			GUI.backgroundColor = ColorController.Instance.colorParameters.downColor;
			if(GUILayout.Button("Down"))
			{
				newColorChoice = ColoredObjectScript.ColorChoice.Down;
			}
			
		EditorGUILayout.EndHorizontal();

		if(newColorChoice != _target.currentColor)
		{
			_target.currentColor = newColorChoice;
			_target.updateColor(_target.currentColor);
		}
		
		EditorGUILayout.Separator();
		
		EditorGUILayout.LabelField("Status", boldStyle);
			
		EditorGUILayout.BeginHorizontal();
			
			if(_target.isStatic)
				GUI.backgroundColor = Color.green;
			
			else
				GUI.backgroundColor = Color.white;
				
			if(GUILayout.Button("Static"))
			{
				_target.isStatic = true;
			}
			
			if(_target.isStatic)
				GUI.backgroundColor = Color.white;
			
			else
				GUI.backgroundColor = Color.green;
				
			if(GUILayout.Button("Non-Static"))
			{
				_target.isStatic = false;
			}
	
		EditorGUILayout.EndHorizontal();
		
		if(GUI.changed)
		{
			EditorUtility.SetDirty(_target);
		}
		
	}
}
