Shader "Custom/BasicRing" {
    Properties {
        ringMap ("Map", 2D) = "gary" {}
        ringFromTo ("Ring From To", Vector) = (110,130,0,0)
        normal ("Ring Normal", Vector) = (0,1,0,0)
        ground_refract ("ground refract", Range(0, 1)) = 1.0
        ground_light ("ground light", Range(0, 1)) = 0.0
        opacity ("opacity multiplier", Range(0, 1)) = 1.0
        deltaL ("scatterLUT light curve max derivative(v)", Range(1.0,20.0)) = 8.0
        deltaW ("scatterLUT light curve max derivative(p)", Range(1.0,20.0)) = 4.0
        lengthL ("scatterLUT light curve max range(v)", Range(0.5,1.0)) = 1.0
        lengthW ("scatterLUT light curve max range(p)", Range(0.5,1.0)) = 1.0
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
        exposure ("exposure", Float) = 16.0
        minh ("planet ground radius", Float) = 63.71393
        maxh ("planet sky radius", Float) = 64.71393
        sunColor ("Sun Color", Color) = (1,1,1,0.24)
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,0.618)
        reayleigh_scatter ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,11.69904)
        molecule_absorb ("Molecule Absorb Factor", Vector) = (0,0,0,0)
        OZone_absorb ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,6.4)
        mie_scatter ("Mie scatter Factor", Vector) = (3.996,3.996,3.996,3.996)
        mie_absorb ("Mie absorb Factor", Vector) = (4.44,4.44,4.44,4.44)
        scatterLUT_Size ("scatterLUT Size", Vector) = (0,0,0,0)
        translucentLUT ("translucent LUT", 2D) = "white"{}
        outSunLightLUT ("out sun light LUT", 2D) = "white"{}
        inSunLightLUT ("in sun light LUT", 2D) = "white"{}
        scatterLUT_Reayleigh ("scatter LUT Reayleigh", 2D) = "black"{}
        scatterLUT_Mie ("scatter LUT Mie", 2D) = "black"{}
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType" = "Opaque" }
        Cull Off
        ZWrite Off
        
        Blend SrcAlpha OneMinusSrcAlpha
        // Blend SrcAlpha One
        LOD 100
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "../PBSky.cginc"
            #include "../PBAttach.cginc"

            struct vertex_data {
                float4 vertex : POSITION;
            };

            struct fragment_data {
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD0;
                float4 objPos : TEXCOORD1;
                // float4 screen : TEXCOORD2;
                // float4 screenDir : TEXCOORD4;
            };
            
            // uniform float4 _PlanetSunLightDirection;
            float4 sunColor;
            
            float ground_refract;
            float ground_light;
            float opacity;

            float4 sampleRing(float u)
            {
                return sampleRingBasic(u);
            }
            
            fragment_data vert(vertex_data v) {
                fragment_data f;
                f.vertex = UnityObjectToClipPos(v.vertex);
                f.worldPos = mul(unity_ObjectToWorld, v.vertex);
                // f.screenDir = mul(UNITY_MATRIX_MV,v.vertex);
                f.objPos = v.vertex;
                // f.screen = f.vertex;
                return f;
            }

            fixed4 frag(fragment_data f) : SV_Target {
                // float3 lightDir = normalize(-_PlanetSunLightDirection.xyz);
                // float3 sphereCenter = mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0)).xyz;
                float3 sphereCenter = float3(0.0,0.0,0.0);

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 worldPos = f.worldPos.xyz - sphereCenter;
                float3 pos = _WorldSpaceCameraPos.xyz - sphereCenter;
                
                float t;
                float4 pixel = getColorFromRing(pos,worldPos - pos,worldPos,t);

                
                AtmospherePropInfo infos = getAtmospherePropInfoByRelPos(pos,worldPos,lightDir);
                float4 translucentLight = getLightTranslucent(infos.ahlwE.zy);
                float4 translucentGround = getGroundTranslucent(infos);
                
                
                float4 reayleighScatter;
                float4 mieScatter;
                float4 scatter = getSkyScatter(infos,translucentGround,reayleighScatter,mieScatter);
                
        
        
                scatter = max(scatter, 0.0);
                scatter.xz += float2(0.08,0.2) * scatter.w;
                scatter.xyz = hdr(scatter.xyz);
                pixel.xyz *= translucentGround * (translucentLight * ground_refract + ground_light);
                pixel.xyz += scatter;
                // pixel.xyz = ACESTonemap(pixel.xyz);

                return pixel;
            }
            ENDCG
        }
    }
}