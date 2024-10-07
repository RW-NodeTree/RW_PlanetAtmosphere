float3 normal;
float3 tangent;

float2 ringFromTo;
sampler2D ringMap;

float radius;
sampler2D sphereMap;

inline float4 sampleRing(float u);

float4 sampleRingBasic(float u)
{
    return tex2Dlod(ringMap,float4(u,0.5,0.0,0.0));
}

float4 getColorFromRing(float3 start, float3 dir, out float3 crossPoint, out float t)
{
    normal = normalize(normal);
    dir = normalize(dir);
    float3 d3 = normalize(cross(normal,dir));
    float3 d2 = normalize(cross(d3,normal));
    float3x3 mat = float3x3(normal,d2,d3);
    start = mul(mat,start);
    dir = mul(mat,dir);
    t = -start.x / dir.x;
    crossPoint = start + dir * t;
    float dis = length(crossPoint.yz);
    crossPoint = mul(crossPoint,mat);
    ringFromTo = abs(ringFromTo);
    // ringFromTo = float2(min(ringFromTo.x,ringFromTo.y),max(ringFromTo.x,ringFromTo.y));
    dis = (dis - ringFromTo.x) / (ringFromTo.y - ringFromTo.x);
    return sampleRing(dis) * step(0.0, t) * step(0.0, dis) * step(dis, 1.0);
    // return tex2Dlod(ringMap,float4(dis,0.5,0.0,0.0)) * step(0.0, t) * step(0.0, dis) * step(dis, 1.0);
}

inline float4 sampleSphere(float2 uv);

float4 sampleSphereBasic(float2 uv)
{
    return tex2Dlod(sphereMap,float4(uv,0.0,0.0));
}

void getColorFromSphere(
    float3 start,
    float3 dir,
    out float4 resultA,
    out float4 crossPointA,
    out float4 resultB,
    out float4 crossPointB
)
{
    resultA = 0;
    resultB = 0;
    radius = abs(radius);
    normal = normalize(normal);
    tangent = normalize(tangent);
    dir = normalize(dir);
    float3 midCrossPoint = start - dot(dir,start) * dir;
    crossPointA.xyz = midCrossPoint;
    crossPointB.xyz = midCrossPoint;
    crossPointA.w = 0;
    crossPointB.w = 0;
    float h0 = length(midCrossPoint);
    if(h0 <= radius)
    {
        float x0 = sqrt(radius * radius - h0 * h0);
        crossPointA.xyz -= x0 * dir;
        crossPointB.xyz += x0 * dir;
        float3 d2 = normalize(cross(normal,tangent));
        float3 d3 = normalize(cross(d2,normal));
        crossPointA.w = step(0.0,dot(crossPointA.xyz-start,dir));
        crossPointB.w = step(0.0,dot(crossPointB.xyz-start,dir));
        float3x3 mat = float3x3(normal,d3,d2);
        d2 = normalize(mul(mat,crossPointA.xyz));
        d3 = normalize(mul(mat,crossPointB.xyz));
        // resultA = tex2Dlod(sphereMap,float4(atan2(d2.z, d2.y), acos(clamp(-d2.x,-1.0,1.0)),0,0));
        // resultB = tex2Dlod(sphereMap,float4(atan2(d3.z, d3.y), acos(clamp(-d3.x,-1.0,1.0)),0,0));
        resultA = sampleSphere(float2(0.5 * atan2(d2.z, d2.y), acos(clamp(-d2.x,-1.0,1.0))) * UNITY_INV_PI);
        resultB = sampleSphere(float2(0.5 * atan2(d3.z, d3.y), acos(clamp(-d3.x,-1.0,1.0))) * UNITY_INV_PI);
        resultA *= crossPointA.w;
        resultB *= crossPointB.w;
    }
}