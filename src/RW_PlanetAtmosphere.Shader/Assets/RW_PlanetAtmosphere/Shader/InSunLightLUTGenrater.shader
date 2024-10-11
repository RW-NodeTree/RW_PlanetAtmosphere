Shader "Planet/InSunLightLUTGenrater"
{
    Properties
    {
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
        minh ("planet ground radius", float) = 63.71393
        maxh ("planet sky radius", float) = 64.71393
        H_Reayleigh ("reayleigh factor scale", float) = 0.08
        H_Mie ("min factor scale", float) = 0.02
        H_OZone ("ozone height", float) = 0.25
        D_OZone ("ozone radius", float) = 0.15
        translucentLUT ("translucent LUT", 2D) = "white"{}
        inSunLightLUT ("in sun light LUT", 2D) = "white"{}
        reayleigh_scatter ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,11.69904)
        molecule_absorb ("Molecule Absorb Factor", Vector) = (0,0,0,0)
        OZone_absorb ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,6.4)
        mie_scatter ("Mie scatter Factor", Vector) = (3.996,3.996,3.996,3.996)
        mie_absorb ("Mie absorb Factor", Vector) = (4.44,4.44,4.44,4.44)
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,0.618)
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
                i.uv *= inSunLightLUT_TexelSize.zw;
                i.uv /= inSunLightLUT_TexelSize.zw-float2(1.0,1.0);
                float cachedMaxh = maxh;
                maxh = minh + 2.0 * (maxh - minh);
                float2 ah = fMap2AH(i.uv);
                maxh = cachedMaxh;
                ah.x = -ah.x;
                return IngSunLight(ah);
                // return float4(ah,0,0);
            }

            ENDCG
        }
    }
}
