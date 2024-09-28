Shader "Planet/SkyBoxCloudLUT"
{
    Properties
    {
        playRange ("play speed", Range(0, 1)) = 0.015625
        ground_refract ("ground refract", Range(0, 1)) = 1.0
        ground_light ("ground light", Range(0, 1)) = 0.0
        opacity ("opacity multiplier", Range(0, 1)) = 1.0
        deltaL ("scatterLUT light curve max derivative(v)", Range(1.0,20.0)) = 8.0
        deltaW ("scatterLUT light curve max derivative(p)", Range(1.0,20.0)) = 4.0
        lengthL ("scatterLUT light curve max range(v)", Range(0.5,1.0)) = 1.0
        lengthW ("scatterLUT light curve max range(p)", Range(0.5,1.0)) = 1.0
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
        exposure ("exposure", Float) = 16.0
        flowDir ("flow dir", Float) = 0.0
        minh ("planet ground radius", Float) = 63.71393
        maxh ("planet sky radius", Float) = 64.71393
        sunColor ("Sun Color", Color) = (1,1,1,0.24)
        reayleigh_scatter ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,11.69904)
        molecule_absorb ("Molecule Absorb Factor", Vector) = (0,0,0,0)
        OZone_absorb ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,6.4)
        mie_scatter ("Mie scatter Factor", Vector) = (3.996,3.996,3.996,3.996)
        mie_absorb ("Mie absorb Factor", Vector) = (4.44,4.44,4.44,4.44)
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,0.618)
        scatterLUT_Size ("scatterLUT Size", Vector) = (0,0,0,0)
        translucentLUT ("translucent LUT", 2D) = "white"{}
        outSunLightLUT ("out sun light LUT", 2D) = "white"{}
        inSunLightLUT ("in sun light LUT", 2D) = "white"{}
        scatterLUT_Reayleigh ("scatter LUT Reayleigh", 2D) = "black"{}
        scatterLUT_Mie ("scatter LUT Mie", 2D) = "black"{}
        cloudTexture ("cloud texture", 2D) = "transparent"{}
        noiseTexture ("noise texture", 2D) = "transparent"{}
    }

    
    CGINCLUDE

    #pragma multi_compile_fwdbase

    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "AutoLight.cginc"
    #include "../PBSky.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 pos : SV_POSITION;
        float3 world : TEXCOORD0;
        float3 obj : TEXCOORD1;
        float2 uv : TEXCOORD2;
    };

    sampler2D cloudTexture;
    float4 cloudTexture_TexelSize;
    sampler2D noiseTexture;
    float4 sunColor;
    
    float ground_refract;
    float ground_light;
    float opacity;
    float playRange;
    float flowDir;
    // sampler2D _CameraDepthTexture;
    // const float s = float(6.6315851227221438037423488874623);
    // #define ingCount 6
    // #define ingLightCount 8
    
    v2f vert (appdata v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.world =  mul(unity_ObjectToWorld, v.vertex).xyz;
        o.obj = v.vertex;
        o.uv = v.uv;
        return o;
    }

    
    float2 pos2UV(float3 pos)
    {
        pos = normalize(pos);
        float2 uv = float2(atan2(pos.z, pos.x), acos(clamp(-pos.y,-1.0,1.0)));
        uv /= float2(PI * 2.0, PI);
        // uv = clamp(uv,0.0,1.0);
        return uv;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        // const float3 OZone_absorb = float3(0.21195,0.20962,0.01686);
        // const float3 OZone_absorb = float3(0.065,0.1881,0.0085);
        float3 sun = normalize(_WorldSpaceLightPos0.xyz);
        // float3 eye = normalize(i.world - _WorldSpaceCameraPos.xyz);
        float3 pos = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));

        float3 cloudPos = i.world - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));
        float2 n_flowDir = float2(cos(flowDir * PI / 180.0),sin(flowDir * PI / 180.0));
        float2 uv = pos2UV(i.obj);
        
        float4 col = abs(tex2Dlod(cloudTexture,float4(uv.x,uv.y,0.0,0.0)));
        float4 noiseA = abs(tex2Dlod(noiseTexture,float4(uv.xy + 0.5 * playRange * _Time.x * n_flowDir,0.0,0.0)));
        float4 noiseB = abs(tex2Dlod(noiseTexture,float4(uv.xy + playRange * _Time.x * n_flowDir,0.0,0.0)));
        float playRange_pixel = ceil(playRange * cloudTexture_TexelSize.z);
        float disCloud = 0;
        float end = exp(-playRange);
        for(int i = 1; i <= playRange_pixel; i++)
        {
            disCloud = max(disCloud,abs(tex2Dlod(cloudTexture,float4(uv.xy + float(i) * cloudTexture_TexelSize.x * n_flowDir,0.0,0.0)).w) * (exp(- float(i) * cloudTexture_TexelSize.x) - end) / (1.0 - end));
        }
        // float4 noise = abs(tex2Dlod(noiseTexture,float4(uv,0.0,0.0)));
        // float disCloud = 0;
        // for(int i = 1; i <= playSpeed; i++)
        // {
        //     disCloud += abs(tex2Dlod(cloudTexture,float4(uv + float(i) * cloudTexture_TexelSize.xy * (2.0 * noise.xy - 1.0),0.0,0.0)).w) * 0.5 * exp(- i / playSpeed) / playSpeed;
        // }
        // noise = abs(tex2Dlod(noiseTexture,float4(uv + float(i) * playSpeed * _Time.x * cloudTexture_TexelSize.xy * (2.0 * noise.xy - 1.0),0.0,0.0)));
        // disCloud = clamp(disCloud, 0.0, 1.0);
        col.w = clamp(col.w, disCloud * noiseA.x * noiseB.x, 1.0);
        col.xyz = col.www;
        // col.xyz = 1.0;
        // col.xyz = dot(sun,normalize(cloudPos));
        // col.xyz = (dot(sun,normalize(cloudPos)) + minh/maxh) / (1.0 + minh/maxh);
        // col.xyz = sqrt(col.xyz);
        // col.xyz *= col.xyz;
        float3 cachedCloudPos = cloudPos;
        AtmospherePropInfo infos = getAtmospherePropInfoByRelPos(pos,cloudPos,sun);
        float4 translucentLight = getLightTranslucent(infos.ahlwE.zy);
        float4 translucentGround = getGroundTranslucent(infos);
        
        float4 reayleighScatter;
        float4 mieScatter;
        float4 scatter = getSkyScatter(infos,translucentGround,reayleighScatter,mieScatter);
        


        scatter = max(scatter, 0.0);
        scatter.xz += float2(0.08,0.2) * scatter.w;
        scatter.xyz = hdr(scatter.xyz);
        col.xyz *= translucentGround * (translucentLight * ground_refract * saturate(hdr(dot(normalize(cloudPos),sun))) + ground_light);
        col.xyz += scatter;

        // col.xyz = ACESTonemap(col.xyz);
        col.w *= opacity;
        // // float2 SCREEN = i.screen.xy/i.screen.z;
        // // return tex2D(_CameraDepthTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0);
        // color = abs(IngOZoneDensity(h0+1.0,pos.y * length(eye.xz)) - IngOZoneDensity(pos.y*eye.y,pos.y * length(eye.xz))) * (float3(1.0,1.0,1.0)-OZone_absorb);

        return col;
        // // return fixed4(SCREEN.x,SCREEN.y,0.0,1.0);

    }
    ENDCG
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "LightMode"="ForwardBase"}
        Blend SrcAlpha OneMinusSrcAlpha
        // Blend SrcAlpha One
        ZWrite Off
        LOD 0
        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            ENDCG
        }
        Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            ENDCG
        }
    }
}
