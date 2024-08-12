
sampler2D translucentLUT;
sampler2D scatterLUT_Reayleigh;
sampler2D scatterLUT_Mie;
float4 translucentLUT_TexelSize;
float4 scatterLUT_Size;
float4 mie_eccentricity;
float4 reayleigh_scatter;
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
float deltaAHLW_L;
float deltaAHLW_W;
float lengthAHLW_L;
float lengthAHLW_W;

#define ingCount 2048
// #define ingLightCount 8

#define PI 3.1415926535897932384626433832795

float bright(float3 L)
{
    return L.x * 0.299 + L.y * 0.587 + L.z * 0.114;
}

float3 hdr(float3 L) 
{
    L *= exposure;
    // L = lerp(pow(L * 0.38317, 1.0 / 2.2) , float3(1.0,1.0,1.0) - exp(-L), step(1.413,L));
    L = float3(1.0,1.0,1.0) - exp(-L);
    return L;
}

static const float a = 2.51;  // Default: 2.51f
static const float b = 0.03;  // Default: 0.03f
static const float c = 2.43;  // Default: 2.43f
static const float d = 0.59;  // Default: 0.59f
static const float e = 0.14;  // Default: 0.14f
static const float p = 1.3;
static const float overlap = 0.2;

static const float rgOverlap = 0.1 * overlap;
static const float rbOverlap = 0.01 * overlap;
static const float gbOverlap = 0.04 * overlap;

static const float3x3 coneOverlap = float3x3(
                                                1.0,        rgOverlap,  rbOverlap,
                                                rgOverlap,  1.0, 		gbOverlap,
                                                rbOverlap, 	rgOverlap, 	1.0
                                            );

static const float3x3 coneOverlapInverse = float3x3(
                                                        1.0 + (rgOverlap + rbOverlap),  -rgOverlap, 	                -rbOverlap,
                                                        -rgOverlap, 		            1.0 + (rgOverlap + gbOverlap),  -gbOverlap,
                                                        -rbOverlap, 		            -rgOverlap, 	                1.0 + (rbOverlap + rgOverlap)
                                                    );

float3 ACESTonemap(float3 color){
    color = mul(coneOverlap,color);
    color = pow(color, float3(p,p,p));
    color = (color * (a * color + b)) / (color * (c * color + d) + e);
    color = pow(color, float3(1.0,1.0,1.0)/p);
    color = mul(coneOverlapInverse,color);
    color = clamp(color,float3(0.0,0.0,0.0),float3(1.0,1.0,1.0));
    return color;
}

float2 Map2AH(float2 map)
{
    map = clamp(map,float2(0.0,0.0),float2(1.0,1.0));
    map.y *= map.y;
    map.y *= maxh - minh;
    map.y += minh;
    float angRange = PI - asin(clamp(minh/map.y,-1.0,1.0)); //p
    

    // float ang = 2.0*map.x-1.0;
    // map.x = 0.5*PI*(1.0+sign(ang)*ang*ang);
    map.x *= map.x;
    map.x = 1.0 - map.x;
    map.x *= angRange;
    map = clamp(map,float2(0.0,minh),float2(angRange,maxh));
    return map;
}


float4 Map2AHLW(float2 map)
{
    map = clamp(map,0.0,1.0);
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

    result.xy = clamp(result.xy,float2(0.0,0.0),float2(1.0,1.0));
    result.y *= result.y;
    result.y *= maxh - minh;
    result.y += minh;
    
    float horizonAngA = asin(clamp(minh/result.y,-1.0,1.0)); //p
    float horizonAngB = PI - horizonAngA; //p
    float ang = result.x*(1.0+sqrt(horizonAngA/horizonAngB))-1.0;
    result.x = horizonAngB*(1.0+sign(ang)*ang*ang);

    // float ang = 2.0*map.x-1.0;
    // map.x = 0.5*PI*(1.0+sign(ang)*ang*ang);
    result.xy = clamp(result.xy,float2(0.0,minh),float2(PI,maxh));
    // result.xy = Map2AH(result.xy);
    // result.w *= PI;
    float2 deltaAHLW_Div_PI = 2.0 * float2(deltaAHLW_L, deltaAHLW_W) / PI;
    result.zw *= float2(lengthAHLW_L, lengthAHLW_W);
    result.zw *= 2.0;
    result.zw -= 1.0;
    result.zw *= sign(deltaAHLW_Div_PI) * (1.0 - exp(-abs(deltaAHLW_Div_PI)));
    result.zw = -sign(result.zw)*log(max(0.0,1.0-abs(result.zw)));
    result.zw /= -deltaAHLW_Div_PI;
    result.zw = acos(clamp(result.zw,-1.0,1.0));
    return result;
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

void IngAirDensityFromTo(in float x0,in float end,in float h, out float reayleigh, out float mie)
{
    reayleigh = 0.0;
    mie = 0.0;
    float d = (end-x0)/float(ingCount);
    float prve_H = sqrt(x0*x0+h*h)-minh;
    float prve_reayleigh = exp(-prve_H/H_Reayleigh);
    float prve_mie = exp(-prve_H/H_Mie);
    for(int i = 1; i < ingCount; i++)
    {
        float pos = x0+d*float(i);
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

void IngAirDensity(in float x0, in float h, out float reayleigh, out float mie, out float oZone)
{
    float mh = 0.0;
    if(h < maxh)
    {
        mh = sqrt(maxh*maxh-h*h);
    }
    float ml = -mh;

    x0 = max(ml,x0);
    x0 = min(mh,x0);
    IngAirDensityFromTo(x0,mh,h,reayleigh,mie);
    oZone = IngOZoneDensity(mh,h) - IngOZoneDensity(x0,h);
    
}

float4 translucentFromLUT(float2 ah)
{
    float angRange = PI - asin(clamp(minh/ah.y,-1.0,1.0)); //p
    ah.x /= angRange;
    ah.x = 1.0 - ah.x;
    ah.x = min(ah.x,1.0);
    if(ah.x < 0.0) return 0.0;
    ah.x = sqrt(ah.x);

    // float ang = 2.0 * ah.x / PI - 1.0;
    // ah.x = (1.0 + sign(ang) * sqrt(abs(ang)))/2.0;


    ah.y -= minh;
    ah.y /= maxh - minh;
    ah.y = min(ah.y,1.0);
    if(ah.y < 0.0) return 0.0;
    ah.y = sqrt(ah.y);

    ah = (ah * (translucentLUT_TexelSize.zw - float2(1.0,1.0)) + float2(0.5,0.5)) * translucentLUT_TexelSize.xy;
    return clamp(tex2Dlod(translucentLUT,float4(ah.x,ah.y,0.0,0.0)), 0.0, 1.0);
}

void scatterFromLUT(float4 ahlw, out float4 reayleigh, out float4 mie)
{
    float2 deltaAHLW_Div_PI = 2.0 * float2(deltaAHLW_L, deltaAHLW_W) / PI;
    ahlw.zw = cos(ahlw.zw);
    ahlw.zw *= -deltaAHLW_Div_PI;
    ahlw.zw = sign(ahlw.zw) * (1.0 - exp(-abs(ahlw.zw)));
    ahlw.zw /= sign(deltaAHLW_Div_PI) * (1.0 - exp(-abs(deltaAHLW_Div_PI)));
    ahlw.zw += 1.0;
    ahlw.zw /= 2.0;
    ahlw.zw /= float2(lengthAHLW_L, lengthAHLW_W);
    if(ahlw.z > 1.0)
    {
        reayleigh = 0.0;
        mie = 0.0;
        return;
    }
    // ahlw.w /= PI;
    // ahlw.xy = AH2Map(ahlw.xy);
    
    ahlw.xy = clamp(ahlw.xy,float2(0.0,minh),float2(PI,maxh));

    float horizonAngA = asin(clamp(minh/ahlw.y,-1.0,1.0)); //p
    float horizonAngB = PI - horizonAngA; //p
    float ang = ahlw.x / horizonAngB - 1.0;
    ahlw.x = (1.0 + sign(ang) * sqrt(abs(ang)))/(1.0 + sqrt(horizonAngA/horizonAngB));


    ahlw.y -= minh;
    ahlw.y /= maxh - minh;
    ahlw.y = sqrt(ahlw.y);
    ahlw.xy = clamp(ahlw.xy,float2(0.0,0.0),float2(1.0,1.0));

    // ahlw = ahlw.wxzy;
    ahlw = ahlw.xzwy;

    ahlw = clamp(ahlw,0.0,1.0) * (scatterLUT_Size - float4(1.0,1.0,1.0,1.0));
    ahlw.xy += float2(0.5,0.5);
    float2 zwFloor = clamp(floor(ahlw.zw),float2(0.0,0.0),scatterLUT_Size.zw - float2(1.0,1.0));
    float2 zwCeil = clamp(ceil(ahlw.zw),float2(0.0,0.0),scatterLUT_Size.zw - float2(1.0,1.0));
    float2 from = (ahlw.xy / scatterLUT_Size.xy + zwFloor) / scatterLUT_Size.zw;
    float2 to = (ahlw.xy / scatterLUT_Size.xy + zwCeil) / scatterLUT_Size.zw;
    float2 wTo = clamp(ahlw.zw - zwFloor,0.0,1.0);
    float2 wFrom = float2(1.0,1.0) - wTo;

    reayleigh   =   (tex2Dlod(scatterLUT_Reayleigh,float4(from.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Reayleigh,float4(from.x,to.y,0.0,0.0)) * wTo.y) * wFrom.x +
                    (tex2Dlod(scatterLUT_Reayleigh,float4(to.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Reayleigh,float4(to.x,to.y,0.0,0.0)) * wTo.y) * wTo.x;
    mie         =   (tex2Dlod(scatterLUT_Reayleigh,float4(from.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Reayleigh,float4(from.x,to.y,0.0,0.0)) * wTo.y) * wFrom.x +
                    (tex2Dlod(scatterLUT_Reayleigh,float4(to.x,from.y,0.0,0.0)) * wFrom.y + tex2Dlod(scatterLUT_Reayleigh,float4(to.x,to.y,0.0,0.0)) * wTo.y) * wTo.x;
}

void GenScatterInfo(float viewAng, float height, float lightAng, float lightToViewAng, out float4 reayleighScatter, out float4 mieScatter)
{
    reayleighScatter = float4(0.0,0.0,0.0,0.0);
    mieScatter = float4(0.0,0.0,0.0,0.0);

    float x0 = cos(viewAng)*height;
    float h0 = sin(viewAng)*height;
    float mh = 0.0;
    if(h0 < maxh)
    {
        mh = sqrt(maxh*maxh-h0*h0);
    }
    float minhOffseted = max(minh - 0.0001, 0.0);
    float ml = -mh;
    if(h0 < minhOffseted)
    {
        ml = sqrt(minhOffseted*minhOffseted-h0*h0);
    }
    if(x0 >= ml || x0 <= -ml)
    {
        if(x0 < -ml)
        {
            float mml = -ml;
            ml = -mh;
            mh = mml;
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
        float3 viewDir = float3(sin(viewAng),cos(viewAng),0.0);
        float3 lightDir = float3(sin(lightAng)*cos(lightToViewAng),cos(lightAng),sin(lightAng)*sin(lightToViewAng));
        float4 prve_light = translucentFromLUT(float2(lightAng,height));
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
            float4 current_light = translucentFromLUT(float2(acos(clamp(dot(current_postion/current_H,lightDir),-1.0,1.0)),current_H));
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
            float4 trans = translucent(reayleigh_scatter * reayleigh) * translucent((mie_scatter + mie_absorb) * mie) * translucent(OZone_absorb * (prve_oZone - oZone));
            reayleighScatter += reayleighScatterLight * trans;
            mieScatter += mieScatterLight * trans;


            reayleigh += reayleighScatterAmount;
            mie += mieScatterAmount;

            prve_reayleigh = current_reayleigh;
            prve_mie = current_mie;
            prve_oZone = current_oZone;
            prve_light = current_light;
        }
    }
    reayleighScatter = max(reayleighScatter, float4(0.0,0.0,0.0,0.0));
    mieScatter = max(mieScatter, float4(0.0,0.0,0.0,0.0));
}


struct IngAirFogPropInfo
{
    float h;
    float depth;
    float3 viewDir;
    float3 lightDir;
};

float3x3 Crossfloat3x3_W2L(float3 d1, float3 d2) //d1,d2 on x-y plane, x axis lock on d1
{
    d1 = normalize(d1);
    d2 = normalize(d2);
    float3 d3 = normalize(cross(d1,d2));
    d2 = normalize(cross(d3,d1));
    return float3x3(d1,d2,d3);
}

IngAirFogPropInfo getIngAirFogPropInfoByRelPos(float3 relPos, float3 viewDir, float3 lightDir, float depth)
{
    float3 d1 = abs(relPos);
    if (d1.y < d1.x)
    {
        if (d1.z < d1.y) d1 = float3(0.0,0.0,1.0);
        else d1 = float3(0.0,1.0,0.0);
    }
    else
    {
        if (d1.z < d1.x) d1 = float3(0.0,0.0,1.0);
        else d1 = float3(1.0,0.0,0.0);
    }
    float3x3 proj = Crossfloat3x3_W2L(relPos,d1);
    IngAirFogPropInfo result;
    result.h = length(relPos);
    result.depth = depth;
    result.viewDir = normalize(mul(proj,viewDir)).yxz;
    result.lightDir = normalize(mul(proj,lightDir)).yxz;
    
    float h0 = result.h * length(result.viewDir.xz);
    float x0 = result.h * result.viewDir.y;
    float mh = sqrt(maxh*maxh-h0*h0)*step(h0,maxh);
    if(x0 < -mh && x0 + result.depth >= -mh)
    {
        relPos = float3(0.0,result.h,0.0) - (x0 + mh) * result.viewDir;
        d1 = abs(relPos);
        if (d1.y < d1.x)
        {
            if (d1.z < d1.y) d1 = float3(0.0,0.0,1.0);
            else d1 = float3(0.0,1.0,0.0);
        }
        else
        {
            if (d1.z < d1.x) d1 = float3(0.0,0.0,1.0);
            else d1 = float3(1.0,0.0,0.0);
        }
        proj = Crossfloat3x3_W2L(relPos,d1);
        result.h = maxh;
        result.depth += mh + x0;
        result.viewDir = normalize(mul(proj,result.viewDir)).yxz;
        result.lightDir = normalize(mul(proj,result.lightDir)).yxz;
        x0 = -mh;
        depth = result.depth;
    }
    float ml = sqrt(minh*minh-h0*h0)*step(h0,minh);
    if(x0 < -ml) result.depth = min(result.depth,-ml-x0);
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

void getTranslucents(IngAirFogPropInfo infos, out float4 translucentLight, out float4 translucentGround)
{
    float3 p = infos.depth * infos.viewDir + float3(0.0,infos.h,0.0);
    float viewAngS = acos(clamp(infos.viewDir.y,-1.0,1.0));
    float h = length(p);
    translucentLight = step(minh, infos.h);
    translucentGround = translucentLight * translucentFromLUT(float2(viewAngS,infos.h));
    p /= h;
    h = max(minh,h);
    if(h < maxh)
    {
        float lightAng = acos(clamp(dot(infos.lightDir,p),-1.0,1.0));
        float viewAngG = acos(clamp(dot(infos.viewDir,p),-1.0,1.0));
        if(2.0 * viewAngG < PI) translucentGround = clamp(translucentGround / translucentFromLUT(float2(viewAngG,h)),0.0,1.0);
        else translucentGround = clamp(translucentFromLUT(float2(PI - viewAngG,h)) / translucentFromLUT(float2(PI - viewAngS,infos.h)),0.0,1.0);
        translucentGround *= translucentLight;
        translucentLight *= translucentFromLUT(float2(lightAng,h));
    }
}

//blocked light
float4 LightScatter(IngAirFogPropInfo infos, float4 lightColor, float4 surfaceColor, float4 surfaceLight, out float4 translucentLight, out float4 translucentGround, out float4 reayleighScatter, out float4 mieScatter)
{
    float4 result = 0.0;
    float viewAngS = acos(clamp(infos.viewDir.y,-1.0,1.0));
    float a = acos(clamp(infos.viewDir.y,-1.0,1.0));
    float l = acos(clamp(infos.lightDir.y,-1.0,1.0));
    float w = acos(clamp(dot(normalize(infos.viewDir.xz),normalize(infos.lightDir.xz)),-1.0,1.0));
    translucentLight = step(minh, infos.h);
    translucentGround = translucentLight * translucentFromLUT(float2(viewAngS,infos.h));
    scatterFromLUT(float4(a,infos.h,l,w),reayleighScatter,mieScatter);
    reayleighScatter = max(reayleighScatter * translucentLight,0.0);
    mieScatter = max(mieScatter * translucentLight,0.0);
    float3 p = infos.depth * infos.viewDir + float3(0.0,infos.h,0.0);
    float h = length(p);
    p /= h;
    h = max(minh,h);
    if(h < maxh)
    {
        float lightAng = acos(clamp(dot(infos.lightDir,p),-1.0,1.0));
        float viewAngG = acos(clamp(dot(infos.viewDir,p),-1.0,1.0));
        if(2.0 * viewAngG < PI) translucentGround = clamp(translucentGround / translucentFromLUT(float2(viewAngG,h)),0.0,1.0);
        else translucentGround = clamp(translucentFromLUT(float2(PI - viewAngG,h)) / translucentFromLUT(float2(PI - viewAngS,infos.h)),0.0,1.0);
        translucentGround *= translucentLight;
        translucentLight *= translucentFromLUT(float2(lightAng,h));
        result = (surfaceColor * translucentLight * lightColor + surfaceLight) * translucentGround;
        result = max(result,0.0);

        a = acos(clamp(dot(infos.viewDir,p),-1.0,1.0));
        l = acos(clamp(dot(infos.lightDir,p),-1.0,1.0));
        float4 reayleighScatter_out;
        float4 mieScatter_out;
        scatterFromLUT(float4(a,h,l,w),reayleighScatter_out,mieScatter_out);
        reayleighScatter -= max(reayleighScatter_out * translucentGround,0.0);
        mieScatter -= max(mieScatter_out * translucentGround,0.0);
    }
    // reayleighScatter = scatter * reayleigh_scatter / (reayleigh_scatter + mie_scatter);
    // mieScatter = scatter * mie_scatter / (reayleigh_scatter + mie_scatter);
    
    float cosw = dot(infos.viewDir,infos.lightDir);
    reayleighScatter *= reayleighStrong(cosw);
    mieScatter *= mieStrong(cosw);
    result += (reayleighScatter + mieScatter) * lightColor;
    return max(result,0.0);
}
