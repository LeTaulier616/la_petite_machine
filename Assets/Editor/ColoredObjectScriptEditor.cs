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
		ColoredObjectScript.ColorChoice newColorChoice = (ColoredObjectScript.ColorChoice)EditorGUILayout.EnumPopup("Color", _target.currentColor);

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
}
