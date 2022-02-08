Shader "Custom/ErosionFinalization"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SecondTex ("Texture", 2D) = "white" {}
        _ThirdTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Tags { "PassName" = "HeightExtraction" }

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return float4(col.xxx, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "HeightDifferenceExtraction"}

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _SecondTex;

            fixed4 frag (v2f i) : SV_Target
            {
                float heightDifference = saturate((tex2D(_MainTex, i.uv) - tex2D(_SecondTex, i.uv)).x); // Positive height differences (where height is now larger) appear here.

                return float4(heightDifference.xxx, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "DebugToScreen"}

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _SecondTex;
            sampler2D _ThirdTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = float4(0, 0, 0, 0);

                if (i.uv.x < 0.5 && i.uv.y < 0.5) { // Velocity
                    col = tex2D(_ThirdTex, float2(i.uv.x * 2, i.uv.y * 2));
                } else if (i.uv.x < 0.5 && i.uv.y > 0.5) { // Terrain
                    col = tex2D(_MainTex, float2(i.uv.x * 2, (i.uv.y - 0.5) * 2));
                } else if (i.uv.x > 0.5 && i.uv.y > 0.5) { // Flux
                    col = tex2D(_SecondTex, float2((i.uv.x - 0.5) * 2, (i.uv.y - 0.5) * 2));
                }

                return col;
            }
            ENDCG
        }
    }
}
