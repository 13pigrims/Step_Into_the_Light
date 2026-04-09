Shader "Custom/HalftoneObject"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _MaxDots ("Max Dots", Range(1, 1000)) = 100
        _DotSize ("Dot Size", Range(0.01, 10.0)) = 1.0
        _BrightnessMult ("Brightness Multiplier", Range(0.0, 2.0)) = 1.0
        [Toggle] _Invert ("Invert", Float) = 0
        _PrimaryColor ("Primary Color", Color) = (0.1, 0.1, 0.2, 1)
        _SecondaryColor ("Secondary Color", Color) = (0.9, 0.9, 0.8, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "HalftoneForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            float _MaxDots;
            float _DotSize;
            float _BrightnessMult;
            float _Invert;
            float4 _PrimaryColor;
            float4 _SecondaryColor;

            // RGB to HSV
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Aspect ratio correction
                float2 ratio = float2(1.0, _MainTex_TexelSize.z / _MainTex_TexelSize.w);

                // Pixelated UV for brightness sampling
                float2 pixelated_uv = floor(uv * _MaxDots * ratio) / (_MaxDots * ratio);

                // Dot pattern
                float dots = length(frac(uv * _MaxDots * ratio) - float2(0.5, 0.5)) / _DotSize;

                // Invert
                dots = lerp(dots, 1.0 - dots, _Invert);

                // Brightness from pixelated sample
                float3 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelated_uv).rgb;
                float brightness = rgb2hsv(texColor).z;
                dots += brightness * _BrightnessMult;

                // Sharpen with anti-aliasing
                dots = pow(dots, 5.0);
                dots = saturate(dots);

                // Mix colors
                float4 finalColor = lerp(_PrimaryColor, _SecondaryColor, dots);

                // Original alpha
                finalColor.a *= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;

                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
