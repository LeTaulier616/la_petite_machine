using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ColorZone))]
[CanEditMultipleObjects]
public class ColorZoneEditor : Editor 
{
	ColorZone _target;

	ColorController.ColorChoice newColorChoice;

	GUIStyle boldStyle;
	GUIStyle headerStyle;

	void OnEnable() 
	{
		_target = (ColorZone)target;
	}

	public override void OnInspectorGUI()
	{
		newColorChoice = _target.zoneColor;
		
		headerStyle = new GUIStyle();
		headerStyle.fontStyle = FontStyle.Bold;
		headerStyle.normal.textColor = Color.white;
		
		boldStyle = new GUIStyle(GUI.skin.button);
		boldStyle.fontStyle = FontStyle.Bold;
		boldStyle.normal.textColor = Color.white;

		EditorGUILayout.LabelField("Color", headerStyle);
		
		EditorGUILayout.BeginHorizontal();
								
			//GUI.backgroundColor = ColorController.Instance.colorParameters.leftColor;
			
			drawColorButton(ColorController.ColorChoice.Left, "Left");
			
			//GUI.backgroundColor = ColorController.Instance.colorParameters.rightColor;
			
			drawColorButton(ColorController.ColorChoice.Right, "Right");
		
		EditorGUILayout.EndHorizontal();

		if(newColorChoice != _target.zoneColor)
		{
			_target.zoneColor = newColorChoice;
			_target.UpdateZoneVariables(_target.zoneColor, _target.volume);
		}
		
		if(GUI.changed)
		{
			EditorUtility.SetDirty(_target);
		}
	}

	public void drawColorButton(ColorController.ColorChoice buttonChoice, string label)
	{
		if(buttonChoice == _target.zoneColor)
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
