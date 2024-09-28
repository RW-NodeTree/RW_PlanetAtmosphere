Shader "Planet/PlanetLight"
{
    Properties
    {
        deltaL ("scatterLUT light curve max derivative(v)", Range(1.0,20.0)) = 8.0
        deltaW ("scatterLUT light curve max derivative(p)", Range(1.0,20.0)) = 4.0
        lengthL ("scatterLUT light curve max range(v)", Range(0.5,1.0)) = 1.0
        lengthW ("scatterLUT light curve max range(p)", Range(0.5,1.0)) = 1.0
        exposure ("exposure", Float) = 16.0
        minh ("planet ground radius", Float) = 63.71393
        maxh ("planet sky radius", float) = 64.71393
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
        sunColor ("Sun Color", Color) = (1,1,1,0.24)
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,0.618)
        reayleigh_scatter ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,11.69904)
        molecule_absorb ("Molecule Absorb Factor", Vector) = (0,0,0,0)
        OZone_absorb ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,6.4)
        mie_scatter ("Mie scatter Factor", Vector) = (3.996,3.996,3.996,3.996)
        mie_absorb ("Mie absorb Factor", Vector) = (4.44,4.44,4.44,4.44)
        translucentLUT ("translucent LUT", 2D) = "white"{}
        outSunLightLUT ("out sun light LUT", 2D) = "white"{}
        inSunLightLUT ("in sun light LUT", 2D) = "white"{}
        planetLightTexture ("planet light texture", 2D) = "black"{}
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Opaque"}
        // Blend SrcAlpha OneMinusSrcAlpha
        // Blend SrcAlpha One
        Blend One One
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityShaderVariables.cginc"
            #include "../PBSky.cginc"
        
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
        
            sampler2D planetLightTexture;
            float4 sunColor;
            
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
            
            fixed4 frag (v2f i) : SV_Target
            {
                // const float3 OZone_absorb = float3(0.21195,0.20962,0.01686);
                // const float3 OZone_absorb = float3(0.065,0.1881,0.0085);
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                // float3 eye = normalize(i.position - _WorldSpaceCameraPos.xyz);
                float3 pos = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));

                float3 fargPos = i.position - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));
                float2 uv = pos2UV(UnityWorldToObjectDir(fargPos));
                
                
                float4 col = max(tex2Dlod(planetLightTexture,float4(uv.x,uv.y,0.0,0.0)),0.0);
                // col.x = 1.0;
                // col.y = 1.0;
                // col.z = 1.0;
                AtmospherePropInfo infos = getAtmospherePropInfoByRelPos(pos,fargPos,sun);
                float4 translucentLight = getLightTranslucent(infos.ahlwE.zy);
                float4 translucentGround = getGroundTranslucent(infos);
                // col.xz += float2(0.08,0.2) * col.w;
                col.xyz *= clamp((1.0 - bright(translucentLight) * 4.0),0.0,1.0);
                col.xyz *= translucentGround;
                col.xyz = hdr(col.xyz);
                // col.xyz = ACESTonemap(col.xyz);
                // col.w = 0.0;
                // // float2 SCREEN = i.screen.xy/i.screen.z;
                // // return tex2D(_CameraDepthTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0);
                // color = abs(IngOZoneDensity(h0+1.0,pos.y * length(eye.xz)) - IngOZoneDensity(pos.y*eye.y,pos.y * length(eye.xz))) * (float3(1.0,1.0,1.0)-OZone_absorb);

                return col;
                // // return fixed4(SCREEN.x,SCREEN.y,0.0,1.0);

            }

            ENDCG
        }
    }
}