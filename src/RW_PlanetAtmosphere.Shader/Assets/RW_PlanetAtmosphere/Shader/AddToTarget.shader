Shader "Planet/AddToTarget"
{
    // Properties
    // {
    //     _MainTex ("Albedo (RGB)", 2D) = "(0,0,0,1)" {}
    // }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque"}
        Pass
        {
            Blend DstAlpha One
            ZTest Always
            Cull Off
            ZWrite Off
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag

            #include "./BaseInc.cginc"
            sampler2D ColorTex;
            sampler2D DepthTex;

            f2bColor frag (v2f i)
            {
                f2bColor o;
                o.reflection = float4(tex2Dlod(ColorTex,float4(0.5*i.screenNear.xy/i.screenNear.w + 0.5,0,0)).xyz,0);
                o.depthTexel = 0.0;
                o.depth = tex2Dlod(DepthTex,float4(0.5*i.screenNear.xy/i.screenNear.w + 0.5,0,0)).x;
                return o;
            }

            ENDCG
        }
        Pass
        {
            Blend One One
            Cull Off
            ZWrite Off
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag

            #include "./BaseInc.cginc"
            sampler2D ColorTex;
            sampler2D DepthTex;

            f2bColor frag (v2f i)
            {
                f2bColor o;
                o.reflection = float4(tex2Dlod(ColorTex,float4(0.5*i.screenNear.xy/i.screenNear.w + 0.5,0,0)).xyz,0);
                o.depthTexel = 0.0;
                o.depth = tex2Dlod(DepthTex,float4(0.5*i.screenNear.xy/i.screenNear.w + 0.5,0,0)).x;
                return o;
            }

            ENDCG
        }
    }
}