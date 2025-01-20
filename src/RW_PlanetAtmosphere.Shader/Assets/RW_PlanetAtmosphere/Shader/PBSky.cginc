#include "UnityCG.cginc"

sampler2D translucentLUT;
sampler2D outSunLightLUT;
sampler2D inSunLightLUT;
sampler2D scatterLUT_Reayleigh;
sampler2D scatterLUT_Mie;

float4 translucentLUT_TexelSize;
float4 outSunLightLUT_TexelSize;
float4 inSunLightLUT_TexelSize;
float4 scatterLUTSize;
float4 mie_eccentricity;
float4 reayleigh_scatter;
float4 molecule_absorb;
float4 OZone_absorb;
float4 mie_scatter;
float4 mie_absorb;
float minh;
float maxh;
float H_Reayleigh;
float H_Mie;
float H_OZone;
float D_OZone;
float deltaL;
float deltaW;
float lengthL;
float lengthW;
uniform float sunRadius;
uniform float sunDistance;

#ifndef ingCount
#define ingCount 4096
#endif
#ifndef ingCountDyn
#define ingCountDyn 32
#endif
#ifndef ingLightCount
#define ingLightCount 64
#endif


float bright(float3 L)
{
    return L.x * 0.299 + L.y * 0.587 + L.z * 0.114;
}

// float3 hdr(float3 L) 
// {
//     L *= exposure;
//     // L = sqrt(L);
//     // L = lerp(pow(L * 0.38317, 1.0 / 2.2) , float3(1.0,1.0,1.0) - exp(-L), step(1.413,L));
//     // L = float3(1.0,1.0,1.0) - exp(-L);
//     return L;
// }


float3x3 Crossfloat3x3_W2L(float3 d1, float3 d2) //d1,d2 on x-y plane, x axis lock on d1
{
    d1 = normalize(d1);
    d2 = normalize(d2);
    float3 d3 = normalize(cross(d1,d2));
    d2 = normalize(cross(d3,d1));
    return float3x3(d1,d2,d3);
}

// void getLightSampleDir(in float3 baseDir, out float3 sampleDir[ingLightCount])
// {
//     float2 sunPerspectiveSinCos = sunRadius/sunDistance;
//     sunPerspectiveSinCos.y = sqrt(1.0 - sunPerspectiveSinCos.y * sunPerspectiveSinCos.y);
//     float3 d1 = abs(baseDir);
//     d1 = float3(
//         step(d1.x,d1.y) * step(d1.x,d1.z),
//         step(d1.y,d1.x) * step(d1.y,d1.z),
//         step(d1.z,d1.x) * step(d1.z,d1.y)
//     );
//     float3x3 mat = Crossfloat3x3_W2L(baseDir, d1);
//     for(int i = 0; i < ingLightCount; i++)
//     {
//         float ratio = float(i) / float(ingLightCount);
//         sampleDir[i] = normalize(mul(float3(sunPerspectiveSinCos.y, sunPerspectiveSinCos.x * cos(2.0 * PI * ratio), sunPerspectiveSinCos.x * sin(2.0 * PI * ratio)),mat));
//     }
// }


float2 pMap2AH(float2 map)
{
    map = saturate(map);
    // map.y *= map.y;
    // map.y *= maxh - minh;
    // map.y += minh;
    
    float H = maxh * maxh - minh * minh;
    float P = map.y * map.y * H;
    map.y = sqrt(P + minh * minh);
    
    float horizonZenith_ABS = sqrt(P) / map.y;

    map.x *= map.x;
    map.x *= (1.0 + horizonZenith_ABS);
    map.x -= horizonZenith_ABS;

    map = clamp(map,float2(-horizonZenith_ABS,minh),float2(1.0,maxh));
    return map;
}

float2 fMap2AH(float2 map)
{
    map = saturate(map);
    // map.y *= map.y;
    // map.y *= maxh - minh;
    // map.y += minh;
    
    float H = maxh * maxh - minh * minh;
    float P = map.y * map.y * H;
    map.y = sqrt(P + minh * minh);
    
    float horizonZenith_ABS = sqrt(P) / map.y;
    float upToHorizonZenithRange = 1.0 + horizonZenith_ABS;
    float horizonToMaxAndMinZenithRatio = sqrt((1.0 - horizonZenith_ABS)/upToHorizonZenithRange);

    map.x *= 1.0 + horizonToMaxAndMinZenithRatio;
    map.x -= horizonToMaxAndMinZenithRatio;
    map.x *= abs(map.x);
    map.x *= upToHorizonZenithRange;
    map.x -= horizonZenith_ABS;

    map = clamp(map,float2(-1.0,minh),float2(1.0,maxh));
    return map;
}


float2 Map2AD(float2 map)
{
    map = saturate(map);

    sunDistance = abs(sunDistance);
    sunRadius = abs(sunRadius);
    
    map.y = maxh / map.y;
    map.y = max(map.y,maxh);


    float maxRange = maxh + sunRadius;
    maxRange *= maxRange;
    maxRange = sqrt(sunDistance * sunDistance - maxRange);
    float cameraPlanetTangent = sqrt(map.y * map.y - maxh * maxh);
    maxRange += cameraPlanetTangent;
    float camera2Sun = sqrt(maxRange * maxRange + sunRadius * sunRadius);
    maxRange = (cameraPlanetTangent * maxRange - maxh * sunRadius) / (map.y * camera2Sun);
    map.x = sqrt(map.x);
    // map.x = sqrt(map.x);
    map.x = lerp(1.0, maxRange,map.x);

    return map;
}

float4 Map2AHLW(float4 map)
{
    map = saturate(map);
    // result = result.ywzx;
    //xyzw=wxzy
    //yxwz=xwyz
    //ywzx=xyzw
    map = map.xwyz;

    map.xy = fMap2AH(map.xy);
    // result.xy = Map2AH(result.xy);
    // result.w *= PI;
    float2 deltaLW = float2(deltaL, deltaW);
    deltaLW *= 4.0 * deltaLW;
    deltaLW = 1.0 / deltaLW;
    float2 startLW = sqrt(deltaLW);
    float2 scaleLW = 2.0 * startLW + 1.0;
    float2 lengthLW = float2(lengthL, lengthW);

    // ahlw.zw = (sqrt(abs(ahlw.zw) * scaleLW + deltaLW) - startLW) * sign(ahlw.zw);
    // ahlw.zw += 1.0;
    // ahlw.zw *= 0.5;
    // ahlw.zw /= lengthLW;
    // ahlw.zw -= (1.0 / lengthLW) - 1.0;
    map.zw += (1.0 / lengthLW) - 1.0;
    map.zw *= lengthLW;
    map.zw *= 2.0;
    map.zw -= 1.0;
    float2 signLW = sign(map.zw);
    map.zw = abs(map.zw) + startLW;
    map.zw *= map.zw;
    map.zw -= deltaLW;
    map.zw /= scaleLW;
    map.zw *= signLW;

    map.zw = clamp(map.zw,-1.0,1.0);
    return map;
}

float2 AH2pMap(float2 ah, out float2 valid, out float allValid)
{
    valid = step(float2(-1.0,minh),ah.xy) * step(ah.xy,float2(1.0,maxh));
    ah.xy = clamp(ah.xy,float2(-1.0,minh),float2(1.0,maxh));

    float horizonZenith_ABS = sqrt(ah.y * ah.y - minh * minh) / ah.y;
    ah.x += horizonZenith_ABS;
    ah.x /= 1.0 + horizonZenith_ABS;
    ah.x = sqrt(ah.x);


    float H = sqrt(maxh * maxh - minh * minh);
    float P = sqrt(ah.y * ah.y - minh * minh);

    ah.y = P / H;
    
    valid *= step(0.0,ah.xy) * step(ah.xy,1.0);
    ah.xy = saturate(ah.xy);

    allValid = valid.x * valid.y;
    return ah;
}

float2 AH2fMap(float2 ah, out float2 valid, out float allValid)
{
    valid = step(float2(-1.0,minh),ah.xy) * step(ah.xy,float2(1.0,maxh));
    ah.xy = clamp(ah.xy,float2(-1.0,minh),float2(1.0,maxh));

    float horizonZenith_ABS = sqrt(ah.y * ah.y - minh * minh) / ah.y;
    float upToHorizonZenithRange = 1.0 + horizonZenith_ABS;
    float horizonToMaxAndMinZenithRatio = sqrt((1.0 - horizonZenith_ABS)/upToHorizonZenithRange);

    ah.x += horizonZenith_ABS;
    ah.x /= upToHorizonZenithRange;
    ah.x = sign(ah.x) * sqrt(abs(ah.x));
    ah.x += horizonToMaxAndMinZenithRatio;
    ah.x /= 1.0 + horizonToMaxAndMinZenithRatio;


    float H = sqrt(maxh * maxh - minh * minh);
    float P = sqrt(ah.y * ah.y - minh * minh);

    ah.y = P / H;
    
    valid *= step(0.0,ah.xy) * step(ah.xy,1.0);
    ah.xy = saturate(ah.xy);

    allValid = valid.x * valid.y;
    return ah;
}

float2 AD2Map(float2 ad, out float2 valid, out float allValid)
{
    sunDistance = abs(sunDistance);
    sunRadius = abs(sunRadius);
    
    valid.y = step(maxh, ad.y);
    ad.y = max(ad.y,maxh);

    float maxRange = maxh + sunRadius;
    maxRange *= maxRange;
    maxRange = sqrt(sunDistance * sunDistance - maxRange);
    float cameraPlanetTangent = sqrt(ad.y * ad.y - maxh * maxh);
    maxRange += cameraPlanetTangent;
    float camera2Sun = sqrt(maxRange * maxRange + sunRadius * sunRadius);
    maxRange = (cameraPlanetTangent * maxRange - maxh * sunRadius) / (ad.y * camera2Sun);
    
    valid.x = step(maxRange, ad.y) * step(ad.y,1.0);
    ad.x = clamp(ad.x,maxRange,1.0);

    ad.x = (ad.x - 1.0) / (maxRange - 1.0);
    ad.x *= ad.x;
    // ad.x *= ad.x;

    ad.y = maxh / ad.y;


    valid *= step(0.0,ad.xy) * step(ad.xy,1.0);
    ad.xy = saturate(ad.xy);

    allValid = valid.x * valid.y;
    return ad;
}

float4 AHLW2Map(float4 ahlw, out float4 valid, out float allValid)
{
    float2 deltaLW = float2(deltaL, deltaW);
    deltaLW *= 4.0 * deltaLW;
    deltaLW = 1.0 / deltaLW;
    float2 startLW = sqrt(deltaLW);
    float2 scaleLW = 2.0 * startLW + 1.0;
    float2 lengthLW = float2(lengthL, lengthW);
    valid.zw = step(-1.0,ahlw.zw) * step(ahlw.zw,1.0);
    ahlw.zw = clamp(ahlw.zw,-1.0,1.0);

    ahlw.zw = (sqrt(abs(ahlw.zw) * scaleLW + deltaLW) - startLW) * sign(ahlw.zw);
    ahlw.zw += 1.0;
    ahlw.zw *= 0.5;
    ahlw.zw /= lengthLW;
    ahlw.zw -= (1.0 / lengthLW) - 1.0;

    valid.zw *= step(ahlw.zw,1.0);
    ahlw.zw = saturate(ahlw.zw);
    // float outFlag = 1.0 - step(1.0, ahlw.z);

    // ahlw.w /= PI;
    // ahlw.xy = AH2Map(ahlw.xy);

    ahlw.xy = AH2fMap(ahlw.xy, valid.xy , allValid);

    // ahlw.xy = saturate(ahlw.xy);

    // ahlw = ahlw.wxzy;
    valid = valid.xzwy;
    ahlw = ahlw.xzwy;


    allValid *= valid.y * valid.z;
    return ahlw;
}

float4 translucentFromLUT(float2 ah)
{
    float2 valid;
    float allValid;
    ah = AH2pMap(ah,valid,allValid);

    ah = (ah * (translucentLUT_TexelSize.zw - float2(1.0,1.0)) + float2(0.5,0.5)) * translucentLUT_TexelSize.xy;
    return saturate(tex2Dlod(translucentLUT,float4(ah.x,ah.y,0.0,0.0))) * allValid;
}

float4 inSunLightFromLUT(float2 ah)
{
    float cachedMaxh = maxh;
    maxh = minh + 2.0 * (maxh - minh);
    float2 valid;
    float allValid;
    ah = AH2fMap(ah,valid,allValid);
    maxh = cachedMaxh;

    ah = (ah * (inSunLightLUT_TexelSize.zw - float2(1.0,1.0)) + float2(0.5,0.5)) * inSunLightLUT_TexelSize.xy;
    return saturate(tex2Dlod(inSunLightLUT,float4(ah.x,ah.y,0.0,0.0))) * allValid;
}


float4 outSunLightFromLUT(float2 ad)
{
    float2 valid;
    float allValid;
    ad = AD2Map(ad,valid,allValid);

    ad = (ad * (outSunLightLUT_TexelSize.zw - float2(1.0,1.0)) + float2(0.5,0.5)) * outSunLightLUT_TexelSize.xy;
    return lerp(1.0, saturate(tex2Dlod(outSunLightLUT,float4(ad.x,ad.y,0.0,0.0))),(1-valid.x)*valid.y);
}

void scatterFromLUT(float4 ahlw, out float4 reayleigh, out float4 mie)
{
    float4 valid;
    float allValid;
    ahlw = AHLW2Map(ahlw,valid,allValid);
    ahlw *= (scatterLUTSize - float4(1.0,1.0,1.0,1.0));
    ahlw.xy += float2(0.5,0.5);
    float2 zwFloor = clamp(floor(ahlw.zw),float2(0.0,0.0),scatterLUTSize.zw - float2(1.0,1.0));
    float2 zwCeil = clamp(ceil(ahlw.zw),float2(0.0,0.0),scatterLUTSize.zw - float2(1.0,1.0));
    float2 from = (ahlw.xy / scatterLUTSize.xy + zwFloor) / scatterLUTSize.zw;
    float2 to = (ahlw.xy / scatterLUTSize.xy + zwCeil) / scatterLUTSize.zw;
    float2 wTo = saturate(ahlw.zw - zwFloor);
    float2 wFrom = float2(1.0,1.0) - wTo;

    reayleigh   =   (tex2Dlod(scatterLUT_Reayleigh,float4(from.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Reayleigh,float4(from.x,to.y,0.0,0.0)) * wTo.y) * wFrom.x +
                    (tex2Dlod(scatterLUT_Reayleigh,float4(to.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Reayleigh,float4(to.x,to.y,0.0,0.0)) * wTo.y) * wTo.x;
    mie         =   (tex2Dlod(scatterLUT_Mie,float4(from.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Mie,float4(from.x,to.y,0.0,0.0)) * wTo.y) * wFrom.x +
                    (tex2Dlod(scatterLUT_Mie,float4(to.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Mie,float4(to.x,to.y,0.0,0.0)) * wTo.y) * wTo.x;

    reayleigh *= valid.y;
    mie *= valid.y;
}

float2 getAHMappingCoord(inout float3 startPos, inout float3 viewDir)
{
    viewDir = normalize(viewDir);
    float h0 = length(cross(startPos,viewDir));
    float xs = dot(startPos,viewDir);
    float hs = length(startPos);
    if(h0 <= maxh)
    {
        float cTop = sqrt(maxh * maxh - h0 * h0);
        if(xs < -cTop)
        {
            startPos -= (xs+cTop) * viewDir;
            xs = -cTop;
            hs = maxh;
        }
    }
    return float2(
        clamp(dot(startPos/hs,viewDir),-1,1),
        hs
    );
}


float4 translucent(float4 densSum)
{
    densSum = max(float4(0.0,0.0,0.0,0.0),densSum);
    densSum = exp(-densSum);
    return densSum;
}

float IngOZoneDensity(float x, float h)
{
    // return 0.0;
    float result = 0.0;
    // h*=h;
    float ozMin = 0.15+minh;
    float ozMid = 0.25+minh;
    float ozMax = 0.35+minh;
    if (h < ozMax)
    {
        float qh = h*h;
        float xCur = abs(x);
        float proccessedX = 0.0;
        float proccessedH = h;
        float xMax = sqrt(ozMax*ozMax-qh);
        if (h < ozMid)
        {
            float xMid = sqrt(ozMid*ozMid-qh);
            if (h < ozMin)
            {
                float xMin = sqrt(ozMin*ozMin-qh);
                if (xCur > xMin)
                {
                    proccessedX = xMin;
                    proccessedH = ozMin;
                }
                else return 0.0;
            }
            if (xCur > proccessedX)
            {
                float nextX = min(xCur,xMid);
                float nextH = sqrt(nextX*nextX+qh);
                //f(x) = sgn(H-targetH)(5*qh*ln(targetH+x)+(5*targetH−10*H)*x) + x
                //fd(x) = (5*qh*ln(targetH+x)+(5*targetH−10*H)*x) + x
                //fd(x) = (5*qh*ln(targetH+x)+5*(targetH−2*H)*x) + x
                //fd(x) = 5*(qh*ln(targetH+x)+(targetH−2*H)*x) + x
                //fd(x) = 5*((targetH-2*H)*x+qh*ln(targetH+x)) + x
                // result += 5.0*((nextH - 2.0*H) * nextX + qh * log(nextH + nextX)) + nextX
                // - 5.0*((proccessedH - 2.0*H) * proccessedX + qh * log(proccessedH + proccessedX)) - proccessedX
                result += 5.0 * ((nextH - 2.0 * ozMid) * nextX - (proccessedH - 2.0 * ozMid) * proccessedX + qh * log((nextH + nextX) / (proccessedH + proccessedX))) + nextX - proccessedX;
                proccessedX = nextX;
                proccessedH = nextH;
            }
        }
        if (xCur > proccessedX)
        {
            float nextX = min(xCur,xMax);
            float nextH = sqrt(nextX*nextX+qh);
            //f(x) = sgn(H-targetH)(5*qh*ln(targetH+x)+(5*targetH−10*H)*x) + x
            //fu(x) = -(5*qh*ln(targetH+x)+(5*targetH−10*H)*x) + x
            //fu(x) = -5*qh*ln(targetH+x)-(5*targetH−10*H)*x + x
            //fu(x) = -5*qh*ln(targetH+x)+(10*H-5*targetH)*x + x
            //fu(x) = -5*qh*ln(targetH+x)+5*(2*H-targetH)*x + x
            //fu(x) = 5*(2*H-targetH)*x-5*qh*ln(targetH+x)+x
            //fu(x) = 5*((2*H-targetH)*x-qh*ln(targetH+x))+x
            // result += 5.0*((2.0 * H - nextH) * nextX - qh * log(nextH + nextX)) + nextX
            //         - 5.0*((2.0 * H - proccessedH) * proccessedX - qh * log(proccessedH + proccessedX)) - proccessedX;
            result += 5.0 * ((2.0 * ozMid - nextH) * nextX - (2.0 * ozMid - proccessedH) * proccessedX - qh * log((nextH + nextX) / (proccessedH + proccessedX))) + nextX - proccessedX;
            // proccessedX = nextX;
            // proccessedH = nextH;
        }
    }
    return result * sign(x);
}

void IngAirDensityFromTo(in float start,in float end,in float h, out float reayleigh, out float mie)
{
    reayleigh = 0.0;
    mie = 0.0;
    float d = (end-start)/float(ingCount);
    float prve_H = sqrt(start*start+h*h)-minh;
    float prve_reayleigh = exp(-prve_H/H_Reayleigh);
    float prve_mie = exp(-prve_H/H_Mie);
    for(int i = 1; i < ingCount; i++)
    {
        float pos = start+d*float(i);
        float current_H = sqrt(pos*pos+h*h)-minh;
        float current_reayleigh = exp(-current_H/H_Reayleigh);
        float current_mie = exp(-current_H/H_Mie);
        reayleigh += (prve_reayleigh + current_reayleigh) * d * 0.5;
        mie += (prve_mie + current_mie) * d * 0.5;
        prve_H = current_H;
        prve_reayleigh = current_reayleigh;
        prve_mie = current_mie;
    }
}

void IngAirDensity(in float viewZenith, in float height, out float reayleigh, out float mie, out float oZone)
{
    height = max(minh,height);
    float start = viewZenith*height;
    height = sqrt(max(height*height - start*start,0.0));
    float mh = sqrt(maxh*maxh - height*height) * step(height, maxh);
    start = clamp(start,-mh,mh);
    IngAirDensityFromTo(start,mh,height,reayleigh,mie);
    oZone = IngOZoneDensity(mh,height) - IngOZoneDensity(start,height);
}

float4 IngSunLight(float2 ad)
{
    float3 baseDir = float3(ad.x,0,0);
    baseDir.y = sqrt(1.0 - ad.x * ad.x);
    float3 d1 = abs(baseDir);
    d1 = float3(
        step(d1.x,d1.y) * step(d1.x,d1.z),
        step(d1.y,d1.x) * step(d1.y,d1.z),
        step(d1.z,d1.x) * step(d1.z,d1.y)
    );
    float4 result = 0.0;
    float3x3 mat = Crossfloat3x3_W2L(baseDir, d1);
    float sunPerspective = ad.y * baseDir.y;
    sunPerspective *= sunPerspective;
    sunPerspective = asin(saturate(sunRadius/(ad.y * baseDir.x + sqrt(sunPerspective + sunDistance * sunDistance))));
    for(int i = 0; i < ingLightCount; i++)
    {
        float ratioI = (float(i) + 0.5) / ingLightCount;
        float sizeFactor = (i + 1) * (i + 1) - i * i;
        float2 sumAng;
        sincos(ratioI * sunPerspective,sumAng.x,sumAng.y);
        for(int j = 0; j < ingLightCount; j++)
        {
            float ratioJ = 2.0 * UNITY_PI * (float(j) + 0.5) / ingLightCount;
            float3 sampleDir = mul(float3(sumAng.y, sumAng.x * cos(ratioJ), sumAng.x * sin(ratioJ)),mat);
            float3 samplePos = float3(-ad.y,0,0);
            float2 ah = getAHMappingCoord(samplePos,sampleDir);
            result += (ah.y > maxh ? 1.0 : translucentFromLUT(ah)) * sizeFactor;
        }
    }
    result /= ingLightCount * ingLightCount * (ingLightCount - 1.0);
    return result;
}

void GenScatterInfo(float viewZenith, float height, float lightZenith, float lightToViewXYDotProduct, out float4 reayleighScatter, out float4 mieScatter)
{
    reayleighScatter = float4(0.0,0.0,0.0,0.0);
    mieScatter = float4(0.0,0.0,0.0,0.0);

    height = max(minh,height);
    float x0 = viewZenith*height;
    float h0 = sqrt(max(height*height-x0*x0,0.0));
    float mh = 0.0;
    if(h0 < maxh)
    {
        mh = sqrt(maxh*maxh-h0*h0);
        x0 = sign(x0) * min(abs(x0),mh);
    }
    float ml = -mh;
    if(h0 < minh)
    {
        ml = sqrt(minh*minh-h0*h0);
        x0 = sign(x0) * max(abs(x0),ml);
        if(x0 <= -ml)
        {
            float mml = -ml;
            ml = -mh;
            mh = mml;
        }
    }
    x0 = max(ml,x0);
    x0 = min(mh,x0);

    
    float d = (mh-x0)/float(ingCount - 1);
    float reayleigh = 0.0;
    float mie = 0.0;
    float oZone = IngOZoneDensity(x0,h0);
    float prve_reayleigh = exp((minh - height)/H_Reayleigh);
    float prve_mie = exp((minh - height)/H_Mie);
    float prve_oZone = oZone;
    float3 viewDir = float3(h0/height,viewZenith,0.0);
    float3 lightDir = float3(sqrt(1.0 - lightZenith*lightZenith),lightZenith,0.0);
    lightDir.z = lightDir.x * sqrt(1.0 - lightToViewXYDotProduct*lightToViewXYDotProduct);
    lightDir.x *= lightToViewXYDotProduct;
    float3 sunPos = lightDir * sunDistance;
    lightDir = normalize(sunPos - float3(0,height,0));
    float4 prve_light = inSunLightFromLUT(float2(lightDir.y,height));
    // float prve_mie = prve_reayleigh * prve_reayleigh;
    // prve_mie *= prve_mie * prve_mie * prve_reayleigh;
    for(int i = 1; i < ingCount; i++)
    {
        // exp((h0-sqrt(x^2+h^2))/H) dx
        // -sqrt(x^2+h^2)*H/x d(exp((h0-sqrt(x^2+h^2))/H))
        float l = d * float(i);
        float3 current_postion = l * viewDir;
        current_postion.y += height;
        float current_H = length(current_postion);
        lightDir = normalize(sunPos - current_postion);
        float4 current_light = inSunLightFromLUT(float2(clamp(dot(current_postion/current_H,lightDir),-1.0,1.0),current_H));
        current_H -= minh;
        float current_reayleigh = exp(-current_H/H_Reayleigh);
        float current_mie = exp(-current_H/H_Mie);
        float current_oZone = IngOZoneDensity(x0+l,h0);
        // float current_mie = current_reayleigh * current_reayleigh;
        // current_mie *= current_mie * current_mie * current_reayleigh;
        float reayleighScatterAmount = (prve_reayleigh + current_reayleigh) * d * 0.5;
        float mieScatterAmount = (prve_mie + current_mie) * d * 0.5;


        float4 reayleighScatterLight = (prve_light + current_light) * 0.5 * reayleighScatterAmount * reayleigh_scatter;
        float4 mieScatterLight = (prve_light + current_light) * 0.5 * mieScatterAmount * mie_scatter;
        float4 trans = translucent((reayleigh_scatter + molecule_absorb) * reayleigh) * translucent((mie_scatter + mie_absorb) * mie) * translucent(OZone_absorb * (prve_oZone - oZone));
        reayleighScatter += reayleighScatterLight * trans;
        mieScatter += mieScatterLight * trans;


        reayleigh += reayleighScatterAmount;
        mie += mieScatterAmount;

        prve_reayleigh = current_reayleigh;
        prve_mie = current_mie;
        prve_oZone = current_oZone;
        prve_light = current_light;
    }
    reayleighScatter = max(reayleighScatter, float4(0.0,0.0,0.0,0.0));
    mieScatter = max(mieScatter, float4(0.0,0.0,0.0,0.0));
}




void GenScatterInfoDyn(float viewZenith, float height, float lightZenith, float lightToViewXYDotProduct, float depth, out float4 reayleighScatter, out float4 mieScatter, out float4 trans)
{
    reayleighScatter = float4(0.0,0.0,0.0,0.0);
    mieScatter = float4(0.0,0.0,0.0,0.0);

    float x0 = viewZenith*height;
    float h0 = sqrt(height*height-x0*x0);
    float d = depth/float(ingCountDyn - 1);
    float reayleigh = 0.0;
    float mie = 0.0;
    float oZone = IngOZoneDensity(x0,h0);
    float prve_reayleigh = exp((minh - height)/H_Reayleigh);
    float prve_mie = exp((minh - height)/H_Mie);
    float prve_oZone = oZone;
    float3 viewDir = float3(h0/height,viewZenith,0.0);
    float3 lightDir = float3(sqrt(1.0 - lightZenith*lightZenith),lightZenith,0.0);
    lightDir.z = lightDir.x * sqrt(1.0 - lightToViewXYDotProduct*lightToViewXYDotProduct);
    lightDir.x *= lightToViewXYDotProduct;
    float3 sunPos = lightDir * sunDistance;
    lightDir = normalize(sunPos - float3(0,height,0));
    float4 prve_light = inSunLightFromLUT(float2(lightDir.y,height));
    // float prve_mie = prve_reayleigh * prve_reayleigh;
    // prve_mie *= prve_mie * prve_mie * prve_reayleigh;
    for(int i = 1; i < ingCountDyn; i++)
    {
        // exp((h0-sqrt(x^2+h^2))/H) dx
        // -sqrt(x^2+h^2)*H/x d(exp((h0-sqrt(x^2+h^2))/H))
        float l = d * float(i);
        float3 current_postion = l * viewDir;
        current_postion.y += height;
        float current_H = length(current_postion);
        lightDir = normalize(sunPos - current_postion);
        float4 current_light = inSunLightFromLUT(float2(clamp(dot(current_postion/current_H,lightDir),-1.0,1.0),current_H));
        current_H -= minh;
        float current_reayleigh = exp(-current_H/H_Reayleigh);
        float current_mie = exp(-current_H/H_Mie);
        float current_oZone = IngOZoneDensity(x0+l,h0);
        // float current_mie = current_reayleigh * current_reayleigh;
        // current_mie *= current_mie * current_mie * current_reayleigh;
        float reayleighScatterAmount = (prve_reayleigh + current_reayleigh) * d * 0.5;
        float mieScatterAmount = (prve_mie + current_mie) * d * 0.5;


        float4 reayleighScatterLight = (prve_light + current_light) * 0.5 * reayleighScatterAmount * reayleigh_scatter;
        float4 mieScatterLight = (prve_light + current_light) * 0.5 * mieScatterAmount * mie_scatter;
        trans = translucent(reayleigh_scatter * reayleigh) * translucent((mie_scatter + mie_absorb) * mie) * translucent(OZone_absorb * (prve_oZone - oZone));
        reayleighScatter += reayleighScatterLight * trans;
        mieScatter += mieScatterLight * trans;


        reayleigh += reayleighScatterAmount;
        mie += mieScatterAmount;

        prve_reayleigh = current_reayleigh;
        prve_mie = current_mie;
        prve_oZone = current_oZone;
        prve_light = current_light;
    }
    reayleighScatter = max(reayleighScatter, float4(0.0,0.0,0.0,0.0));
    mieScatter = max(mieScatter, float4(0.0,0.0,0.0,0.0));
    trans = saturate(trans);
}

struct AtmospherePropInfo
{
    float4 ahlwS;
    float4 ahlwE;
    // float3 shadowReciverPos;
};

AtmospherePropInfo getAtmospherePropInfoByRelPos(inout float3 startPos, inout float3 endPos, inout float3 lightDir)
{
    float3 transedLightDir;
    float3x3 proj;
    AtmospherePropInfo result;
    lightDir = normalize(lightDir);
    float3 viewDir = normalize(endPos - startPos);
    float h0 = length(cross(startPos,viewDir));
    float xs = dot(startPos,viewDir);
    float xe = dot(endPos,viewDir);
    float hs = length(startPos);
    float he = length(endPos);
    // result.shadowReciverPos = endPos;
    if(h0 <= maxh)
    {
        float cTop = sqrt(maxh * maxh - h0 * h0);
        if(xs < -cTop && xe >= -cTop)
        {
            startPos -= (xs + cTop) * viewDir;
            // result.shadowReciverPos = startPos;
            xs = -cTop;
            hs = maxh;
        }
        if (h0 <= minh)
        {
            float cLow = sqrt(minh * minh - h0 * h0);
            if(xs < -cLow && xe >= -cLow)
            {
                endPos += (xe+cLow) * viewDir;
                xe = -cLow;
                he = minh;
            }
        }
        // if (hs < maxh)
        // {
        //     result.shadowReciverPos = startPos;
        //     if (h0 > minh || xs > 0) result.shadowReciverPos += (cTop - xs) * viewDir;
        // }
    }
    proj = Crossfloat3x3_W2L(startPos,viewDir);
    transedLightDir = normalize(mul(proj,lightDir));
    result.ahlwS = float4(
        clamp(dot(startPos/hs,viewDir),-1,1),
        hs,
        clamp(transedLightDir.x,-1,1),
        clamp(transedLightDir.y,-1,1)
    );
    proj = Crossfloat3x3_W2L(endPos,viewDir);
    transedLightDir = normalize(mul(proj,lightDir));
    result.ahlwE = float4(
        clamp(dot(endPos/he,viewDir),-1,1),
        max(he,minh),
        clamp(transedLightDir.x,-1,1),
        clamp(transedLightDir.y,-1,1)
    );
    return result;
}

float reayleighStrong(float cosw)
{
    return 0.05968310365946075091333141126469*(1.0+cosw*cosw);
}

float4 mieStrong(float cosw)
{
    float4 mie = mie_eccentricity * 2.0 - float4(1.0,1.0,1.0,1.0);
    float4 g = mie * mie;
    return 0.07957747154594766788444188168626*(float4(1.0,1.0,1.0,1.0)-g)*(1.0+cosw*cosw)/((float4(2.0,2.0,2.0,2.0)+g)*pow(float4(1.0,1.0,1.0,1.0)+g-2.0*mie*cosw,float4(1.5,1.5,1.5,1.5)));
}

float4 getLightTranslucent(float2 LHMappingCoord)
{
    // if(AHMappingCoord.y < minh) return 0.0;
    float2 sunRelPos = float2(LHMappingCoord.x*sunDistance-LHMappingCoord.y,sqrt(1.0-LHMappingCoord.x*LHMappingCoord.x)*sunDistance);
    sunRelPos = normalize(sunRelPos);
    LHMappingCoord.x = sunRelPos.x;
    float4 result = 0.0;
    float p = (LHMappingCoord.y - maxh) / (maxh - minh);
    p *= abs(p);
    p *= abs(p);
    result += saturate(inSunLightFromLUT(LHMappingCoord) * (1.0 - max(p,0.0)) * step(p,1.0));
    LHMappingCoord.x = -LHMappingCoord.x;
    result += saturate(outSunLightFromLUT(LHMappingCoord) * min(p,1.0) * step(0.0,p));
    return result;
}

float4 getGroundTranslucent(AtmospherePropInfo infos)
{
    if(infos.ahlwS.y < minh) return 0.0;
    if(infos.ahlwS.y > maxh) return 1.0;
    if(infos.ahlwE.y <= maxh)
    {
        float4 StartEndAH = infos.ahlwE.x > 0 ? float4(infos.ahlwS.x,infos.ahlwS.y,infos.ahlwE.x,infos.ahlwE.y): float4(-infos.ahlwE.x,infos.ahlwE.y,-infos.ahlwS.x,infos.ahlwS.y);
        return saturate(translucentFromLUT(StartEndAH.xy) / translucentFromLUT(StartEndAH.zw));
    }
    else
    {
        return saturate(translucentFromLUT(infos.ahlwS.xy));
    }
}

//blocked light
float4 getSkyScatter(
    AtmospherePropInfo infos,
    float4 translucentGround,
    out float4 reayleighScatter,
    out float4 mieScatter
)
{
    if(infos.ahlwS.y > maxh)
    {
        reayleighScatter = 0.0;
        mieScatter = 0.0;
        return 0.0;
    }
    scatterFromLUT(infos.ahlwS,reayleighScatter,mieScatter);
    if(infos.ahlwE.y <= maxh && infos.ahlwE.y > minh)
    {
        float4 reayleighScatterTest;
        float4 mieScatterTest;
        scatterFromLUT(infos.ahlwE,reayleighScatterTest,mieScatterTest);
        reayleighScatter = max(reayleighScatter - translucentGround * reayleighScatterTest, 0.0);
        mieScatter = max(mieScatter - translucentGround * mieScatterTest, 0.0);
    }
    float cosw = sqrt(1.0 - infos.ahlwS.x*infos.ahlwS.x)*sqrt(1.0 - infos.ahlwS.z*infos.ahlwS.z)*infos.ahlwS.w + infos.ahlwS.x*infos.ahlwS.z;
    reayleighScatter *= reayleighStrong(cosw);
    mieScatter *= mieStrong(cosw);
    return max(reayleighScatter + mieScatter,0.0);
}

