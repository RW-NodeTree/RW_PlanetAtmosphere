Shader "Planet/SkyBoxCloud"
{
    Properties
    {
        // playRange ("play speed", Range(0, 1)) = 0.015625
        refraction ("refraction", Range(0, 1)) = 1.0
        luminescen ("luminescen", Range(0, 1)) = 0.0
        // sunRadius("sun Radius", Float) = 6960
        // sunDistance("sun Distance", Float) = 1495978.92
        exposure ("exposure", Float) = 16.0
        // flowDir ("flow dir", Float) = 0.0
        radius ("cloud layer radius", Float) = 63.76393
        normal ("cloud normal", Vector) = (0,1,0,0)
        tangent ("cloud tangent", Vector) = (1,0,0,0)
        cloudTexture ("cloud texture", 2D) = "transparent"{}
        // noiseTexture ("noise texture", 2D) = "transparent"{}
    }

    
    CGINCLUDE

    // #pragma multi_compile_fwdbase

    #include "../BaseInc.cginc"
    #include "../PBAttach.cginc"
    // #include "../PBSky.cginc"


    sampler2D cloudTexture;
    float4 cloudTexture_TexelSize;
    // sampler2D noiseTexture;
    
    float refraction;
    float luminescen;
    // float playRange;
    // float flowDir;
    float exposure;

    // float4 sampleSphere(float2 uv)
    // {
    //     float2 n_flowDir = float2(cos(flowDir * UNITY_PI / 180.0),sin(flowDir * UNITY_PI / 180.0));
    //     float4 col = abs(tex2Dlod(cloudTexture,float4(uv.x,uv.y,0.0,0.0)));
    //     float4 noiseA = abs(tex2Dlod(noiseTexture,float4(uv.xy + 0.5 * playRange * _Time.x * n_flowDir,0.0,0.0)));
    //     float4 noiseB = abs(tex2Dlod(noiseTexture,float4(uv.xy + playRange * _Time.x * n_flowDir,0.0,0.0)));
    //     float playRange_pixel = ceil(playRange * cloudTexture_TexelSize.z);
    //     float disCloud = 0;
    //     float end = exp(-playRange);
    //     for(int i = 1; i <= playRange_pixel; i++)
    //     {
    //         disCloud = max(disCloud,abs(tex2Dlod(cloudTexture,float4(uv.xy + float(i) * cloudTexture_TexelSize.x * n_flowDir,0.0,0.0)).w) * (exp(- float(i) * cloudTexture_TexelSize.x) - end) / (1.0 - end));
    //     }
    //     col.w = clamp(col.w, disCloud * noiseA.x * noiseB.x, 1.0);
    //     col.xyz = col.www;
    //     return col;
    // }

    float4 sampleSphere(float2 uv)
    {
        return tex2Dlod(cloudTexture,float4(uv,0,0));
    }
    ENDCG
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "LightMode"="ForwardBase"}
        Cull Off
        // Pass 0; Cloud Shadow
        Pass
        {
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
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 fargPos = worldPosFromDepthMap(i,depth);
                fargPos -= i.worldSpaceZeroPoint;
        
                float4 cloudColorA;
                float4 cloudPosA;
                float4 cloudColorB;
                float4 cloudPosB;
                float radiusCache = radius;
                // radius *= 0.999;
                getColorFromSphere(fargPos,sun,cloudColorA,cloudPosA,cloudColorB,cloudPosB);
                // radius = radiusCache;
                if(cloudPosA.w == 0 && cloudPosB.w == 0) discard;
                fargPos += i.worldSpaceZeroPoint;
                if(length(fargPos) > radius) discard;

                // float maxDistance = radius * sunDistance * ingLightCount / (sunRadius - radius * ingLightCount);
                // maxDistance = sqrt(maxDistance * maxDistance + radius * radius);
                float4 shadowFactor = lerp(1.0,1.0 - cloudColorA.w,cloudPosA.w) * lerp(1.0,1.0 - cloudColorB.w,cloudPosB.w);
                shadowFactor.w = 1;
                return shadowFactor;
            }

            ENDCG
        }

        // Pass 1; Cloud Trans A
        Pass
        {
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
                // float depth;
                float3 eye = i.worldSpaceFarPos - i.worldSpaceNearPos;
                float3 pos = i.worldSpaceNearPos;
                // float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                // fargPos -= i.worldSpaceZeroPoint;
        
                float4 cloudColorA;
                float4 cloudPosA;
                float4 cloudColorB;
                float4 cloudPosB;
                getColorFromSphere(pos,eye,cloudColorA,cloudPosA,cloudColorB,cloudPosB);
                if(cloudPosA.w == 0) discard;
                float4 clipSpacePos = UnityWorldToClipPos(cloudPosA.xyz + i.worldSpaceZeroPoint);
                o.transFactor = 1.0 - cloudColorA.w;
                // o.reflection = 1.0 - cloudPosA.w;
                // o.reflection.w = 1;
                o.depth = clipSpacePos.z / clipSpacePos.w;
                return o;
            }

            ENDCG
        }

        // Pass 2; Cloud Trans B
        Pass
        {
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
                // float depth;
                float3 eye = i.worldSpaceFarPos - i.worldSpaceNearPos;
                float3 pos = i.worldSpaceNearPos;
                // float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                // fargPos -= i.worldSpaceZeroPoint;
        
                float4 cloudColorA;
                float4 cloudPosA;
                float4 cloudColorB;
                float4 cloudPosB;
                getColorFromSphere(pos,eye,cloudColorA,cloudPosA,cloudColorB,cloudPosB);
                if(cloudPosB.w == 0) discard;
                float4 clipSpacePos = UnityWorldToClipPos(cloudPosB.xyz + i.worldSpaceZeroPoint);
                o.transFactor = 1.0 - cloudColorB.w;
                // o.reflection = 1.0 - cloudPosB.w;
                // o.reflection.w = 1;
                o.depth = clipSpacePos.z / clipSpacePos.w;
                return o;
            }

            ENDCG
        }

        // Pass 3; Cloud Color A
        Pass
        {
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
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = i.worldSpaceFarPos - i.worldSpaceNearPos;
                float3 pos = i.worldSpaceNearPos;
                float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                fargPos -= i.worldSpaceZeroPoint;
        
                float4 cloudColorA;
                float4 cloudPosA;
                float4 cloudColorB;
                float4 cloudPosB;
                getColorFromSphere(pos,eye,cloudColorA,cloudPosA,cloudColorB,cloudPosB);
                if(cloudPosA.w == 0) discard;
                float4 clipSpacePos = UnityWorldToClipPos(cloudPosA.xyz + i.worldSpaceZeroPoint);
                o.reflection.xyz = max(cloudColorA.xyz * _LightColor0.xyz * cloudColorA.w * refraction * exposure,0) * saturate(dot(cloudPosA.xyz,sun));
                o.reflection.w = cloudColorA.w;
                o.depthTexel = clipSpacePos.z / clipSpacePos.w;
                o.depth = o.depthTexel;
                return o;
        
            }
            ENDCG
        }

        // Pass 4; Cloud Color B
        Pass
        {
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
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = i.worldSpaceFarPos - i.worldSpaceNearPos;
                float3 pos = i.worldSpaceNearPos;
                float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                fargPos -= i.worldSpaceZeroPoint;
        
                float4 cloudColorA;
                float4 cloudPosA;
                float4 cloudColorB;
                float4 cloudPosB;
                getColorFromSphere(pos,eye,cloudColorA,cloudPosA,cloudColorB,cloudPosB);
                if(cloudPosB.w == 0) discard;
                float4 clipSpacePos = UnityWorldToClipPos(cloudPosB.xyz + i.worldSpaceZeroPoint);
                o.reflection.xyz = max(cloudColorB.xyz * _LightColor0.xyz * cloudColorB.w * refraction * exposure, 0) * saturate(dot(cloudPosB.xyz,sun));
                o.reflection.w = cloudColorB.w;
                o.depthTexel = clipSpacePos.z / clipSpacePos.w;
                o.depth = o.depthTexel;
                return o;
        
            }
            ENDCG
        }

        // Pass 5; Cloud Luminescen A
        Pass
        {
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
                float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                fargPos -= i.worldSpaceZeroPoint;
        
                float4 cloudColorA;
                float4 cloudPosA;
                float4 cloudColorB;
                float4 cloudPosB;
                getColorFromSphere(pos,eye,cloudColorA,cloudPosA,cloudColorB,cloudPosB);
                if(cloudPosA.w == 0) discard;
                float4 clipSpacePos = UnityWorldToClipPos(cloudPosA.xyz + i.worldSpaceZeroPoint);
                o.reflection.xyz = max(cloudColorA.xyz * cloudColorA.w * luminescen * exposure, 0);
                o.reflection.w = 0;
                o.depthTexel = clipSpacePos.z / clipSpacePos.w;
                o.depth = o.depthTexel;
                return o;
        
            }
            ENDCG
        }

        // Pass 6; Cloud Luminescen B
        Pass
        {
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
                float3 fargPos = worldPosFromDepthMap(i,depth);
                pos -= i.worldSpaceZeroPoint;
                fargPos -= i.worldSpaceZeroPoint;
        
                float4 cloudColorA;
                float4 cloudPosA;
                float4 cloudColorB;
                float4 cloudPosB;
                getColorFromSphere(pos,eye,cloudColorA,cloudPosA,cloudColorB,cloudPosB);
                if(cloudPosB.w == 0) discard;
                float4 clipSpacePos = UnityWorldToClipPos(cloudPosB.xyz + i.worldSpaceZeroPoint);
                o.reflection.xyz = max(cloudColorB.xyz * cloudColorB.w * luminescen * exposure, 0);
                o.reflection.w = 0;
                o.depthTexel = clipSpacePos.z / clipSpacePos.w;
                o.depth = o.depthTexel;
                return o;
        
            }
            ENDCG
        }
    }
}
