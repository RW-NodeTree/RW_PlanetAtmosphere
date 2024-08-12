Shader "Planet/SkyBoxLUT"
{
    Properties
    {
        exposure ("exposure", Range(0, 20)) = 16.0
        ground_refract ("ground refract", Range(0, 1)) = 0.0625
        ground_light ("ground light", Range(0, 1)) = 0.0
        deltaAHLW_L ("scatterLUT light curve max derivative(v)", Range(1.0,20.0)) = 8.0
        deltaAHLW_W ("scatterLUT light curve max derivative(p)", Range(1.0,20.0)) = 4.0
        lengthAHLW_L ("scatterLUT light curve max range(v)", Range(0.5,1.0)) = 1.0
        lengthAHLW_W ("scatterLUT light curve max range(p)", Range(0.5,1.0)) = 1.0
        minh ("planet ground radius", Float) = 63.71393
        maxh ("planet sky radius", float) = 64.71393
        H_Reayleigh ("reayleigh factor scale", float) = 0.08
        H_Mie ("min factor scale", float) = 0.02
        H_OZone ("ozone height", float) = 0.25
        D_OZone ("ozone radius", float) = 0.15
        SunColor ("SunColor", Color) = (1,1,1,0.24)
        mie_eccentricity ("mie eccentricity", Color) = (0.618,0.618,0.618,0.618)
        scatterLUT_Size ("scatterLUT_Size", Vector) = (0,0,0,0)
        reayleigh_scatter ("Reayleigh Scatter Factor", Vector) = (0.46278,1.25945,3.10319,11.69904)
        mie_scatter ("mie scatter", Vector) = (3.996,3.996,3.996,3.996)
        mie_absorb ("mie absorb", Vector) = (4.44,4.44,4.44,4.44)
        OZone_absorb ("OZone Absorb Factor", Vector) = (0.21195,0.20962,0.01686,6.4)
        translucentLUT ("translucent LUT", 2D) = "white"{}
        scatterLUT_Reayleigh ("scatter LUT Reayleigh", 2D) = "black"{}
        scatterLUT_Mie ("scatter LUT Mie", 2D) = "black"{}
    }
    SubShader
    {
        Tags { "Queue"="Transparent-1" "RenderType"="Transparent"}
        ZWrite Off
        Cull Off
        LOD 0
        GrabPass 
        {
            "_GrabTexture"
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "UnityShaderVariables.cginc"
            #include "./PBSky.cginc"
    
            struct appdata
            {
                float4 vertex : POSITION;
            };
    
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 position : TEXCOORD0;
                float4 screen : TEXCOORD1;
                float4 screenDir : TEXCOORD2;
                SHADOW_COORDS(3)
            };
    
            sampler2D _GrabTexture;
            float4 SunColor;
            
            float ground_refract;
            float ground_light;
            sampler2D _CameraDepthTexture;
            // const float s = float(6.6315851227221438037423488874623);
            // #define ingCount 6
            // #define ingLightCount 8
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.position =  mul(unity_ObjectToWorld, v.vertex).xyz;
                o.screen = o.vertex;
                o.screenDir = mul(UNITY_MATRIX_MV,v.vertex);
                TRANSFER_SHADOW(o)
                return o;
            }

            // float LinearEyeDepthToOutDepth(float z)
            // {
            //     return (1 - _ZBufferParams.w * z) / (_ZBufferParams.z * z);
            // }
            
            void sky(in float4 LIGHT0_COLOR, in float3 LIGHT0_DIRECTION, in float3 EYEDIR, in float3 POSITION, in float2 SCREEN, in float DEPTH, out float4 COLOR) {
                // POSITION /= 100000.0;
                // POSITION.y += h0 + 0.00005;
                
                float3 backGround = tex2D(_GrabTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0).xyz;
                // float3 rgb = max(0.0,ceil(dot(LIGHT0_DIRECTION,EYEDIR) - cos(0.004652439059837008))) * LIGHT0_COLOR + backGround;
                float4 rgb = float4(backGround,0.0);
                IngAirFogPropInfo infos = getIngAirFogPropInfoByRelPos(POSITION,EYEDIR,LIGHT0_DIRECTION,DEPTH);
    
                float4 translucentLight;
                float4 translucentGround;
                float4 reayleighScatter;
                float4 mieScatter;
                float4 scatter = LightScatter(infos, LIGHT0_COLOR, rgb * ground_refract, rgb * ground_light, translucentLight, translucentGround, reayleighScatter, mieScatter);

                scatter.xz += float2(0.08,0.2) * scatter.w;
                // rgb.z += 0.2 * rgb.w;
                // scatter.z += 0.2 * scatter.w;

                scatter.xyz = hdr(scatter.xyz);
                scatter.xyz = ACESTonemap(scatter.xyz);
                // scatter.xyz = ACESTonemap(scatter.xyz * exposure);
                // scatter.xyz = scatter.xyz * exposure;
                float3 p = infos.depth * infos.viewDir + float3(0.0,infos.h,0.0);
                float h = length(p);
                COLOR = rgb * translucentGround * step(maxh, h) + scatter;
            }
    
            float4 frag (v2f i, out float depth : SV_DEPTH) : SV_Target
            {
                // const float3 OZone_absorb = float3(0.21195,0.20962,0.01686);
                // const float3 OZone_absorb = float3(0.065,0.1881,0.0085);
                
                // fixed shadow = UNITY_SHADOW_ATTENUATION(i);
                float4 color = SunColor * UNITY_SHADOW_ATTENUATION(i,i.position);
                // float4 color = UNITY_SHADOW_ATTENUATION(i,i.position);
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = normalize(i.position - _WorldSpaceCameraPos.xyz);
                float3 pos = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, float4(0.0,0.0,0.0,1.0));
                float2 scr = i.screen.xy/i.screen.w;

                float scrDis = abs(normalize(i.screenDir.xyz).z);

                pos += eye * _ProjectionParams.y / scrDis;

                float currentDepth = max(step(length(pos),maxh), i.screen.z/i.screen.w);

                depth = currentDepth;
                // return float4(color.x,color.y,color.z,1.0);
                // return float4(_ProjectionParams.y,_ProjectionParams.y,_ProjectionParams.y,1.0);
                float depthValue = tex2D(_CameraDepthTexture, float2(1.0 + scr.x,1.0 - scr.y) / 2.0).x;
                depthValue = LinearEyeDepth(depthValue);
                depthValue -= _ProjectionParams.y;
                depthValue /= scrDis;
                sky(color,sun,eye,pos,scr,depthValue,color);
    
                // // float2 SCREEN = i.screen.xy/i.screen.z;
                // // return tex2D(_CameraDepthTexture,float2(1.0 + SCREEN.x,1.0 - SCREEN.y) / 2.0);
                // color = abs(IngOZoneDensity(h0+1.0,pos.y * length(eye.xz)) - IngOZoneDensity(pos.y*eye.y,pos.y * length(eye.xz))) * (float3(1.0,1.0,1.0)-OZone_absorb);
                
                // return float4(scrDir.x,scrDir.y,scrDir.z,1.0);
                return float4(color.x,color.y,color.z,1.0);
                // // return fixed4(SCREEN.x,SCREEN.y,0.0,1.0);
    
            }
            
            ENDCG
        }
    }
}
