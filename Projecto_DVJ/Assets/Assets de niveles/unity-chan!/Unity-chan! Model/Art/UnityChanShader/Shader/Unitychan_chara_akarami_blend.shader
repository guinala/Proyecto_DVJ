Shader "UnityChan/Blush - Transparent"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.8, 0.8, 1, 1)
        _MainTex ("Diffuse", 2D) = "white" {}
        _FalloffSampler ("Falloff Control", 2D) = "white" {}
        _RimLightSampler ("RimLight Control", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Geometry+3"
            "IgnoreProjector"="True"
            "RenderType"="Overlay"
            "LightMode"="UniversalForward"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha, One One
            ZWrite Off
            Cull Back
            ZTest LEqual
            HLSLPROGRAM
			#pragma multi_compile_fwdbase
            #pragma target 3.0
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "CharaSkin.hlsl" // Incluye tu archivo HLSL actualizado aquí
            ENDHLSL
        }
    }

    FallBack "Transparent/Cutout/Diffuse"
}