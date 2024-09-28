Shader "Planet/SkyBoxLUT"
{
    Properties
    {
        ground_refract ("ground refract", Range(0, 1)) = 1.0
        ground_light ("ground light", Range(0, 1)) = 0.0
        deltaL ("scatterLUT light curve max derivative(v)", Range(1.0,20.0)) = 8.0
        deltaW ("scatterLUT light curve max derivative(p)", Range(1.0,20.0)) = 4.0
        lengthL ("scatterLUT light curve max range(v)", Range(0.5,1.0)) = 1.0
        lengthW ("scatterLUT light curve max range(p)", Range(0.5,1.0)) = 1.0
        debug_draw ("debug draw", Range(0, 1)) = 0.0
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
        exposure ("exposure", Float) = 16.0
        minh ("planet ground radius", Float) = 63.71393
        maxh ("planet sky radius", float) = 64.71393
        H_Reayleigh ("reayleigh factor scale", float) = 0.08
        H_Mie ("min factor scale", float) = 0.02
        H_OZone ("ozone height", float) = 0.25
        D_OZone ("ozone radius", float) = 0.15
        sunColor ("Sun Color", Color) = (1,1,1,0.24)
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,0.618)
        scatterLUT_Size ("scatterLUT Size", Vector) = (0,0,0,0)
        reayleigh_scatter ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,11.69904)
        molecule_absorb ("Molecule Absorb Factor", Vector) = (0,0,0,0)
        OZone_absorb ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,6.4)
        mie_scatter ("Mie scatter Factor", Vector) = (3.996,3.996,3.996,3.996)
        mie_absorb ("Mie absorb Factor", Vector) = (4.44,4.44,4.44,4.44)
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,0.618)
        translucentLUT ("translucent LUT", 2D) = "white"{}
        outSunLightLUT ("out sun light LUT", 2D) = "white"{}
        inSunLightLUT ("in sun light LUT", 2D) = "white"{}
        scatterLUT_Reayleigh ("scatter LUT Reayleigh", 2D) = "black"{}
        scatterLUT_Mie ("scatter LUT Mie", 2D) = "black"{}
    }
    SubShader
    {
        Tags { "Queue"="Transparent-1" "RenderType"="Transparent"}
        ZWrite Off
        Cull Front
        // Cull Off
        LOD 0
        CGINCLUDE

        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "AutoLight.cginc"
        #include "../PBSky.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float3 world : TEXCOORD0;
            float4 screen : TEXCOORD1;
            float4 screenDir : TEXCOORD2;
            // SHADOW_COORDS(3)
        };

        float4 sunColor;
        
        float ground_refract;
        float ground_light;
        float debug_draw;
        sampler2D _CameraDepthTexture;
        // const float s = float(6.6315851227221438037423488874623);
        // #define ingCount 6
        // #define ingLightCount 8
        
        // v2f vert (appdata v)
        // {
        //     v2f o;
        //     o.pos = UnityObjectToClipPos(v.vertex);
        //     o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
        //     o.screen = o.pos;
        //     o.screenDir = mul(UNITY_MATRIX_MV,v.vertex);
        //     // TRANSFER_SHADOW(o);
        //     return o;
        // }
        
        
        
        v2f vert (appdata v)
        {
            v2f o;
            o.pos.xy = v.vertex.xy * 2.0 * _ProjectionParams.z;
            o.pos.z = _ProjectionParams.z - _ProjectionParams.y;
            o.pos.w = _ProjectionParams.z;
            o.screen = o.pos;
            o.screenDir = mul(unity_CameraInvProjection, o.screen);
            #if UNITY_UV_STARTS_AT_TOP
            o.screenDir.y = -o.screenDir.y;
            #endif
            o.screenDir /= o.screenDir.w;
            // o.screenDir.x = -o.screenDir.x;
            o.world = mul(UNITY_MATRIX_I_V,o.screenDir.xyzw).xyz;
            // o.world = -o.world;
            // o.world.y *= -1;
            // TRANSFER_SHADOW(o);
            return o;
        }

        // float LinearEyeDepthToOutDepth(float z)
        // {
        //     return (1 - _ZBufferParams.w * z) / (_ZBufferParams.z * z);
        // }
        

        
        ENDCG

        // Pass
        // {
        //     Cull Off
        //     ZTest Always
        //     Blend One Zero
        //     CGPROGRAM
        //     // #pragma target 3.0
        //     #pragma vertex vert
        //     #pragma fragment frag

        //     float4 frag (v2f i) : SV_Target
        //     {
        //         // return float4(1.0,0.0,0.0,1.0);
        //         // return i.screenDir;
        //         return float4(i.world,1.0);
        //     }
        //     ENDCG
        // }

        Pass
        {
            Blend Zero SrcColor
            CGPROGRAM
            // #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i, out float depth : SV_DEPTH) : SV_Target
            {
                float4 color = sunColor;
                // float4 color = _LightColor0;
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.world);
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = normalize(i.world - _WorldSpaceCameraPos.xyz);
                float3 pos = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));
                float2 scr = i.screen.xy/i.screen.w;
    
                float scrDis = abs(normalize(i.screenDir.xyz).z);
    
                pos += eye * _ProjectionParams.y / scrDis;
                // return float4(color.x,color.y,color.z,1.0);
                // return float4(_ProjectionParams.y,_ProjectionParams.y,_ProjectionParams.y,1.0);
                
                float depthValue = tex2D(_CameraDepthTexture, float2(1.0 + scr.x,1.0 - scr.y) / 2.0).x;
                depthValue = LinearEyeDepth(depthValue);
                depthValue += step(_ProjectionParams.z,depthValue*1.001) * 1e16;
                depthValue -= _ProjectionParams.y;
                depthValue /= scrDis;

                float3 fargPos = pos + eye * depthValue;
                AtmospherePropInfo infos = getAtmospherePropInfoByRelPos(pos,fargPos,sun);
                float4 translucentLight = getLightTranslucent(infos.ahlwE.zy);
                float4 translucentGround = getGroundTranslucent(infos);
                

                float4 result = (ground_refract * translucentLight + ground_light) * translucentGround;
    
                float4 clipSpacePos = UnityWorldToClipPos(pos);
                depth = max(step(length(pos),maxh), clipSpacePos.z / clipSpacePos.w);
                return result;
            }
            ENDCG
        }

        Pass
        {
            Blend One One
            CGPROGRAM
            // #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i, out float depth : SV_DEPTH) : SV_Target
            {
                // const float3 OZone_absorb = float3(0.21195,0.20962,0.01686);
                // const float3 OZone_absorb = float3(0.065,0.1881,0.0085);
                
                // fixed shadow = UNITY_SHADOW_ATTENUATION(i);
                // float4 color = sunColor * LIGHT_ATTENUATION(i);
                float4 color = sunColor;
                // float4 color = _LightColor0;
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.world);
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = normalize(i.world - _WorldSpaceCameraPos.xyz);
                float3 pos = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));
                float2 scr = i.screen.xy/i.screen.w;
    
                float scrDis = abs(normalize(i.screenDir.xyz).z);
    
                pos += eye * _ProjectionParams.y / scrDis;
                // return float4(color.x,color.y,color.z,1.0);
                // return float4(_ProjectionParams.y,_ProjectionParams.y,_ProjectionParams.y,1.0);
                
                float depthValue = tex2D(_CameraDepthTexture, float2(1.0 + scr.x,1.0 - scr.y) / 2.0).x;
                depthValue = LinearEyeDepth(depthValue);
                depthValue += step(_ProjectionParams.z,depthValue*1.001) * 1e16;
                depthValue -= _ProjectionParams.y;
                depthValue /= scrDis;
                // sky(color,sun,eye,pos,scr,depthValue,color);
    
                float3 fargPos = pos + eye * depthValue;
                AtmospherePropInfo infos = getAtmospherePropInfoByRelPos(pos,fargPos,sun);
                // float4 rgb = step(maxh, infos.ahlwE.y) * max(0.0,ceil(dot(sun,eye) - cos(0.004652439059837008))) * color + float4(backGround, 0.0);
                // float4 rgb = max(0.0,ceil(dot(sun,eye) - cos(0.004652439059837008))) * color + float4(backGround, 0.0);
    
                float4 clipSpacePos = UnityWorldToClipPos(pos);
    
                depth = max(step(length(pos),maxh), clipSpacePos.z / clipSpacePos.w);
                // if(infos.h > maxh)
                // {
                //     discard;
                //     return color;
                // }
                // float currentDepth = max(step(length(pos),maxh), step(infos.h,maxh)*clipSpacePos.z/clipSpacePos.w);
                // depth = currentDepth;
                float4 translucentGround = getGroundTranslucent(infos);
                float4 reayleighScatter;
                float4 mieScatter;
                float4 scatter = getSkyScatter(infos,translucentGround,reayleighScatter,mieScatter);
    
                scatter.xz += float2(0.08,0.2) * scatter.w;
                // rgb.z += 0.2 * rgb.w;
                // scatter.z += 0.2 * scatter.w;
    
                // scatter.xyz = round(scatter.xyz * 256.0) / 256.0;
                scatter.xyz = hdr(scatter.xyz);
                // scatter.xyz = ACESTonemap(scatter.xyz);
                // scatter.xyz = ACESTonemap(scatter.xyz * exposure);
                // scatter.xyz = scatter.xyz * exposure;
                // color = rgb * translucentGround * step(maxh, infos.ahlwE.y) + scatter;
                // color = translucentGround;
                // color = scatter;
                color = lerp(scatter, translucentGround, debug_draw);
                // color = lerp(scatter, translucentGround, debug_draw);
    
                // return float4(scrDir.x,scrDir.y,scrDir.z,1.0);
                return float4(color.x,color.y,color.z,1.0);
                // // return fixed4(SCREEN.x,SCREEN.y,0.0,1.0);
    
            }
            ENDCG
        }
    }
}
