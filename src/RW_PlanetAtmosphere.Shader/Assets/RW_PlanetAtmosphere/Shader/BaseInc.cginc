

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
    o.screenNear = float4(
        v.vertex.x * _ProjectionParams.y,
        v.vertex.y * _ProjectionParams.y * _ProjectionParams.x,
        (1.0 - _ZBufferParams.w * _ProjectionParams.y) / _ZBufferParams.z,
        _ProjectionParams.y
    );
    o.screenFar = float4(
        v.vertex.x * _ProjectionParams.z,
        v.vertex.y * _ProjectionParams.z * _ProjectionParams.x,
        (1.0 - _ZBufferParams.w * _ProjectionParams.z) / _ZBufferParams.z,
        _ProjectionParams.z
    );
    o.cameraSpaceNearPos = mul(unity_CameraInvProjection, o.screenNear);
    o.cameraSpaceNearPos.z = -o.cameraSpaceNearPos.z;
    o.cameraSpaceFarPos = mul(unity_CameraInvProjection, o.screenFar);
    o.cameraSpaceFarPos.z = -o.cameraSpaceFarPos.z;
    o.worldSpaceNearPos = mul(unity_CameraToWorld,float4(o.cameraSpaceNearPos,1));
    o.worldSpaceFarPos = mul(unity_CameraToWorld,float4(o.cameraSpaceFarPos,1));
    o.worldSpaceZeroPoint = mul(unity_ObjectToWorld,float4(0,0,0,1));
    float mid = 0.5 * (_ProjectionParams.y + _ProjectionParams.z);
    o.vertex = float4(
        v.vertex.xy * mid,
        (1.0 - _ZBufferParams.w * mid) / _ZBufferParams.z,
        mid
    );
    return o;
}



float3 worldPosFromDepthMap(v2f data, out float originDepth)
{
    originDepth = tex2Dlod(_CameraDepthTexture, float4(0.5*data.screenNear.xy/data.screenNear.w + 0.5,0,0)).x;
    float3
    result  = Linear01Depth(originDepth);
    result += step(1.0,result*1.001) * 1e16;
    result  = lerp(_WorldSpaceCameraPos.xyz,data.worldSpaceFarPos,result);
    return result;
}