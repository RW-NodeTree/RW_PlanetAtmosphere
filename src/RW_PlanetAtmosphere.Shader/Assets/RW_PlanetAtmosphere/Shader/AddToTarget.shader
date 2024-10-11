Shader "Planet/AddToTarget"
{
    // Properties
    // {
    //     _Color ("color", Color) = (1,1,1,1)
    // }
    SubShader
    {
        CGINCLUDE

        sampler2D ColorTex;
        sampler2D DepthTex;
        float4 MainColor;


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

        
        void fragColorAndDepth (v2f i, out float4 color : SV_TARGET, out float depth : SV_DEPTH)
        {
            color = float4(tex2Dlod(ColorTex,float4(i.uv,0,0)).xyz*MainColor.xyz,0);
            depth = tex2Dlod(DepthTex,float4(i.uv,0,0)).x;
        }

        float4 fragColorOnly (v2f i) : SV_TARGET
        {
            return tex2Dlod(ColorTex,float4(i.uv,0,0))*MainColor;
        }

        ENDCG

        Tags { "Queue"="Geometry" "RenderType"="Opaque"}
        Pass
        {
            Blend DstAlpha One
            ZTest Always
            Cull Off
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragColorAndDepth

            ENDCG
        }
        Pass
        {
            Blend One One
            Cull Off
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragColorAndDepth

            ENDCG
        }
        Pass
        {
            Blend One Zero
            ZTest Always
            Cull Off
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragColorOnly

            ENDCG
        }
        Pass
        {
            Blend One One
            ZTest Always
            Cull Off
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragColorOnly

            ENDCG
        }
    }
}