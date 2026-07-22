Shader "Custom/DestructionBlockShader"
{
    Properties
    {
        _MainTex ("Crack Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0,1)) = 0
        _Color ("Tint Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True"
        }
        
        // Отключаем отсечение граней
        Cull Off
        // Включаем прозрачность
        Blend SrcAlpha OneMinusSrcAlpha
        // Не записываем в глубину
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            
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
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Progress;
            fixed4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Просто передаем UV без изменений для начала
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Берем цвет из текстуры
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Умножаем альфу на прогресс и цветовую альфу
                col.a *= _Progress * _Color.a;
                
                // Умножаем RGB на цвет
                col.rgb *= _Color.rgb;
                
                return col;
            }
            ENDCG
        }
    }
}