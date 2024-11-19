// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Outline shader

// Material parameters
float4 _Color;
float4 _LightColor0;
float _EdgeThickness = 1.0;
float4 _MainTex_ST;

// Textures
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

struct Attributes
{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 lightmapUV : TEXCOORD1;
};

// Structure from vertex shader to fragment shader
struct Varyings
{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
};

// Float types
// #define float_t  half
// #define float2_t half2
// #define float3_t half3
// #define float4_t half4

// Outline thickness multiplier
#define INV_EDGE_THICKNESS_DIVISOR 0.00285
// Outline color parameters
#define SATURATION_FACTOR 0.6
#define BRIGHTNESS_FACTOR 0.8

// Vertex shader
Varyings vert( Attributes v )
{
	Varyings o;
	o.uv = TRANSFORM_TEX( v.texcoord.xy, _MainTex );

	half4 projSpacePos = TransformObjectToHClip( v.vertex );
	half4 projSpaceNormal = normalize( TransformObjectToHClip( half4( v.normal, 0 ) ) );
	half4 scaledNormal = _EdgeThickness * INV_EDGE_THICKNESS_DIVISOR * projSpaceNormal; // * projSpacePos.w;

	scaledNormal.z += 0.00001;
	o.pos = projSpacePos + scaledNormal;

	return o;
}

// Fragment shader
float4 frag( Varyings i ) : COLOR
{
	half4 diffuseMapColor = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, i.uv );

	half maxChan = max( max( diffuseMapColor.r, diffuseMapColor.g ), diffuseMapColor.b );
	half4 newMapColor = diffuseMapColor;

	maxChan -= ( 1.0 / 255.0 );
	half3 lerpVals = saturate( ( newMapColor.rgb - float3( maxChan, maxChan, maxChan ) ) * 255.0 );
	newMapColor.rgb = lerp( SATURATION_FACTOR * newMapColor.rgb, newMapColor.rgb, lerpVals );
	
	return float4( BRIGHTNESS_FACTOR * newMapColor.rgb * diffuseMapColor.rgb, diffuseMapColor.a ) * _Color * _LightColor0; 
}
