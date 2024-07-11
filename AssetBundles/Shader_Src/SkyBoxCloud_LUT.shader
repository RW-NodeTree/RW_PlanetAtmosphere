Shader "Skybox/SkyBoxCloud_LUT"
{
    Properties
    {
        exposure ("exposure", Range(0, 20)) = 4.0
        ground_refract ("ground refract", Range(0, 1)) = 1.0
        ground_light ("ground light", Range(0, 1)) = 0.0
        opacity ("opacity multiplier", Range(0, 1)) = 1.0
        mie_amount ("mie amount", Range(0, 10)) = 3.996
        mie_absorb ("mie absorb", Range(0, 10)) = 1.11
        deltaAHLW_L ("scatterLUT light curve max derivative(v)", Range(1.0,20.0)) = 8.0
        deltaAHLW_W ("scatterLUT light curve max derivative(p)", Range(1.0,20.0)) = 4.0
        lengthAHLW_L ("scatterLUT light curve max range(v)", Range(0.5,1.0)) = 1.0
        lengthAHLW_W ("scatterLUT light curve max range(p)", Range(0.5,1.0)) = 1.0
        minh ("planet ground radius", Float) = 63.71393
        maxh ("planet sky radius", float) = 64.71393
        SunColor ("SunColor", Color) = (1,1,1,0.24)
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,0.618)
        reayleighScatterFactor ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,11.69904)
        OZoneAbsorbFactor ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,6.4)
        scatterLUT_Size ("scatterLUT_Size", Vector) = (0,0,0,0)
        translucentLUT ("translucent LUT", 2D) = "white"{}
        scatterLUT ("scatter LUT", 2D) = "black"{}
        cloudTexture ("cloud texture", 2D) = "transparent"{}
    }
    
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "UnityShaderVariables.cginc"
    #include "./PBSky.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float3 position : TEXCOORD0;
        float2 uv : TEXCOORD1;
    };

    sampler2D cloudTexture;
    float4 SunColor;
    
    float ground_refract;
    float ground_light;
    float opacity;
    // sampler2D _CameraDepthTexture;
    // const float s = float(6.6315851227221438037423488874623);
    // #define ingCount 6
    // #define ingLightCount 8
    
    v2f vert (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.position =  mul(unity_ObjectToWorld, v.vertex).xyz;
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

    ENDCG
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Opaque"}
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

            fixed4 frag (v2f i) : SV_Target
            {
                // const float3 OZoneAbsorbFactor = float3(0.21195,0.20962,0.01686);
                // const float3 OZoneAbsorbFactor = float3(0.065,0.1881,0.0085);
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = normalize(i.position - _WorldSpaceCameraPos.xyz);
                float3 pos = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));

                float3 cloudPos = i.position - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));
                float2 uv = pos2UV(UnityWorldToObjectDir(cloudPos));
                
                
                float4 col = abs(tex2Dlod(cloudTexture,float4(uv.x,uv.y,0.0,0.0)));
                // col.x = 1.0;
                // col.y = 1.0;
                // col.z = 1.0;
                IngAirFogPropInfo infoCamera = getIngAirFogPropInfoByRelPos(pos,eye,sun,distance(cloudPos,pos));
                float4 trans;
                float4 scatter = max(LightScatter(infoCamera, SunColor, float4(col.xyz,0.0) * ground_refract, float4(col.xyz,0.0) * ground_light, trans),0.0);
                scatter = max(0.0,scatter);

                scatter.xz += float2(0.08,0.2) * scatter.w;
                // scatter.z += 0.2 * scatter.w;

                col.xyz = scatter.xyz;
                col.xyz = hdr(col.xyz);
                col.xyz = ACESTonemap(col.xyz);
                col.w *= opacity;
                // // float2 SCREEN = i.screen.xy/i.screen.z;
                // // return tex2D(_CameraDepthTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0);
                // color = abs(IngOZoneDensity(h0+1.0,pos.y * length(eye.xz)) - IngOZoneDensity(pos.y*eye.y,pos.y * length(eye.xz))) * (float3(1.0,1.0,1.0)-OZoneAbsorbFactor);

                return col;
                // // return fixed4(SCREEN.x,SCREEN.y,0.0,1.0);

            }

            ENDCG
        }
        Pass
        {
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            fixed4 frag (v2f i) : SV_Target
            {
                // const float3 OZoneAbsorbFactor = float3(0.21195,0.20962,0.01686);
                // const float3 OZoneAbsorbFactor = float3(0.065,0.1881,0.0085);
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = normalize(i.position - _WorldSpaceCameraPos.xyz);
                float3 pos = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));

                float3 cloudPos = i.position - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));
                float2 uv = pos2UV(UnityWorldToObjectDir(cloudPos));
                
                
                float4 col = abs(tex2Dlod(cloudTexture,float4(uv.x,uv.y,0.0,0.0)));
                // col.x = 1.0;
                // col.y = 1.0;
                // col.z = 1.0;
                IngAirFogPropInfo infoCamera = getIngAirFogPropInfoByRelPos(pos,eye,sun,distance(cloudPos,pos));
                float4 trans;
                float4 scatter = max(LightScatter(infoCamera, SunColor, float4(col.xyz,0.0) * ground_refract, float4(col.xyz,0.0) * ground_light, trans),0.0);
                scatter = max(0.0,scatter);

                scatter.xz += float2(0.08,0.2) * scatter.w;
                // scatter.z += 0.2 * scatter.w;

                col.xyz = scatter.xyz;
                col.xyz = hdr(col.xyz);
                col.xyz = ACESTonemap(col.xyz);
                col.w *= opacity;
                // // float2 SCREEN = i.screen.xy/i.screen.z;
                // // return tex2D(_CameraDepthTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0);
                // color = abs(IngOZoneDensity(h0+1.0,pos.y * length(eye.xz)) - IngOZoneDensity(pos.y*eye.y,pos.y * length(eye.xz))) * (float3(1.0,1.0,1.0)-OZoneAbsorbFactor);

                return col;
                // // return fixed4(SCREEN.x,SCREEN.y,0.0,1.0);

            }

            ENDCG
        }
    }
}
