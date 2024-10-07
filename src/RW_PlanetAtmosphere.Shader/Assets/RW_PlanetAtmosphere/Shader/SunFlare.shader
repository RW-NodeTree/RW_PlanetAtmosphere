Shader "Planet/SunFlare"
{
    Properties
    {
        sunRadius("sun Radius", Float) = 6960
        sunDistance("sun Distance", Float) = 1495978.92
        sunFlareTexture ("sun Dlare Texture", 2D) = "Black"{}

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
            
            f2bShadowTrans frag (v2f i)
            {
                f2bShadowTrans o;
                o.depth = i.screenFar.z / i.screenFar.w;
                o.shadowTransFactor = 1;
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

            // float sunRadius;
            // float sunDistance;
            sampler2D sunFlareTexture;
            sampler2D backgroundTexture;

            struct v2fExt
            {
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
                return o;
            }
            
            float4 frag (v2fExt i) : SV_TARGET
            {
                float2 sunPos2D = i.screenSunPos.xy / i.screenSunPos.z;
                if(i.screenSunPos.z <= 0 || abs(sunPos2D.x) > 1 || abs(sunPos2D.y) > 1) discard;
                float2 screenPos2D = i.screenFar.xy/i.screenFar.w - sunPos2D;
                screenPos2D *= _ScreenParams.xy / min(_ScreenParams.x,_ScreenParams.y);
                if(abs(screenPos2D.x) > 1 || abs(screenPos2D.y) > 1) discard;
                sunPos2D = 0.5 * sunPos2D + 0.5;
                screenPos2D = 0.5 * screenPos2D + 0.5;
                float4 screenSunLight = tex2Dlod(backgroundTexture,float4(sunPos2D,0,0));
                screenSunLight *= tex2Dlod(sunFlareTexture,float4(screenPos2D,0,0));
                screenSunLight.w = 0;
                return screenSunLight;
            }
            ENDCG
        }
    }
}
