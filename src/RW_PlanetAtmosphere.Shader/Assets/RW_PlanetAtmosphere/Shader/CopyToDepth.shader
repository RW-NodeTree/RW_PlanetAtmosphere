Shader "Planet/CopyToDepth"
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
            // Blend 0 One Zero, 1 One Zero
            Cull Off
            ZTest Always
            ZWrite On
            CGPROGRAM
            #pragma vertex basicVert
            #pragma fragment frag

            #include "./BaseInc.cginc"

            f2bColor frag (v2f i)
            {
                f2bColor o;
                o.reflection = float4(0,0,0,0);
                o.depthTexel = tex2Dlod(_CameraDepthTexture,float4(0.5*i.screenNear.xy/i.screenNear.w + 0.5,0,0)).x;
                o.depth = o.depthTexel;
                return o;
                // return tex2Dlod(_MainTex,float4(i.uv.xy,0,0));
                // return float4(i.uv.xy,0,0);
                // return Linear01Depth(depth);
            }

            ENDCG
        }
    }
}