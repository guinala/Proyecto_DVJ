// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Character shader
// Includes falloff shadow and highlight, specular, reflection, and normal mapping
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#define ENABLE_CAST_SHADOWS

// Material parameters
float4 _Color;
float4 _ShadowColor;
float4 _LightColor0;
float _SpecularPower;
float4 _MainTex_ST;

// Textures
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_FalloffSampler);
SAMPLER(sampler_FalloffSampler);
TEXTURE2D(_RimLightSampler);
SAMPLER(sampler_RimLightSampler);
TEXTURE2D(_SpecularReflectionSampler);
SAMPLER(sampler_SpecularReflectionSampler);
TEXTURE2D(_EnvMapSampler);
SAMPLER(sampler_EnvMapSampler);
TEXTURE2D(_NormalMapSampler);
SAMPLER(sampler_NormalMapSampler);

// Constants
#define FALLOFF_POWER 0.3

struct Attributes
{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 lightmapUV : TEXCOORD1;
};

#ifdef ENABLE_CAST_SHADOWS

// Structure from vertex shader to fragment shader
struct Varyings
{
	float4 pos      : SV_POSITION;
	float4 shadowCoord : TEXCOORD1;
	float2 uv       : TEXCOORD2;
	float3 eyeDir   : TEXCOORD3;
	float3 lightDir : TEXCOORD4;
	float3 normal   : TEXCOORD5;
#ifdef ENABLE_NORMAL_MAP
	float3 tangent  : TEXCOORD6;
	float3 binormal : TEXCOORD7;
#endif
};

#else

// Structure from vertex shader to fragment shader
struct Varyings
{
	float4 pos      : SV_POSITION;
	float2 uv       : TEXCOORD0;
	float3 eyeDir   : TEXCOORD1;
	float3 lightDir : TEXCOORD2;
	float3 normal   : TEXCOORD3;
#ifdef ENABLE_NORMAL_MAP
	float3 tangent  : TEXCOORD4;
	float3 binormal : TEXCOORD5;
#endif
};

#endif

// Float types
// #define float_t    half
// #define float2_t   half2
// #define float3_t   half3
// #define float4_t   half4
// #define float3x3_t half3x3

// Vertex shader
Varyings vert (Attributes v)
{
	Varyings o;
	o.pos = TransformObjectToHClip( v.vertex.xyz );
	o.uv.xy = TRANSFORM_TEX( v.texcoord.xy, _MainTex );
	o.normal = normalize( mul( unity_ObjectToWorld, half4( v.normal, 0 ) ).xyz );
	
	// Eye direction vector
	half4 worldPos = mul( unity_ObjectToWorld, v.vertex );
	o.eyeDir.xyz = normalize( _WorldSpaceCameraPos.xyz - worldPos.xyz ).xyz;
	// o.lightDir = WorldSpaceLightDir( v.vertex );
	Light mainLight = GetMainLight();
    o.lightDir = normalize(mainLight.direction.xyz);
	
#ifdef ENABLE_NORMAL_MAP	
	// Binormal and tangent (for normal map)
	o.tangent = normalize( mul( unity_ObjectToWorld, half4( v.tangent.xyz, 0 ) ).xyz );
	o.binormal = normalize( cross( o.normal, o.tangent ) * v.tangent.w );
#endif

#ifdef ENABLE_CAST_SHADOWS
	o.shadowCoord = TransformWorldToShadowCoord(worldPos);
#endif

	return o;
}

// Overlay blend
inline half3 GetOverlayColor( half3 inUpper, half3 inLower )
{
	half3 oneMinusLower = half3( 1.0, 1.0, 1.0 ) - inLower;
	half3 valUnit = 2.0 * oneMinusLower;
	half3 minValue = 2.0 * inLower - half3( 1.0, 1.0, 1.0 );
	half3 greaterResult = inUpper * valUnit + minValue;

	half3 lowerResult = 2.0 * inLower * inUpper;

	half3 lerpVals = round(inLower);
	return lerp(lowerResult, greaterResult, lerpVals);
}

#ifdef ENABLE_NORMAL_MAP

// Compute normal from normal map
inline half3 GetNormalFromMap( Varyings input )
{
	half3 normalVec = normalize( SAMPLE_TEXTURE2D(_NormalMapSampler, sampler_NormalMapSampler, input.uv).xyz * 2.0 - 1.0 );
	half3x3 localToWorldTranspose = half3x3(
		input.tangent,
		input.binormal,
		input.normal
	);
	
	normalVec = normalize( mul( normalVec, localToWorldTranspose ) );
	return normalVec;
}

#endif

// Fragment shader
float4 frag( Varyings i ) : SV_Target
{
	half4 diffSamplerColor = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, i.uv );

#ifdef ENABLE_NORMAL_MAP
	half3 normalVec = GetNormalFromMap( i );
#else
	half3 normalVec = i.normal;
#endif

	// Falloff. Convert the angle between the normal and the camera direction into a lookup for the gradient
	half normalDotEye = dot( normalVec, i.eyeDir.xyz );
	half falloffU = clamp( 1.0 - abs( normalDotEye ), 0.02, 0.98 );
	half4 falloffSamplerColor = FALLOFF_POWER * SAMPLE_TEXTURE2D( _FalloffSampler, sampler_FalloffSampler, float2( falloffU, 0.25f ) );
	half3 shadowColor = diffSamplerColor.rgb * diffSamplerColor.rgb;
	half3 combinedColor = lerp( diffSamplerColor.rgb, shadowColor, falloffSamplerColor.r );
	combinedColor *= ( 1.0 + falloffSamplerColor.rgb * falloffSamplerColor.a );

	// Specular
	// Use the eye vector as the light vector
	half4 reflectionMaskColor = SAMPLE_TEXTURE2D( _SpecularReflectionSampler, sampler_SpecularReflectionSampler, i.uv.xy );
	half specularDot = dot( normalVec, i.eyeDir.xyz );
	half4 lighting = lit( normalDotEye, specularDot, _SpecularPower );
	half3 specularColor = saturate( lighting.z ) * reflectionMaskColor.rgb * diffSamplerColor.rgb;
	combinedColor += specularColor;
	
	// Reflection
	half3 reflectVector = reflect( -i.eyeDir.xyz, normalVec ).xzy;
	half2 sphereMapCoords = 0.5 * ( half2( 1.0, 1.0 ) + reflectVector.xy );
	half3 reflectColor = SAMPLE_TEXTURE2D( _EnvMapSampler, sampler_EnvMapSampler, sphereMapCoords ).rgb;
	reflectColor = GetOverlayColor( reflectColor, combinedColor );

	combinedColor = lerp( combinedColor, reflectColor, reflectionMaskColor.a );
	combinedColor *= _Color.rgb * (half3 (0.1f, 0.1f, 0.1f) + (_LightColor0.rgb * 0.9f));
	float opacity = diffSamplerColor.a * _Color.a * _LightColor0.a;

#ifdef ENABLE_CAST_SHADOWS
	// Cast shadows
	shadowColor = _ShadowColor.rgb * combinedColor;
	Light mainLight = GetMainLight(i.shadowCoord);
	half attenuation = saturate( 2.0 * mainLight.distanceAttenuation - 1.0 );
	combinedColor = lerp( shadowColor, combinedColor, attenuation );
#endif

	// Rimlight
	half rimlightDot = saturate( 0.5 * ( dot( normalVec, i.lightDir ) + 1.0 ) );
	falloffU = saturate( rimlightDot * falloffU );
	falloffU = SAMPLE_TEXTURE2D( _RimLightSampler, sampler_RimLightSampler, float2( falloffU, 0.25f ) ).r;
	half3 lightColor = diffSamplerColor.rgb; // * 2.0;
	combinedColor += falloffU * lightColor;

	return float4( combinedColor, opacity );
}
