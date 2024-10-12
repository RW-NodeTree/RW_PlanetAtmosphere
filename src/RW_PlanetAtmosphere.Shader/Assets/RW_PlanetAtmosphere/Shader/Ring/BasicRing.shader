Shader "Custom/BasicRing" {
    Properties {
        ringMap ("Map", 2D) = "gary" {}
        ringFromTo ("Ring From To", Vector) = (110,130,0,0)
        normal ("Ring Normal", Vector) = (0,1,0,0)
        refraction ("refraction", Range(0, 1)) = 1.0
        luminescen ("luminescen", Range(0, 1)) = 0.0
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
    }

    SubShader {
        CGINCLUDE

        #include "../BaseInc.cginc"
        #include "../PBAttach.cginc"
        #include "../PBSky.cginc"

        float refraction;
        float luminescen;

        float4 sampleRing(float u)
        {
            return sampleRingBasic(u);
        }
        ENDCG
        
        Tags {"Queue"="Transparent" "RenderType" = "Opaque" }
        Cull Off
        // Pass 0; Ring Shadow
        Pass {
            Blend Zero SrcColor
            ZWrite Off
            ZTest Always
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag
            
            float4 frag (v2f i) : SV_TARGET
            {
                // const float3 OZone_absorb = float3(0.21195,0.20962,0.01686);
                // const float3 OZone_absorb = float3(0.065,0.1881,0.0085);
                // float4 color = _LightColor0;
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.world);
                float depth;
                float linearDepth;
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                // float3 pos = i.worldSpaceNearPos;
                float3 fargPos = worldPosFromDepthMap(i,depth,linearDepth);
                if(linearDepth >= 1) discard;
                // pos -= i.worldSpaceZeroPoint;
                fargPos -= i.worldSpaceZeroPoint;

                float radius = max(ringFromTo.x,ringFromTo.y);
                float maxDistance = radius * sunDistance * ingLightCount / (sunRadius - radius * ingLightCount);
                maxDistance = sqrt(maxDistance * maxDistance + radius * radius);

                float t;
                float3 crossPoint;
                float4 color = getColorFromRing(fargPos,sun,crossPoint,t);
                float4 shadowFactor = lerp(1.0 - color.w, 1.0, saturate(max(length(fargPos) / maxDistance, 0)));
                shadowFactor.w = 1.0;
                return shadowFactor;
            }

            ENDCG
        }
        // Pass 1; Ring Trans
        Pass {
            Blend Zero SrcColor
            ZWrite Off
            // ZTest Less
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag
            f2bTrans frag (v2f i)
            {
                // const float3 OZone_absorb = float3(0.21195,0.20962,0.01686);
                // const float3 OZone_absorb = float3(0.065,0.1881,0.0085);
                f2bTrans o;
                // float4 color = _LightColor0;
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.world);
                float depth;
                float3 eye = i.worldSpaceFarPos - i.worldSpaceNearPos;
                float3 pos = i.worldSpaceNearPos;
                // float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                // fargPos -= i.worldSpaceZeroPoint;

                float t;
                float3 crossPoint;
                float4 color = getColorFromRing(pos,eye,crossPoint,t);
                float4 clipSpacePos = UnityWorldToClipPos(crossPoint + i.worldSpaceZeroPoint);
                o.transFactor = 1.0-color.w;
                // o.transFactor.w = 1;
                o.depth = clipSpacePos.z / clipSpacePos.w;
                return o;
            }

            ENDCG
        }
        // Pass 2; Ring Refraction
        Pass {
            Blend 0 One One
            Blend 1 One Zero
            // ZWrite Off
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag
            f2bColor frag (v2f i)
            {
                // const float3 OZone_absorb = float3(0.21195,0.20962,0.01686);
                // const float3 OZone_absorb = float3(0.065,0.1881,0.0085);
                f2bColor o;
                // float4 color = _LightColor0;
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.world);
                float depth;
                float3 eye = i.worldSpaceFarPos - i.worldSpaceNearPos;
                float3 pos = i.worldSpaceNearPos;
                // float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                // fargPos -= i.worldSpaceZeroPoint;

                float t;
                float3 crossPoint;
                float4 color = getColorFromRing(pos,eye,crossPoint,t);
                float4 clipSpacePos = UnityWorldToClipPos(crossPoint + i.worldSpaceZeroPoint);
                o.reflection.xyz = color.xyz*_LightColor0.xyz*color.w*refraction;
                o.reflection.w = color.w;
                o.depthTexel = clipSpacePos.z / clipSpacePos.w;
                o.depth = o.depthTexel;
                return o;
            }

            ENDCG
        }
        // Pass 3; Ring Luminescen
        Pass {
            Blend 0 One One
            Blend 1 One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag
            f2bColor frag (v2f i)
            {
                // const float3 OZone_absorb = float3(0.21195,0.20962,0.01686);
                // const float3 OZone_absorb = float3(0.065,0.1881,0.0085);
                f2bColor o;
                // float4 color = _LightColor0;
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.world);
                float depth;
                float3 eye = i.worldSpaceFarPos - i.worldSpaceNearPos;
                float3 pos = i.worldSpaceNearPos;
                // float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                // fargPos -= i.worldSpaceZeroPoint;

                float t;
                float3 crossPoint;
                float4 color = getColorFromRing(pos,eye,crossPoint,t);
                float4 clipSpacePos = UnityWorldToClipPos(crossPoint + i.worldSpaceZeroPoint);
                o.reflection.xyz = color.xyz*color.w*luminescen;
                o.reflection.w = 0;
                o.depthTexel = clipSpacePos.z / clipSpacePos.w;
                o.depth = o.depthTexel;
                return o;
            }

            ENDCG
        }
    }
}