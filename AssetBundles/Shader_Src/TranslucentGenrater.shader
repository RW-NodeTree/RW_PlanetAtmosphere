Shader "Skybox/TranslucentGenrater"
{
    Properties
    {
        mie_amount ("mie amount", Range(0, 10)) = 3.996
        mie_absorb ("mie absorb", Range(0, 10)) = 1.11
        minh ("planet ground radius", float) = 63.71393
        maxh ("planet sky radius", float) = 64.71393
        H_Reayleigh ("reayleigh factor scale", float) = 0.08
        H_Mie ("min factor scale", float) = 0.02
        H_OZone ("ozone height", float) = 0.25
        D_OZone ("ozone radius", float) = 0.15
        translucentLUT ("translucent LUT", 2D) = "white"{}
        scatterLUT ("scatter LUT", 2D) = "black"{}
        scatterLUT_Size ("scatterLUT_Size", Vector) = (0,0,0,0)
        reayleighScatterFactor ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,0)
        OZoneAbsorbFactor ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,0)
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" "PreviewType"="Plane" }
        LOD 0
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
                float2 uv : TEXCOORD0;
            };

            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =  v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                i.uv *= translucentLUT_TexelSize.zw;
                i.uv /= translucentLUT_TexelSize.zw-float2(1.0,1.0);
                // const float3 reayleighScatterFactor = float3(0.58,1.35,3.31);
                // const float3 OZoneAbsorbFactor = float3(0.21195,0.20962,0.01686);
                // const float3 OZoneAbsorbFactor = float3(0.065,0.1881,0.0085);
                float3 mieScatterFactor = mie_amount.xxx;
                float3 mieAbsorbFactor = (mie_absorb * mie_amount).xxx;
                float2 uv = Map2AH(i.uv);
                float reayleigh = 0.0;
                float mie = 0.0;
                float oZone = 0.0;
                float dis = 0.0;
                float x0 = cos(uv.x)*uv.y;
                float h0 = sin(uv.x)*uv.y;
                IngAirDensity(x0, h0, reayleigh, mie, oZone);
                float3 light = translucent(float3(1.0,1.0,1.0), reayleighScatterFactor, reayleigh);
                light = translucent(light, mieScatterFactor + mieAbsorbFactor, mie);
                light = translucent(light, OZoneAbsorbFactor, oZone);
                // light = clamp(light,1.0/4096.0,1.0);
                return float4(light.x,light.y,light.z,1.0);
            }

            ENDCG
        }
    }
}
