Shader "Planet/ScatterGenrater"
{
    Properties
    {
        deltaL ("scatterLUT light curve max derivative(v)", Range(1.0,20.0)) = 8.0
        deltaW ("scatterLUT light curve max derivative(p)", Range(1.0,20.0)) = 4.0
        lengthL ("scatterLUT light curve max range(v)", Range(0.5,1.0)) = 1.0
        lengthW ("scatterLUT light curve max range(p)", Range(0.5,1.0)) = 1.0
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
        minh ("planet ground radius", float) = 63.71393
        maxh ("planet sky radius", float) = 64.71393
        H_Reayleigh ("reayleigh factor scale", float) = 0.08
        H_Mie ("min factor scale", float) = 0.02
        H_OZone ("ozone height", float) = 0.25
        D_OZone ("ozone radius", float) = 0.15
        translucentLUT ("translucent LUT", 2D) = "white"{}
        scatterLUT ("scatter LUT", 2D) = "black"{}
        scatterLUTSize ("scatterLUT Size", Vector) = (0,0,0,0)
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

            struct f2s
            {
                float4 reayleighScatter : SV_TARGET0;
                float4 mieScatter : SV_TARGET1;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =  v.uv;
                return o;
            }

            f2s frag (v2f i)
            {
                f2s result;
                i.uv *= scatterLUTSize.xy*scatterLUTSize.zw;
                i.uv /= scatterLUTSize.xy*scatterLUTSize.zw-float2(1.0,1.0);
                
                i.uv = saturate(i.uv);
                i.uv *= scatterLUTSize.xy*scatterLUTSize.zw-float2(1.0,1.0);
                float4 map = i.uv.xyxy;
                map.zw = floor(map.zw / scatterLUTSize.xy);
                map.xy = (map.xy - map.zw * scatterLUTSize.xy) / (scatterLUTSize.xy - float2(1.0,1.0));
                map.zw = map.zw / (scatterLUTSize.zw - float2(1.0,1.0));
                float4 ahlw = Map2AHLW(map);
                // return ahlw.yyyy-float4(minh,minh,minh,minh);
                GenScatterInfo(ahlw.x, ahlw.y, ahlw.z, ahlw.w,result.reayleighScatter,result.mieScatter);
                // result.reayleighScatter = min(result.reayleighScatter,1.0);
                // result.mieScatter = min(result.mieScatter,1.0);
                return result;
            }

            ENDCG
        }
    }
}
