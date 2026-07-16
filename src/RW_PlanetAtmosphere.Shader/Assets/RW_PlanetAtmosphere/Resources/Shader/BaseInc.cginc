

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
sampler2D _CameraDepthTexture;

struct appdata
{
    float4 vertex : POSITION;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float4 screenNear : TEXCOORD0;
    float4 screenFar : TEXCOORD1;
    float3 cameraSpaceNearPos : TEXCOORD2;
    float3 cameraSpaceFarPos : TEXCOORD3;
    float3 worldSpaceNearPos : TEXCOORD4;
    float3 worldSpaceFarPos : TEXCOORD5;
    float3 worldSpaceZeroPoint : TEXCOORD6;
    float2x2 cameraInvProjection : TEXCOORD7;
};

struct f2bColor
{
    float4 reflection : SV_TARGET0;
    float depthTexel : SV_TARGET1;
    float depth : SV_DEPTH;
};

struct f2bTrans
{
    float4 transFactor : SV_TARGET0;
    float depth : SV_DEPTH;
};

v2f basicVert (appdata v)
{
    v2f o;
    o.screenNear.zw = UnityViewToClipPos(float3(0,0,-_ProjectionParams.y)).zw;
    o.screenNear.xy = v.vertex.xy * o.screenNear.w;
    o.screenNear.y *= _ProjectionParams.x;

    o.screenFar.zw = UnityViewToClipPos(float3(0,0,-_ProjectionParams.z)).zw;
    o.screenFar.xy = v.vertex.xy * o.screenFar.w;
    o.screenFar.y *= _ProjectionParams.x;
    
    // The ZBuffer condition is a total bitch. Go figure it out with my matrix inversion.
    float2 col0 = mul(UNITY_MATRIX_P, float4(0,0,1,0)).zw;
    float2 col1 = mul(UNITY_MATRIX_P, float4(0,0,0,1)).zw;
    float mag = col0.x * col1.y - col0.y * col1.x;
    // float2 row0 = float2( col1.y,-col1.x) / mag;
    // float2 row1 = float2(-col0.y, col0.x) / mag;
    o.cameraInvProjection = float2x2(float2( col1.y,-col1.x) / mag,float2(-col0.y, col0.x) / mag);
    
    o.cameraSpaceNearPos.xy = mul(unity_CameraInvProjection, o.screenNear).xy;
    o.cameraSpaceNearPos.z = -_ProjectionParams.y;
    o.cameraSpaceFarPos.xy = mul(unity_CameraInvProjection, o.screenFar).xy;
    o.cameraSpaceFarPos.z = -_ProjectionParams.z;
    o.worldSpaceNearPos = mul(UNITY_MATRIX_I_V,float4(o.cameraSpaceNearPos,1));
    o.worldSpaceFarPos = mul(UNITY_MATRIX_I_V,float4(o.cameraSpaceFarPos,1));
    o.worldSpaceZeroPoint = mul(UNITY_MATRIX_M,float4(0,0,0,1));
    float mid = 0.5 * (_ProjectionParams.y + _ProjectionParams.z);
    o.vertex.zw = UnityViewToClipPos(float3(0,0,-mid)).zw;
    o.vertex.xy = v.vertex.xy * o.vertex.w;
    return o;
}



float3 worldPosFromDepthMap(v2f data, out float originDepth, out float linear01Depth)
{
    float2 uv = 0.5 * data.screenNear.xy / data.screenNear.w + 0.5;
    // uv.y *= _ProjectionParams.x;
    originDepth = tex2Dlod(_CameraDepthTexture, float4(uv,0,0)).x;
    uv = mul(data.cameraInvProjection, float2(originDepth, 1));
    linear01Depth = uv.x / uv.y;
    linear01Depth -= data.cameraSpaceNearPos.z;
    linear01Depth /= data.cameraSpaceFarPos.z - data.cameraSpaceNearPos.z;
    return lerp(data.worldSpaceNearPos, data.worldSpaceFarPos, linear01Depth);
}