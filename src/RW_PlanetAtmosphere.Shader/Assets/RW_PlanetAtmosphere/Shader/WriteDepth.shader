Shader "Planet/WriteDepth"
{
    Properties
    {
        radius ("cloud layer radius", Float) = 63.76393
    }


    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" "PreviewType"="Plane" }
        Cull Off
        Pass
        {
            Blend One Zero
            // ZWrite Off
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag

            #include "./BaseInc.cginc"
            #include "./PBAttach.cginc"
            float4 sampleSphere(float2 uv)
            {
                return 0;
            }
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
                o.transFactor = clipSpacePos.z / clipSpacePos.w;
                o.depth = o.transFactor;
                return o;
        
            }
            ENDCG
        }

        Pass
        {
            Blend Zero One
            // ZWrite Off
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag

            #include "./BaseInc.cginc"
            #include "./PBAttach.cginc"
            float4 sampleSphere(float2 uv)
            {
                return 0;
            }
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
                o.transFactor = clipSpacePos.z / clipSpacePos.w;
                o.depth = o.transFactor;
                return o;
        
            }
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            Blend One Zero
            // ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}
