// Character skin shader for URP
// Includes falloff shadow
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#define ENABLE_CAST_SHADOWS

// Material parameters
float4 _Color;
float4 _ShadowColor;
float4 _LightColor0;
float4 _MainTex_ST;

// Textures
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_FalloffSampler);
SAMPLER(sampler_FalloffSampler);
TEXTURE2D(_RimLightSampler);
SAMPLER(sampler_RimLightSampler);

// Constants
#define FALLOFF_POWER 1.0

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 lightmapUV   : TEXCOORD1;
};

#ifdef ENABLE_CAST_SHADOWS

// Structure from vertex shader to fragment shader
struct Varyings
{
    float4 pos    : SV_POSITION;
    float3 normal : TEXCOORD0;
    float2 uv     : TEXCOORD1;
    float3 eyeDir : TEXCOORD2;
    float3 lightDir : TEXCOORD3;
    float4 shadowCoord : TEXCOORD4;
};

#else

// Structure from vertex shader to fragment shader
struct Varyings
{
    float4 pos    : SV_POSITION;
    float3 normal : TEXCOORD0;
    float2 uv     : TEXCOORD1;
    float3 eyeDir : TEXCOORD2;
    float3 lightDir : TEXCOORD3;
};

#endif

// Vertex shader
Varyings vert (Attributes input)
{
    Varyings output;
    output.pos = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
    output.normal = normalize(mul(unity_ObjectToWorld, half4(input.normalOS, 0)).xyz);
    // Eye direction vector
    half4 worldPos = mul(unity_ObjectToWorld, input.positionOS);
    output.eyeDir = normalize(_WorldSpaceCameraPos - worldPos.xyz);

    //output.lightDir = normalize(_WorldSpaceLightPos0.xyz);
    Light mainLight = GetMainLight();
    output.lightDir = normalize(mainLight.direction.xyz);
    
    #ifdef ENABLE_CAST_SHADOWS
        output.shadowCoord = TransformWorldToShadowCoord(worldPos);
    #endif

    return output;
}

// Fragment shader
float4 frag(Varyings i) : SV_Target
{
    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    float4 diffSamplerColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

    // Falloff. Convert the angle between the normal and the camera direction into a lookup for the gradient
    float normalDotEye = dot(i.normal, i.eyeDir);
    float falloffU = clamp(1 - abs(normalDotEye), 0.02, 0.98);
    float4 falloffSamplerColor = FALLOFF_POWER * SAMPLE_TEXTURE2D(_FalloffSampler, sampler_FalloffSampler, float2(falloffU, 0.25f));
    float3 combinedColor = lerp(diffSamplerColor.rgb, falloffSamplerColor.rgb * diffSamplerColor.rgb, falloffSamplerColor.a);

    // Rimlight
    float rimlightDot = saturate(0.5 * (dot(i.normal, i.lightDir) + 1.0));
    falloffU = saturate(rimlightDot * falloffU);
    falloffU = SAMPLE_TEXTURE2D(_RimLightSampler, sampler_RimLightSampler, float2(falloffU, 0.25f)).r;
    float3 lightColor = diffSamplerColor.rgb * 0.5;
    combinedColor += falloffU * lightColor;

    #ifdef ENABLE_CAST_SHADOWS
        // Cast shadows
        
        // float shadowAttenuation = SAMPLE_SHADOW(i.shadowCoord);
        // float3 shadowColor = _ShadowColor.rgb * combinedColor;
        // combinedColor = lerp(shadowColor, combinedColor, shadowAttenuation);

        float3 shadowColor = _ShadowColor.rgb * combinedColor;
        Light mainLight = GetMainLight(i.shadowCoord);
        float attenuation = saturate(2.0 * mainLight.distanceAttenuation - 1.0);
        combinedColor = lerp(shadowColor, combinedColor, attenuation);
    #endif

    return float4(combinedColor, diffSamplerColor.a) * _Color * (0.1 + (_LightColor0 * 0.9));
}
