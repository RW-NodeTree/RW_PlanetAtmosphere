Shader "Planet/Tonemaps"
{
    Properties
    {
        gamma("gamma", Float) = 1.0
    }


    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "LightMode"="ForwardBase"}
        Cull Off

        CGINCLUDE
        // #include "UnityCG.cginc"

        float gamma;
        sampler2D backgroundTexture;
        
        
        float Luminance(float3 c){
            return dot(c, float3(0.2125, 0.7154, 0.0721));
        }
        
        static const float overlap = 0.2;
        
        static const float rgOverlap = 0.1 * overlap;
        static const float rbOverlap = 0.01 * overlap;
        static const float gbOverlap = 0.04 * overlap;
        
        static const float3x3 coneOverlap = float3x3(
                                                        1.0,        rgOverlap,  rbOverlap,
                                                        rgOverlap,  1.0, 		gbOverlap,
                                                        rbOverlap, 	rgOverlap, 	1.0
                                                    );
        
        static const float3x3 coneOverlapInverse = float3x3(
                                                                1.0 + (rgOverlap + rbOverlap),  -rgOverlap, 	                -rbOverlap,
                                                                -rgOverlap, 		            1.0 + (rgOverlap + gbOverlap),  -gbOverlap,
                                                                -rbOverlap, 		            -rgOverlap, 	                1.0 + (rbOverlap + rgOverlap)
                                                            );
        

        /////////////////////////////////////////////////////////////////////////////////
        
        float3 SEUSTonemap(float3 color)
        {
            color = mul(coneOverlap,color);
        
        
        
            const float p = gamma * 5.0;
            color = pow(color, p);
            color = color / (1.0 + color);
            color = pow(color, (1.0 / p));
        
        
            color = mul(coneOverlapInverse,color);
            color = saturate(color);
        
            return color;
        }

        /////////////////////////////////////////////////////////////////////////////////
        
        float3 HableTonemap(float3 color)
        {
        
            color = mul(coneOverlap,color);
        
            color *= 1.25;
        
            static const float a = 0.15;
            static const float b = 0.50;
            static const float c = 0.10;
            static const float d = 0.20;
            static const float e = 0.00;
            static const float f = 0.30;
        
            color = pow(color, gamma * 5.0);
        
            color = pow((color*(a*color+c*b)+d*e)/(color*(a*color+b)+d*f), 1.0 / (gamma * 5.0))-e/f;
            color = saturate(color);
        
        
            color = mul(coneOverlapInverse,color);
        
            return color;
        }
        
        /////////////////////////////////////////////////////////////////////////////////
        
        float3 UchimuraTonemap(float3 color) {
            static const float P = 1.0;  // max display brightness Default:1.2
            static const float a = 0.85;  // contrast Default:0.625
            static const float m = 0.175; // linear section start Default:0.1
            static const float l = 0.15;  // linear section length Default:0.0
            static const float c = 1.425; // black Default:1.33
            static const float b = 0.0;  // pedestal
        
        
            float l0 = ((P - m) * l) / a;
            float L0 = m - m / a;
            float L1 = m + (1.0 - m) / a;
            float S0 = m + l0;
            float S1 = m + a * l0;
            float C2 = (a * P) / (P - S1);
            float CP = -C2 / P;
        
            float3 w0 = 1.0 - smoothstep(0.0, m, color);
            float3 w2 = step(m + l0, color);
            float3 w1 = 1.0 - w0 - w2;
        
            float3 T = m * pow(color / m, c) + b;
            float3 S = P - (P - S1) * exp(CP * (color - S0));
            float3 L = m + a * (color - m);
        
            color = mul(coneOverlap,color);
        
            color = T * w0 + L * w1 + S * w2;
        
            color = mul(coneOverlapInverse,color);
            color = saturate(color);
        
            return color;
        }
        
        /////////////////////////////////////////////////////////////////////////////////
        
        float3 RRTAndODTFit(float3 v)
        {
            float3 a = v * (v + 0.0245786) - 0.000090537;
            float3 b = v * (1.0 * v + 0.4329510) + 0.238081;
            return a / b;
        }
        
        float3 ACESTonemap2(float3 color)
        {
            color *= 1.4;
            color = mul(coneOverlap,color);
            color = pow(color, gamma);
        
            color = RRTAndODTFit(color);
        
            color = mul(coneOverlapInverse,color);
        
            return color;
        }
        
        /////////////////////////////////////////////////////////////////////////////////
        
        float3 LottesTonemap(float3 color)
        {
            color *= 5.0;  // Default: 5.0
        
        
        
            // float peak = max(max(color.r, color.g), color.b);
            float peak = Luminance(color);
            float3 ratio = color / peak;
        
        
            //Tonemap here
            static const float contrast = 1.0; // Default: 1.1
            static const float shoulder = 1.0;
            static const float b = 1.0;	//Clipping point
            static const float c = 3.0;	//Speed of compression. Default: 5.0
        
            peak = pow(peak, 1.6);
        
            float x = peak;
            float z = pow(x, contrast);
            peak = z / (pow(z, shoulder) * b + c);
        
            peak = pow(peak, 1.0 / 1.6);
        
            float3 tonemapped = peak * ratio;
        
        
            float tonemappedMaximum = Luminance(tonemapped);
            float3 crosstalk = float3(10.0, 1.0, 10.0);
            float saturation = 0.75;  // Default: 1.1
            float crossSaturation = 1280.0;  // Default: 1114.0
        
            ratio = pow(ratio, saturation / crossSaturation);
            ratio = lerp(ratio, 1.0, pow(tonemappedMaximum, crosstalk));
            ratio = pow(ratio, crossSaturation);
        
            float3 outputColor = peak * ratio;
        
            return outputColor;
        }
        
        /////////////////////////////////////////////////////////////////////////////////
        
        float3 ACESTonemap(float3 color)
        {
            static const float a = 2.51;  // Default: 2.51f
            static const float b = 0.03;  // Default: 0.03f
            static const float c = 2.43;  // Default: 2.43f
            static const float d = 0.59;  // Default: 0.59f
            static const float e = 0.14;  // Default: 0.14f
            static const float p = 1.3;
            color = mul(coneOverlap, color);
            color = pow(color, float3(p,p,p));
            color = (color * (a * color + b)) / (color * (c * color + d) + e);
            color = pow(color, float3(1.0,1.0,1.0)/p);
            color = mul(coneOverlapInverse,color);
            color = clamp(color,float3(0.0,0.0,0.0),float3(1.0,1.0,1.0));
            return color;
        }
        
        /////////////////////////////////////////////////////////////////////////////////

        float3 agx_curve3(float3 v) {
            static const float threshold = 0.6060606060606061;
            static const float a_up = 69.86278913545539;
            static const float a_down = 59.507875;
            static const float b_up = 13.0 / 4.0;
            static const float b_down = 3.0 / 1.0;
            static const float c_up = -4.0 / 13.0;
            static const float c_down = -1.0 / 3.0;

            float3 mask = step(v, threshold);
            float3 a = a_up + (a_down - a_up) * mask;
            float3 b = b_up + (b_down - b_up) * mask;
            float3 c = c_up + (c_down - c_up) * mask;
            return 0.5 + (((-2.0 * threshold)) + 2.0 * v) * pow(1.0 + a * pow(abs(v - threshold), b), c);
        }

        float3 AGXTonemap(float3 /*Linear BT.709*/ci) {
            ci = pow(ci, 2.2 / gamma);
            const float min_ev = -12.473931188332413;
            const float max_ev = 4.026068811667588;
            const float dynamic_range = max_ev - min_ev;

            const float3x3 agx_mat = float3x3(0.8424010709504686, 0.04240107095046854, 0.04240107095046854, 0.07843650156180276, 0.8784365015618028, 0.07843650156180276, 0.0791624274877287, 0.0791624274877287, 0.8791624274877287);
            const float3x3 agx_mat_inv = float3x3(1.1969986613119143, -0.053001338688085674, -0.053001338688085674, -0.09804562695225345, 1.1519543730477466, -0.09804562695225345, -0.09895303435966087, -0.09895303435966087, 1.151046965640339);

            // Input transform (inset)
            ci = mul(ci, agx_mat);

            // Apply sigmoid function
            float3 ct = saturate(log2(ci) * (1.0 / dynamic_range) - (min_ev / dynamic_range));
            float3 co = agx_curve3(ct);

            // Inverse input transform (outset)
            co = mul(co, agx_mat_inv);

            return /*BT.709 (NOT linear)*/co;
        }

        

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


        #define TONEMAP(tonemap,uv) return float4(##tonemap(tex2Dlod(backgroundTexture,float4(uv,0,0))),1);

        ENDCG
        
        //pass 0 : SEUSTonemap
        Pass
        {
            Blend One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
                TONEMAP(SEUSTonemap,i.uv)
            }

            ENDCG
        }
        
        //pass 1 : HableTonemap
        Pass
        {
            Blend One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
                TONEMAP(HableTonemap,i.uv)
            }

            ENDCG
        }
        
        //pass 2 : UchimuraTonemap
        Pass
        {
            Blend One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
                TONEMAP(UchimuraTonemap,i.uv)
            }

            ENDCG
        }
        
        //pass 3 : ACESTonemap
        Pass
        {
            Blend One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
                TONEMAP(ACESTonemap,i.uv)
            }

            ENDCG
        }
        
        //pass 4 : ACESTonemap2
        Pass
        {
            Blend One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
                TONEMAP(ACESTonemap2,i.uv)
            }

            ENDCG
        }
        
        //pass 5 : LottesTonemap
        Pass
        {
            Blend One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
                TONEMAP(LottesTonemap,i.uv)
            }

            ENDCG
        }
        
        //pass 5 : AGXTonemap
        Pass
        {
            Blend One Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
                TONEMAP(AGXTonemap,i.uv)
            }

            ENDCG
        }
    }
}
