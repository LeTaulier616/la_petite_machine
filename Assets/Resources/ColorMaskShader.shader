Shader "Hidden/ColorMaskShader" {
	Properties {
		_Color ("_COLORMASK_Color", Color) = (1,1,1)
	}
	SubShader {
		Color [_COLORMASK_Color]
		Pass {}
	}
}