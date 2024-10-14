

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

    o.cameraSpaceNearPos = mul(unity_CameraInvProjection, o.screenNear);
    o.cameraSpaceNearPos.z = _ProjectionParams.y;
    o.cameraSpaceFarPos = mul(unity_CameraInvProjection, o.screenFar);
    o.cameraSpaceFarPos.z = _ProjectionParams.z;
    o.worldSpaceNearPos = mul(unity_CameraToWorld,float4(o.cameraSpaceNearPos,1));
    o.worldSpaceFarPos = mul(unity_CameraToWorld,float4(o.cameraSpaceFarPos,1));
    o.worldSpaceZeroPoint = mul(unity_ObjectToWorld,float4(0,0,0,1));
    float mid = 0.5 * (_ProjectionParams.y + _ProjectionParams.z);
    o.vertex.zw = UnityViewToClipPos(float3(0,0,-mid)).zw;
    o.vertex.xy = v.vertex.xy * o.vertex.w;
    return o;
}



float3 worldPosFromDepthMap(v2f data, out float originDepth, out float linear01Depth)
{
    originDepth = tex2Dlod(_CameraDepthTexture, float4(0.5*data.screenNear.xy/data.screenNear.w + 0.5,0,0)).x;
    linear01Depth = Linear01Depth(originDepth);
    return lerp(_WorldSpaceCameraPos.xyz,data.worldSpaceFarPos,linear01Depth);
}