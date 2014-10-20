#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;


namespace InControl
{
	[CustomEditor(typeof(TouchManager))]
	public class TouchManagerEditor : Editor
	{	
		Texture headerTexture;
		
		
		void OnEnable()
		{
			var path = AssetDatabase.GetAssetPath( MonoScript.FromScriptableObject( this ) );
			headerTexture = Resources.LoadAssetAtPath<Texture>( Path.GetDirectoryName( path ) + "/Images/TouchManagerHeader.png" );
		}
		
		
		public override void OnInspectorGUI()
		{
			GUILayout.Space( 5.0f );
			
			var headerRect = GUILayoutUtility.GetRect( 0.0f, -22.0f );
			headerRect.width = headerTexture.width;
			headerRect.height = headerTexture.height;
			GUILayout.Space( headerRect.height );

			DrawDefaultInspector();

			GUI.DrawTexture( headerRect, headerTexture );
		}
	}
}
#endif

