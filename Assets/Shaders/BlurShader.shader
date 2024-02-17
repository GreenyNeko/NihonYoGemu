Shader "Unlit/BlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [ShowAsVector2]
        _PartitionMod("Partitioning", Float) = 1
        _Radius ("Radius", Float) = 0.03
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PartitionMod;
            float _Radius;

            float nrand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // calculate circles for each partition
                float2 partitions = _ScreenParams.xy / _PartitionMod;//_Partitioning.xy;
                float size = _Radius;
                // calculate random pos within partition
                float2 thisPos = floor(i.uv * partitions) / partitions;
                float randX = nrand(thisPos);
                float2 thisRand = float2(randX, nrand(float2(thisPos.x, randX)));
                float2 leftPos = floor(i.uv * partitions + float2(-0.5, 0)) / partitions;
                randX = nrand(leftPos);
                float2 leftRand = float2(randX, nrand(float2(leftPos.x, randX)));
                float2 rightPos = floor(i.uv * partitions + float2(0.5, 0)) / partitions;
                randX = nrand(rightPos);
                float2 rightRand = float2(randX, nrand(float2(rightPos.x, randX)));
                float2 topPos = floor(i.uv * partitions + float2(0, -0.5)) / partitions;
                randX = nrand(topPos);
                float2 topRand = float2(randX, nrand(float2(topPos.x, randX)));
                float2 bottomPos = floor(i.uv * partitions + float2(0, 0.5)) / partitions;
                randX = nrand(bottomPos);
                float2 bottomRand = float2(randX, nrand(float2(bottomPos.x, randX)));
                float2 topLeftPos = floor(i.uv * partitions + float2(-0.5, -0.5)) / partitions;
                randX = nrand(topLeftPos);
                float2 topLeftRand = float2(randX, nrand(float2(topLeftPos.x, randX)));
                float2 topRightPos = floor(i.uv * partitions + float2(0.5, -0.5)) / partitions;
                randX = nrand(topRightPos);
                float2 topRightRand = float2(randX, nrand(float2(topRightPos.x, randX)));
                float2 bottomLeftPos = floor(i.uv * partitions + float2(-0.5, 0.5)) / partitions;
                randX = nrand(bottomLeftPos);
                float2 bottomLeftRand = float2(randX, nrand(float2(bottomLeftPos.x, randX)));
                float2 bottomRightPos = floor(i.uv * partitions + float2(-0.5, 0.5)) / partitions;
                randX = nrand(bottomRightPos);
                float2 bottomRightRand = float2(randX, nrand(float2(bottomRightPos.x, randX)));


                // find closest circle and sample color from it
                fixed4 color;
                float closest = 9999;

                float2 pos = (thisPos + thisRand / partitions);
                float dist = distance(pos, i.uv);
                if (dist < size)
                {
                    closest = dist;
                    color = tex2D(_MainTex, pos);
                }
                pos = (leftPos + leftRand / partitions);
                dist = distance(pos, i.uv);
                if (dist < size && dist < closest)
                {
                    closest = dist;
                    color = tex2D(_MainTex, pos);
                }
                pos = (rightPos + rightRand / partitions);
                dist = distance(pos, i.uv);
                if (dist < size && dist < closest)
                {
                    closest = dist;
                    color = tex2D(_MainTex, pos);
                }
                pos = (topPos + topRand / partitions);
                dist = distance(pos, i.uv);
                if (dist < size && dist < closest)
                {
                    closest = dist;
                    color = tex2D(_MainTex, pos);
                }
                pos = (bottomPos + bottomRand / partitions);
                dist = distance(pos, i.uv);
                if (dist < size && dist < closest)
                {
                    closest = dist;
                    color = tex2D(_MainTex, pos);
                }
                pos = (topLeftPos + topLeftRand / partitions);
                dist = distance(pos, i.uv);
                if (dist < size && dist < closest)
                {
                    closest = dist;
                    color = tex2D(_MainTex, pos);
                }
                pos = (topRightPos + topRightRand / partitions);
                dist = distance(pos, i.uv);
                if (dist < size && dist < closest)
                {
                    closest = dist;
                    color = tex2D(_MainTex, pos);
                }
                pos = (bottomLeftPos + bottomLeftRand / partitions);
                dist = distance(pos, i.uv);
                if (dist < size && dist < closest)
                {
                    closest = dist;
                    color = tex2D(_MainTex, pos);
                }
                pos = (bottomRightPos + bottomRightRand / partitions);
                dist = distance(pos, i.uv);
                if (dist < size && dist < closest)
                {
                    closest = dist;
                    color = tex2D(_MainTex, pos);
                }
                if (closest > size)
                {
                    return fixed4(0,0,0,1);
                }
                return color * 0.75;
            }

            ENDHLSL
        }
    }
}
