
sampler2D translucentLUT;
sampler2D scatterLUT_Reayleigh;
sampler2D scatterLUT_Mie;
float4 translucentLUT_TexelSize;
float4 scatterLUT_Size;
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
float exposure;
float deltaL;
float deltaW;
float lengthL;
float lengthW;
float sunPerspective;

#define ingCount 2048
#define ingCountDyn 32
#define ingLightCount 6

#define PI 3.1415926535897932384626433832795

float bright(float3 L)
{
    return L.x * 0.299 + L.y * 0.587 + L.z * 0.114;
}

float3 hdr(float3 L) 
{
    L *= exposure;
    // L = sqrt(L);
    // L = lerp(pow(L * 0.38317, 1.0 / 2.2) , float3(1.0,1.0,1.0) - exp(-L), step(1.413,L));
    // L = float3(1.0,1.0,1.0) - exp(-L);
    return L;
}


float3x3 Crossfloat3x3_W2L(float3 d1, float3 d2) //d1,d2 on x-y plane, x axis lock on d1
{
    d1 = normalize(d1);
    d2 = normalize(d2);
    float3 d3 = normalize(cross(d1,d2));
    d2 = normalize(cross(d3,d1));
    return float3x3(d1,d2,d3);
}

void getLightSampleDir(in float3 baseDir, out float3 sampleDir[ingLightCount])
{
    sunPerspective = saturate(sunPerspective);
    float sunPerspectiveSineValue = sqrt(1.0 - sunPerspective * sunPerspective);
    float3 d1 = abs(baseDir);
    d1 = float3(
        step(d1.x,d1.y) * step(d1.x,d1.z),
        step(d1.y,d1.x) * step(d1.y,d1.z),
        step(d1.z,d1.x) * step(d1.z,d1.y)
    );
    float3x3 mat = Crossfloat3x3_W2L(baseDir, d1);
    for(int i = 0; i < ingLightCount; i++)
    {
        float ratio = float(i) / float(ingLightCount);
        sampleDir[i] = normalize(mul(float3(sunPerspective, sunPerspectiveSineValue * cos(2.0 * PI * ratio), sunPerspectiveSineValue * sin(2.0 * PI * ratio)),mat));
    }
}


float2 Map2AH(float2 map)
{
    map = saturate(map);
    // map.y *= map.y;
    // map.y *= maxh - minh;
    // map.y += minh;
    
    float H = maxh * maxh - minh * minh;
    float P = map.y * map.y * H;
    map.y = sqrt(P + minh * minh);
    
    float horizonZenith_ABS = sqrt(map.y * map.y - minh * minh) / map.y;

    map.x *= map.x;
    map.x *= (1.0 + horizonZenith_ABS);
    map.x -= horizonZenith_ABS;

    map = clamp(map,float2(-horizonZenith_ABS,minh),float2(1.0,maxh));
    return map;
}


float4 Map2AHLW(float2 map)
{
    map = saturate(map);
    map *= scatterLUT_Size.xy*scatterLUT_Size.zw-float2(1.0,1.0);
    float4 result = map.xyxy;
    result.zw = floor(result.zw / scatterLUT_Size.xy);
    result.xy = (result.xy - result.zw * scatterLUT_Size.xy) / (scatterLUT_Size.xy - float2(1.0,1.0));
    result.zw = result.zw / (scatterLUT_Size.zw - float2(1.0,1.0));
    // result = result.ywzx;
    //xyzw=wxzy
    //yxwz=xwyz
    //ywzx=xyzw
    result = result.xwyz;

    result.xy = saturate(result.xy);
    // result.y *= result.y;
    // result.y *= maxh - minh;
    // result.y += minh;

    float H = maxh * maxh - minh * minh;
    float P = result.y * result.y * H;
    result.y = sqrt(P + minh * minh);
    
    float horizonZenith_ABS = sqrt(result.y * result.y - minh * minh) / result.y;
    float upToHorizonZenithRange = 1.0 + horizonZenith_ABS;
    float horizonToMaxAndMinZenithRatio = sqrt((1.0 - horizonZenith_ABS)/upToHorizonZenithRange);

    result.x *= 1.0 + horizonToMaxAndMinZenithRatio;
    result.x -= horizonToMaxAndMinZenithRatio;
    result.x *= abs(result.x);
    result.x *= upToHorizonZenithRange;
    result.x -= horizonZenith_ABS;

    result.xy = clamp(result.xy,float2(-1.0,minh),float2(1.0,maxh));
    // result.xy = Map2AH(result.xy);
    // result.w *= PI;
    float2 deltaAHLW = float2(deltaL, deltaW);
    result.zw *= float2(lengthL, lengthW);
    result.zw *= 2.0;
    result.zw -= 1.0;
    result.zw *= sign(deltaAHLW) * (1.0 - exp(-abs(deltaAHLW)));
    result.zw = -sign(result.zw)*log(max(0.0,1.0-abs(result.zw)));
    result.zw /= -deltaAHLW;
    result.zw = clamp(result.zw,-1.0,1.0);
    return result;
}


float4 translucentFromLUT(float2 ah)
{
    ah.xy = clamp(ah.xy,float2(-1.0,minh),float2(1.0,maxh));

    float horizonZenith_ABS = sqrt(ah.y * ah.y - minh * minh) / ah.y;
    float OutputFlag = step(-horizonZenith_ABS, ah.x);
    ah.x += horizonZenith_ABS;
    ah.x /= 1.0 + horizonZenith_ABS;
    ah.x = sqrt(ah.x);


    float H = sqrt(maxh * maxh - minh * minh);
    float P = sqrt(ah.y * ah.y - minh * minh);

    ah.y = P / H;
    
    ah.xy = saturate(ah.xy);

    ah = (ah * (translucentLUT_TexelSize.zw - float2(1.0,1.0)) + float2(0.5,0.5)) * translucentLUT_TexelSize.xy;
    return saturate(tex2Dlod(translucentLUT,float4(ah.x,ah.y,0.0,0.0))) * OutputFlag;
}

void scatterFromLUT(float4 ahlw, out float4 reayleigh, out float4 mie)
{
    float2 deltaDiv_PI = float2(deltaL, deltaW);
    ahlw.zw = clamp(ahlw.zw,-1.0,1.0);
    ahlw.zw *= -deltaDiv_PI;
    ahlw.zw = sign(ahlw.zw) * (1.0 - exp(-abs(ahlw.zw)));
    ahlw.zw /= sign(deltaDiv_PI) * (1.0 - exp(-abs(deltaDiv_PI)));
    ahlw.zw += 1.0;
    ahlw.zw /= 2.0;
    ahlw.zw /= float2(lengthL, lengthW);
    float outFlag = 1.0 - step(1.0, ahlw.z);

    // ahlw.w /= PI;
    // ahlw.xy = AH2Map(ahlw.xy);
    
    ahlw.xy = clamp(ahlw.xy,float2(-1.0,minh),float2(1.0,maxh));
    
    float horizonZenith_ABS = sqrt(ahlw.y * ahlw.y - minh * minh) / ahlw.y;
    float upToHorizonZenithRange = 1.0 + horizonZenith_ABS;
    float horizonToMaxAndMinZenithRatio = sqrt((1.0 - horizonZenith_ABS)/upToHorizonZenithRange);

    ahlw.x += horizonZenith_ABS;
    ahlw.x /= upToHorizonZenithRange;
    ahlw.x = sign(ahlw.x) * sqrt(abs(ahlw.x));
    ahlw.x += horizonToMaxAndMinZenithRatio;
    ahlw.x /= 1.0 + horizonToMaxAndMinZenithRatio;
    
    float H = sqrt(maxh * maxh - minh * minh);
    float P = sqrt(ahlw.y * ahlw.y - minh * minh);

    ahlw.y = P / H;

    // ahlw.xy = saturate(ahlw.xy);

    // ahlw = ahlw.wxzy;
    ahlw = ahlw.xzwy;

    ahlw = saturate(ahlw) * (scatterLUT_Size - float4(1.0,1.0,1.0,1.0));
    ahlw.xy += float2(0.5,0.5);
    float2 zwFloor = clamp(floor(ahlw.zw),float2(0.0,0.0),scatterLUT_Size.zw - float2(1.0,1.0));
    float2 zwCeil = clamp(ceil(ahlw.zw),float2(0.0,0.0),scatterLUT_Size.zw - float2(1.0,1.0));
    float2 from = (ahlw.xy / scatterLUT_Size.xy + zwFloor) / scatterLUT_Size.zw;
    float2 to = (ahlw.xy / scatterLUT_Size.xy + zwCeil) / scatterLUT_Size.zw;
    float2 wTo = saturate(ahlw.zw - zwFloor);
    float2 wFrom = float2(1.0,1.0) - wTo;

    reayleigh   =   (tex2Dlod(scatterLUT_Reayleigh,float4(from.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Reayleigh,float4(from.x,to.y,0.0,0.0)) * wTo.y) * wFrom.x +
                    (tex2Dlod(scatterLUT_Reayleigh,float4(to.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Reayleigh,float4(to.x,to.y,0.0,0.0)) * wTo.y) * wTo.x;
    mie         =   (tex2Dlod(scatterLUT_Mie,float4(from.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Mie,float4(from.x,to.y,0.0,0.0)) * wTo.y) * wFrom.x +
                    (tex2Dlod(scatterLUT_Mie,float4(to.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Mie,float4(to.x,to.y,0.0,0.0)) * wTo.y) * wTo.x;

    reayleigh *= outFlag;
    mie *= outFlag;
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
    float3 sunParallaxLightDir[ingLightCount];
    float4 prve_light = translucentFromLUT(float2(lightZenith,height));
    getLightSampleDir(lightDir, sunParallaxLightDir);
    for(int j = 0; j < ingLightCount; j++)
    {
        prve_light += translucentFromLUT(float2(sunParallaxLightDir[j].y,height));
    }
    prve_light /= float(ingLightCount + 1);
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
        float4 current_light = translucentFromLUT(float2(clamp(dot(current_postion/current_H,lightDir),-1.0,1.0),current_H));
        for(int j = 0; j < ingLightCount; j++)
        {
            current_light += translucentFromLUT(float2(clamp(dot(current_postion/current_H,sunParallaxLightDir[j]),-1.0,1.0),current_H));
        }
        current_light /= float(ingLightCount + 1);
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
    float4 prve_light = translucentFromLUT(float2(lightZenith,height));
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
        float4 current_light = translucentFromLUT(float2(clamp(dot(current_postion/current_H,lightDir),-1.0,1.0),current_H));
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
    if(h0 <= maxh)
    {
        float cTop = sqrt(maxh * maxh - h0 * h0);
        if(xs < -cTop && xe >= -cTop)
        {
            startPos -= (xs+cTop) * viewDir;
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
    }
    proj = Crossfloat3x3_W2L(startPos,viewDir);
    transedLightDir = normalize(mul(proj,lightDir));
    result.ahlwS = float4(
        clamp(dot(startPos/hs,viewDir),-1,1),
        hs,
        clamp(transedLightDir.x,-1,1),
        clamp(transedLightDir.y,-1,1)
    );
    transedLightDir = endPos;
    h0 = length(cross(endPos,lightDir));
    xs = dot(endPos,lightDir);
    hs = he;
    if(h0 <= maxh)
    {
        float cTop = sqrt(maxh * maxh - h0 * h0);
        if(xs < -cTop)
        {
            transedLightDir -= (xs+cTop) * lightDir;
            xs = -cTop;
            hs = maxh;
        }
    }
    result.ahlwE = -2;
    if(hs <= maxh)
    {
        proj = Crossfloat3x3_W2L(transedLightDir,viewDir);
        transedLightDir = normalize(mul(proj,lightDir));
        result.ahlwE.y = hs;
        result.ahlwE.z = clamp(transedLightDir.x,-1,1);
        if(hs == he)
        {
            result.ahlwE.x = clamp(dot(endPos/he,viewDir),-1,1);
            result.ahlwE.w = clamp(transedLightDir.y,-1,1);
        }
    }
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

float4 getLightTranslucent(AtmospherePropInfo infos)
{
    if(infos.ahlwS.y < minh) return 0.0;
    if(infos.ahlwE.y <= maxh && infos.ahlwE.z >= -1) return saturate(translucentFromLUT(infos.ahlwE.zy));
    return 1.0;
}

float4 getGroundTranslucent(AtmospherePropInfo infos)
{
    if(infos.ahlwS.y < minh) return 0.0;
    if(infos.ahlwS.y > maxh) return 1.0;
    if(infos.ahlwE.y <= maxh && infos.ahlwE.x >= -1)
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
    if(infos.ahlwE.y <= maxh && infos.ahlwE.y > minh && infos.ahlwE.x >= -1)
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

//blocked light
float4 LightScatter(
    AtmospherePropInfo infos,
    float4 lightColor,
    float4 surfaceColor,
    float4 surfaceLight,
    out float4 translucentLight,
    out float4 translucentGround,
    out float4 reayleighScatter,
    out float4 mieScatter
)
{
    translucentLight = getLightTranslucent(infos);
    translucentGround = getGroundTranslucent(infos);
    float4 result = getSkyScatter(infos,translucentGround,reayleighScatter,mieScatter) * lightColor;
    if(infos.ahlwS.y <= maxh && infos.ahlwE.y <= maxh && infos.ahlwE.x >= -1)
    {
        result += translucentGround * (surfaceColor * translucentLight * lightColor + surfaceLight);
    }
    return result;
}
