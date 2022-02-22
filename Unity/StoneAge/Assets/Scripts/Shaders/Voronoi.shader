Shader "Hidden/Voronoi"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            int _ParticleCount = 0;
            int _Size = 2048;
            float _Scale = 1;
            float4 _Particles[10];
            float _MaxDistance = 2;
            float4 _ClusterColor = float4(1,1,1,1);

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                float minDistance = 50000;

                for (int j = 0; j < _ParticleCount; ++j) {
                    float2 particlePosition = _Particles[j].xy;
                    float d = distance(i.uv * _Size, particlePosition * _Scale);
                    if (d < minDistance) {
                        minDistance = d;
                    }
                }
                
                if (minDistance < _MaxDistance * _Scale) {
                    return _ClusterColor * minDistance * 0.1;
                } else {
                    return col;
                }
            }
            ENDCG
        }
    }
}
