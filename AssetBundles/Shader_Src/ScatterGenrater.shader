Shader "Skybox/ScatterGenrater"
{
    Properties
    {
        mie_amount ("mie amount", Range(0, 10)) = 3.996
        mie_absorb ("mie absorb", Range(0, 10)) = 1.11
        deltaAHLW_L ("scatterLUT light curve max derivative(v)", Range(1.0,20.0)) = 8.0
        deltaAHLW_W ("scatterLUT light curve max derivative(p)", Range(1.0,20.0)) = 4.0
        lengthAHLW_L ("scatterLUT light curve max range(v)", Range(0.5,1.0)) = 1.0
        lengthAHLW_W ("scatterLUT light curve max range(p)", Range(0.5,1.0)) = 1.0
        minh ("planet ground radius", float) = 63.71393
        maxh ("planet sky radius", float) = 64.71393
        H_Reayleigh ("reayleigh factor scale", float) = 0.08
        H_Mie ("min factor scale", float) = 0.02
        H_OZone ("ozone height", float) = 0.25
        D_OZone ("ozone radius", float) = 0.15
        translucentLUT ("translucent LUT", 2D) = "white"{}
        scatterLUT ("scatter LUT", 2D) = "black"{}
        scatterLUT_Size ("scatterLUT_Size", Vector) = (0,0,0,0)
        reayleighScatterFactor ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,11.69904)
        OZoneAbsorbFactor ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,6.4)
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" "PreviewType"="Plane" }
        LOD 0
        // Blend One Zero
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
                i.uv *= scatterLUT_Size.xy*scatterLUT_Size.zw;
                i.uv /= scatterLUT_Size.xy*scatterLUT_Size.zw-float2(1.0,1.0);
                float4 ahlw = Map2AHLW(i.uv);
                // return ahlw.yyyy-float4(minh,minh,minh,minh);
                float4 res = GenScatterInfo(ahlw.x, ahlw.y, ahlw.z, ahlw.w);
                return res;
            }

            ENDCG
        }
    }
}
