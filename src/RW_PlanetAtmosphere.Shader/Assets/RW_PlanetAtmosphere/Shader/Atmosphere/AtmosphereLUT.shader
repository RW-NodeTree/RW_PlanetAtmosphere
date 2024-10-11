Shader "Planet/AtmosphereLUT"
{
    Properties
    {
        deltaL ("scatterLUT light curve max derivative(v)", Range(1.0,20.0)) = 8.0
        deltaW ("scatterLUT light curve max derivative(p)", Range(1.0,20.0)) = 2.0
        lengthL ("scatterLUT light curve max range(v)", Range(0.5,1.0)) = 1.0
        lengthW ("scatterLUT light curve max range(p)", Range(0.5,1.0)) = 1.0
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
        exposure ("exposure", Float) = 16.0
        minh ("planet ground radius", Float) = 63.71393
        maxh ("planet sky radius", float) = 64.21393
        H_Reayleigh ("reayleigh factor scale", float) = 0.08
        H_Mie ("min factor scale", float) = 0.02
        H_OZone ("ozone height", float) = 0.25
        D_OZone ("ozone radius", float) = 0.15
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,0.618)
        scatterLUTSize ("scatterLUT Size", Vector) = (0,0,0,0)
        reayleigh_scatter ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,11.69904)
        molecule_absorb ("Molecule Absorb Factor", Vector) = (0,0,0,0)
        OZone_absorb ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,6.4)
        mie_scatter ("Mie scatter Factor", Vector) = (3.996,3.996,3.996,3.996)
        mie_absorb ("Mie absorb Factor", Vector) = (4.44,4.44,4.44,4.44)
        mie_eccentricity ("mie eccentricity", Vector) = (0.618,0.618,0.618,0.618)
        translucentLUT ("translucent LUT", 2D) = "white"{}
        outSunLightLUT ("out sun light LUT", 2D) = "white"{}
        inSunLightLUT ("in sun light LUT", 2D) = "white"{}
        scatterLUT_Reayleigh ("scatter LUT Reayleigh", 2D) = "black"{}
        scatterLUT_Mie ("scatter LUT Mie", 2D) = "black"{}
    }
    SubShader
    {
        Tags { "Queue"="Transparent-1" "RenderType"="Transparent"}
        Cull Off
        ZWrite Off
        CGINCLUDE

        float exposure;
        #include "../BaseInc.cginc"
        #include "../PBSky.cginc"

        ENDCG
        
        // Pass 0; Planet Shadow
        Pass
        {
            Blend Zero SrcColor
            ZTest Always
            CGPROGRAM
            // #pragma target 3.0
            #pragma vertex basicVert
            #pragma fragment frag

            float4 frag (v2f i) : SV_TARGET
            {
                // float4 color = _LightColor0;
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.world);
                float depth;
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 pos = _WorldSpaceCameraPos.xyz;
                float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                fargPos -= i.worldSpaceZeroPoint;

                AtmospherePropInfo infos = getAtmospherePropInfoByRelPos(pos,fargPos,sun);
                // if(infos.ahlwS.y > maxh) discard;
                float4 translucentLight = getLightTranslucent(infos.ahlwE.zy);
            
                // translucentLight.xyz = infos.ahlwE.xzw;
                // o.transFactor.xz += float2(0.08,0.2) * o.transFactor.w * _LightColor0.w;
                translucentLight.w = 1.0;
                return translucentLight;
            }
            ENDCG
        }

        // Pass 1; Planet Trans
        Pass
        {
            Blend Zero SrcColor
            ZTest Less
            CGPROGRAM
            // #pragma target 3.0
            #pragma vertex basicVert
            #pragma fragment frag
            f2bTrans frag (v2f i)
            {
                f2bTrans o;
                // float4 color = _LightColor0;
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.world);
                float depth;
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 pos = i.worldSpaceNearPos;
                float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                fargPos -= i.worldSpaceZeroPoint;

                AtmospherePropInfo infos = getAtmospherePropInfoByRelPos(pos,fargPos,sun);
                if(infos.ahlwS.y > maxh) discard;
                float4 translucentGround = getGroundTranslucent(infos);
                
                o.transFactor = translucentGround;
                o.transFactor.w = 1.0;
    
                float4 clipSpacePos = UnityWorldToClipPos(pos + i.worldSpaceZeroPoint);
                o.depth = clipSpacePos.z / clipSpacePos.w;
                // o.depth = abs(min(depth * _ProjectionParams.x,o.depth * _ProjectionParams.x));
                return o;
            }
            ENDCG
        }

        // Pass 2; Base Color
        Pass
        {
            ZWrite On
            ZTest Always
            Blend One Zero
            CGPROGRAM
            // #pragma target 3.0
            #pragma vertex basicVert
            #pragma fragment frag
            f2bColor frag (v2f i)
            {
                f2bColor o;
                // float4 color = _LightColor0;
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.world);
                float depth;
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 pos = i.worldSpaceNearPos;
                float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                fargPos -= i.worldSpaceZeroPoint;

                AtmospherePropInfo infos = getAtmospherePropInfoByRelPos(pos,fargPos,sun);
                if(infos.ahlwS.y > maxh) discard;
                float4 translucentGround = getGroundTranslucent(infos);
                float4 reayleighScatter;
                float4 mieScatter;
                float4 scatter = getSkyScatter(infos,translucentGround,reayleighScatter,mieScatter) * _LightColor0;
                scatter.xz += float2(0.08,0.2) * scatter.w;
                scatter *= exposure;
                
                // o.reflection = lerp(scatter, translucentGround, debug_draw);
                // float allValid;
                // float4 valid;
                // float4 map = AHLW2Map(infos.ahlwS,valid,allValid);
                // o.reflection = step(-1.0,infos.ahlwS.xzww);
                o.reflection = scatter;
                o.reflection.w = 0.0;

                // float4
                // clipSpacePos = UnityWorldToClipPos(infos.shadowReciverPos + i.worldSpaceZeroPoint);
                // o.depthTexel = clipSpacePos.z / clipSpacePos.w;
                // o.depthTexel = abs(min(depth * _ProjectionParams.x,o.depthTexel * _ProjectionParams.x));
                // clipSpacePos = UnityWorldToClipPos(pos + i.worldSpaceZeroPoint);
                // o.depth = clipSpacePos.z / clipSpacePos.w;
                float4 clipSpacePos = UnityWorldToClipPos(pos + i.worldSpaceZeroPoint);
                o.depthTexel = clipSpacePos.z / clipSpacePos.w;
                o.depth = o.depthTexel;
                return o;
            }
            ENDCG
        }

    }
}
