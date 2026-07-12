Shader "Planet/RemoveAlpha"
{


    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "LightMode"="ForwardBase"}
        Cull Off
        
        Pass
        {
            Blend One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D backgroundTexture;

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
                return float4(tex2Dlod(backgroundTexture,float4(i.uv,0,0)).xyz,1);
            }

            ENDCG
        }
    }
}
