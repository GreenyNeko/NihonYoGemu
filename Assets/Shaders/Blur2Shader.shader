Shader "Unlit/Blur2Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            static const int gaussFilter[49] ={0, 0, 1, 2, 1, 0, 0,
                                        0, 3, 13, 22, 13, 3, 0,
                                        1, 13, 59, 97, 59, 13, 1,
                                        2, 22, 97, 159, 97, 22, 2,
                                        1, 13, 59, 97, 59, 13, 1,
                                        0, 3, 13, 22, 13, 3, 0,
                                        0, 0, 1, 2, 1, 0, 0
                                    };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 colors = fixed4(0, 0, 0, 1);
                for (int j = 0; j < 7; j++)
                {
                    for (int k = 0; k < 7; k++)
                    {
                        colors += tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x, 0) * (j - 3) - float2(0, _MainTex_TexelSize.y) * (k - 3)) * gaussFilter[int(j % 7 + k * 7)] / 1013.f;
                    }
                }
                return col;
            }
            ENDHLSL
        }
    }
}
