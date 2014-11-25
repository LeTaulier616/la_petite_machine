Shader "Colored Objects/Colored Objects Shader" 
{
	Properties 
	{
	    _Color ("Main Color", Color) = (1,1,1,1)
	    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader 
	{
	    ZWrite Off
	    Alphatest Greater 0
	    Tags {Queue=Transparent}
	    Blend SrcAlpha OneMinusSrcAlpha 
	    ColorMask RGB
	    Pass 
	    {
	        ColorMaterial AmbientAndDiffuse
	        Lighting On
	        SeparateSpecular On
	        SetTexture [_MainTex] {
	            Combine texture * primary, texture * primary
	        }
	        SetTexture [_MainTex] {
	            constantColor [_Color]
	            Combine previous * constant DOUBLE, previous * constant
	        } 
	    }
	}

Fallback "Alpha/VertexLit", 1

}