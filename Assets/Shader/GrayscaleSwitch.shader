Shader "Custom/GrayscaleSwitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _IsMonochrome ("Is Monochrome", Range(0, 1)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _IsMonochrome;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 计算灰度值
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                fixed4 grayColor = fixed4(gray, gray, gray, col.a);
                
                // 根据_IsMonochrome在彩色和灰度之间切换
                return lerp(col, grayColor, _IsMonochrome);
            }
            ENDCG
        }
    }
}