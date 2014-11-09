using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RenderColorMask : MonoBehaviour
{
	public Camera maskCamera;
	public bool invertMask;

	private int width, height;
	private RenderTexture maskTexture;
	private Shader colorMaskShader;
	private int colorMaskLayer;

	void Start()
	{
		colorMaskShader = Shader.Find( "Hidden/ColorMaskShader" );
		colorMaskLayer = 1 << LayerMask.NameToLayer( "ColorMask" );

		UpdateRenderTextures();
		UpdateCameraProperties();
	}

	void Update()
	{		
		UpdateRenderTextures();
		UpdateCameraProperties();		
	}

	void UpdateRenderTextures()
	{
		int w = ( int )( camera.pixelWidth + 0.5f );
		int h = ( int )( camera.pixelHeight + 0.5f );

		if ( width != w || height != h )
		{
			width = w;
			height = h;

			if ( maskTexture != null )
				DestroyImmediate( maskTexture );

			maskTexture = new RenderTexture( width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Linear ) { hideFlags = HideFlags.HideAndDontSave, name = "MaskTexture" };
			maskTexture.antiAliasing = ( QualitySettings.antiAliasing > 0 ) ? QualitySettings.antiAliasing : 1;
			maskTexture.Create();
		}		

		if ( camera != null )
			camera.GetComponent<AmplifyColorEffect>().MaskTexture = maskTexture;
	}

	void UpdateCameraProperties()
	{
		maskCamera.CopyFrom( camera );
		maskCamera.targetTexture = maskTexture;
		maskCamera.clearFlags = CameraClearFlags.Nothing;
		if ( maskTexture.antiAliasing > 1 )
			// compensate for vertical offset introduced by Unity
			maskCamera.pixelRect = new Rect( 0, -1, width, height - 1 );
		else
			maskCamera.pixelRect = new Rect( 0, 0, width, height );
		maskCamera.enabled = false;
	}

	void OnPostRender()
	{
		RenderTexture.active = maskTexture;
		GL.Clear( true, true, invertMask ? Color.black : Color.white );		

		// Render all objects, except ColorMask layer
		Shader.SetGlobalColor( "_COLORMASK_Color", invertMask ? Color.black : Color.white );
		maskCamera.cullingMask = ~colorMaskLayer;
		maskCamera.RenderWithShader( colorMaskShader, "" );

		// Render only ColorMask layer
		Shader.SetGlobalColor( "_COLORMASK_Color", invertMask ? Color.white : Color.black );
		maskCamera.cullingMask = colorMaskLayer;
		maskCamera.RenderWithShader( colorMaskShader, "" );

		RenderTexture.active = null;
	}
}