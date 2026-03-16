Shader "Custom/ShadowMaskSensor_Pro"
{
    Properties { }
    SubShader
    {
        // 这里的 RenderType 必须和 Render Objects 里的 Queue (Opaque) 匹配
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "ShadowMaskPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // 必须：开启主光源阴影变体
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
            }; // 约定俗成的结构体名， 规定定点包含哪些数据

            struct Varyings {
                float4 positionCS : SV_POSITION;
                // sv 代表system value, 用于存储物体从模型空间转换到齐次裁剪空间的值
                float3 worldPos   : TEXCOORD0;
                // TEXCOORD只是语义名，不是纹理坐标
                // 注意：这里虽然使用了 TEXCOORD0 语义（插值寄存器），
                // 但实际存储并传递的是“世界空间坐标”，而非模型自带的 UV 信息。
                // 借用这个通道是为了利用 GPU 的硬件插值特性，让每个像素获得准确的世界位置。
            };

            Varyings vert (Attributes IN) {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target {
                // 计算阴影坐标
                float4 shadowCoord = TransformWorldToShadowCoord(IN.worldPos);
                
                // 获取主光源阴影衰减值 (0 = 影, 1 = 光)
                Light mainLight = GetMainLight(shadowCoord);
                float shadow = mainLight.shadowAttenuation;
                
                // 二值化处理：让黑白更纯粹
                float binary = step(0.1, shadow); 

                return half4(binary, binary, binary, 1.0);
            }
            ENDHLSL
        }
    }
}