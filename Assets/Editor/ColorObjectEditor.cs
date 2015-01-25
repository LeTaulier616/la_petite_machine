using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ColorObject))]
[CanEditMultipleObjects]
public class ColorObjectEditor : Editor
{
	ColorObject _target;

	ColoredObject.ColorChoice newColorChoice;
	ColoredObject.HideMode newHideMode;
	bool newIsStatic;

	int colorGridInt;
	string[] colorStrings = new string[] {"None", "Left", "Right"};

	int modeGridInt;
	string[] modeStrings = new string[] {"Invisible", "Transparent"};

	int statusGridInt;
	string[] statusStrings = new string[] {"Static", "Non-Static"};

	void OnEnable()
	{
		_target = (ColorObject)target;

		InitializeGridInts();
	}
	
	public override void OnInspectorGUI()
	{
		newColorChoice = _target.currentColor;
		newHideMode = _target.hideMode;
		newIsStatic = _target.isStatic;

		EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);

		colorGridInt = GUILayout.SelectionGrid(colorGridInt, colorStrings, 3);

		EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);

		modeGridInt = GUILayout.SelectionGrid(modeGridInt, modeStrings, 2);

		EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);

		statusGridInt = GUILayout.SelectionGrid(statusGridInt, statusStrings, 2);

		UpdateEnums();

		UpdateVariables();

		if(GUI.changed)
		{
			EditorUtility.SetDirty(_target);
		}
	}

	public void InitializeGridInts()
	{
		switch(_target.currentColor)
		{
			case ColoredObject.ColorChoice.None:
				colorGridInt = 0;
				break;

			case ColoredObject.ColorChoice.Left:
				colorGridInt = 1;
				break;

			case ColoredObject.ColorChoice.Right:
				colorGridInt = 2;
				break;
		}

		switch(_target.hideMode)
		{
			case ColoredObject.HideMode.Invisible:
				modeGridInt = 0;
				break;

			case ColoredObject.HideMode.Transparent:
				modeGridInt = 1;
				break;
		}

		switch(_target.isStatic)
		{
			case true:
				statusGridInt = 0;
				break;

			case false:
				statusGridInt = 1;
				break;
		}
	}

	public void UpdateEnums()
	{
		switch(colorGridInt)
		{
			case 0:
				newColorChoice = ColoredObject.ColorChoice.None;
				break;

			case 1:
				newColorChoice = ColoredObject.ColorChoice.Left;
				break;

			case 2:
				newColorChoice = ColoredObject.ColorChoice.Right;
				break;
		}

		switch(modeGridInt)
		{
			case 0:
				newHideMode = ColoredObject.HideMode.Invisible;
				break;

			case 1:
				newHideMode = ColoredObject.HideMode.Transparent;
				break;
		}

		switch(statusGridInt)
		{
			case 0:
				newIsStatic = true;
				break;

			case 1:
				newIsStatic = false;
				break;
		}
	}

	public void UpdateVariables()
	{
		if(newColorChoice != _target.currentColor)
		{
			_target.currentColor = newColorChoice;
			_target.updateColor(_target.currentColor);
		}

		if(newHideMode != _target.hideMode)
		{
			_target.hideMode = newHideMode;
		}

		if(newIsStatic != _target.isStatic)
		{
			_target.isStatic = newIsStatic;
		}
	}
}
