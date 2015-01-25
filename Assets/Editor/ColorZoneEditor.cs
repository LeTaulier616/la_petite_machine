using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ColorZone))]
[CanEditMultipleObjects]
public class ColorZoneEditor : Editor 
{
	ColorZone _target;

	ColorController.ColorChoice newColorChoice;

	int colorGridInt;
	string[] colorStrings = new string[] {"None", "Left", "Right"};

	void OnEnable() 
	{
		_target = (ColorZone)target;

		InitializeGridInts();
	}

	public override void OnInspectorGUI()
	{
		newColorChoice = _target.zoneColor;
		
		EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);

		colorGridInt = GUILayout.SelectionGrid(colorGridInt, colorStrings, 3);

		UpdateEnums();

		UpdateVariables();

		if(GUI.changed)
		{
			EditorUtility.SetDirty(_target);
		}
	}

	public void InitializeGridInts()
	{
		switch(_target.zoneColor)
		{
			case ColorController.ColorChoice.None:
				colorGridInt = 0;
				break;
				
			case ColorController.ColorChoice.Left:
				colorGridInt = 1;
				break;
				
			case ColorController.ColorChoice.Right:
				colorGridInt = 2;
				break;
		}
	}

	public void UpdateEnums()
	{
		switch(colorGridInt)
		{
		case 0:
			newColorChoice = ColorController.ColorChoice.None;
			break;
			
		case 1:
			newColorChoice = ColorController.ColorChoice.Left;
			break;
			
		case 2:
			newColorChoice = ColorController.ColorChoice.Right;
			break;
		}
	}

	public void UpdateVariables()
	{
		if(newColorChoice != _target.zoneColor)
		{
			_target.zoneColor = newColorChoice;
			_target.UpdateZoneVariables(_target.zoneColor, _target.volume);
		}
	}
}
