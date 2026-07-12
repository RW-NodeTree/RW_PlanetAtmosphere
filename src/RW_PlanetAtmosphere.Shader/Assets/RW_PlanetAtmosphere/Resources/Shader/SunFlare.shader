Shader "Planet/SunFlare"
{
    Properties
    {
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
        sunFlareTexture ("sun Dlare Texture", 2D) = "black"{}

    }


    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "LightMode"="ForwardBase"}
        Cull Off
        //pass 0 : Skybox Sun
        Pass
        {
            Blend One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag

            #include "./BaseInc.cginc"

            float sunRadius;
            float sunDistance;
            // sampler2D sunFlareTexture;
            
            f2bTrans frag (v2f i)
            {
                f2bTrans o;
                o.depth = i.screenFar.z / i.screenFar.w;
                o.transFactor = _LightColor0;
                o.transFactor.w = 1;
                float3 sun = normalize(_WorldSpaceLightPos0.xyz);
                float3 eye = normalize(i.worldSpaceFarPos - i.worldSpaceNearPos);
                float r = sqrt(sunDistance * sunDistance - sunRadius * sunRadius) / sunDistance;
                if(dot(sun,eye) < r) discard;
                return o;
            }
            ENDCG
        }
        //pass 1 : Sun Flare
        Pass
        {
            Blend One One
            ZWrite Off
            ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "./BaseInc.cginc"

            float sunRadius;
            float sunDistance;
            sampler2D sunFlareTexture;
            sampler2D backgroundTexture;

            #define ingLightCount 16

            struct v2fExt
            {
                float4 sunColor : COLOR0;
                float4 vertex : SV_POSITION;
                float4 screenNear : TEXCOORD0;
                float4 screenFar : TEXCOORD1;
                float3 cameraSpaceNearPos : TEXCOORD2;
                float3 cameraSpaceFarPos : TEXCOORD3;
                float3 worldSpaceNearPos : TEXCOORD4;
                float3 worldSpaceFarPos : TEXCOORD5;
                float3 worldSpaceZeroPoint : TEXCOORD6;
                float3 screenSunPos : TEXCOORD7;
            };



            float3x3 Crossfloat3x3_W2L(float3 d1, float3 d2) //d1,d2 on x-y plane, x axis lock on d1
            {
                d1 = normalize(d1);
                d2 = normalize(d2);
                float3 d3 = normalize(cross(d1,d2));
                d2 = normalize(cross(d3,d1));
                return float3x3(d1,d2,d3);
            }


            float4 IngSunLight()
            {
                float sunPerspective = asin(saturate(sunRadius/sunDistance));
                float3 d1 = abs(_WorldSpaceLightPos0.xyz);
                d1 = float3(
                    step(d1.x,d1.y) * step(d1.x,d1.z),
                    step(d1.y,d1.x) * step(d1.y,d1.z),
                    step(d1.z,d1.x) * step(d1.z,d1.y)
                );
                float4 result = 0.0;
                float3x3 mat = Crossfloat3x3_W2L(_WorldSpaceLightPos0.xyz, d1);
                for(int i = 0; i < ingLightCount; i++)
                {
                    float ratioI = (float(i) + 0.5) / ingLightCount;
                    float sizeFactor = (i + 1) * (i + 1) - i * i;
                    float2 sumAng;
                    sincos(ratioI * sunPerspective,sumAng.x,sumAng.y);
                    for(int j = 0; j < ingLightCount; j++)
                    {
                        float ratioJ = 2.0 * UNITY_PI * (float(j) + 0.5) / ingLightCount;
                        float3 sampleDir = mul(float3(sumAng.y, sumAng.x * cos(ratioJ), sumAng.x * sin(ratioJ)),mat);
                        sampleDir = UnityWorldToClipPos(sampleDir+_WorldSpaceCameraPos.xyz).xyw;
                        sampleDir.y *= _ProjectionParams.x;
                        sampleDir.xy /= sampleDir.z;
                        if(sampleDir.z > 0 && abs(sampleDir.x) <= 1 && abs(sampleDir.y) <= 1)
                        {
                            sampleDir.xy = 0.5 * sampleDir.xy + 0.5;
                            result += tex2Dlod(backgroundTexture,float4(sampleDir.xy,0,0)) * sizeFactor;
                        }
                    }
                }
                result /= ingLightCount * ingLightCount * (ingLightCount - 1.0);
                return result;
            }
            
            v2fExt vert(appdata v)
            {
                v2fExt o;
                v2f c = basicVert(v);
                o.vertex = c.vertex;
                o.screenNear = c.screenNear;
                o.screenFar = c.screenFar;
                o.cameraSpaceNearPos = c.cameraSpaceNearPos;
                o.cameraSpaceFarPos = c.cameraSpaceFarPos;
                o.worldSpaceNearPos = c.worldSpaceNearPos;
                o.worldSpaceFarPos = c.worldSpaceFarPos;
                o.worldSpaceZeroPoint = c.worldSpaceZeroPoint;
                o.screenSunPos = UnityWorldToClipPos(normalize(_WorldSpaceLightPos0.xyz) + _WorldSpaceCameraPos).xyw;
                o.screenSunPos.y *= _ProjectionParams.x;
                o.sunColor = IngSunLight();
                return o;
            }
            
            float4 frag (v2fExt i) : SV_TARGET
            {
                float2 sunPos2D = i.screenSunPos.xy / i.screenSunPos.z;
                if(i.screenSunPos.z <= 0) discard;
                float2 screenPos2D = i.screenFar.xy/i.screenFar.w - sunPos2D;
                screenPos2D *= _ScreenParams.xy / min(_ScreenParams.x,_ScreenParams.y);
                if(abs(screenPos2D.x) > 1 || abs(screenPos2D.y) > 1) discard;
                screenPos2D = 0.5 * screenPos2D + 0.5;
                float4 screenSunLight = i.sunColor;
                screenSunLight *= tex2Dlod(sunFlareTexture,float4(screenPos2D,0,0));
                screenSunLight.w = 0;
                return screenSunLight;
            }
            ENDCG
        }
    }
}
