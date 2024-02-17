Shader "Unlit/LevelAreaShader"
{
    Properties
    {
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float value = (i.uv.x < 0.25 || i.uv.x > 0.75 || i.uv.y < 0.25 || i.uv.y > 0.75) * 1;
                if (value == 0) discard;
                float2 dir = float2(sin(-_Time.y*2), cos(-_Time.y*2));
                float2 rightSideDir = float2(sin(-_Time.y*2 - 3.14 / 2), cos(-_Time.y*2 - 3.14 / 2));
                float2 pos = i.uv - float2(0.5, 0.5);
                float angle = acos(dot(normalize(pos), normalize(dir)));
                float angleToRight = acos(dot(normalize(rightSideDir), normalize(pos)));
                float angleReverse = acos(dot(normalize(pos), normalize(-dir)));
                float angleToLeft = acos(dot(normalize(-rightSideDir), normalize(pos)));
                float result = max((1 - angleReverse) * (angleToRight < 3.14/2),  (1 - angle) * (angleToLeft < 3.14/2));
                return float4(result + 0.1, 0, 0, 1);
            }
            ENDHLSL
        }
    }
}
