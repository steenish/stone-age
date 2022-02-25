Shader "Hidden/Utility"
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
            Tags { "PassName" = "Voronoi" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #define PI 3.141593

            #include "UnityCG.cginc"
            #include "Helpers.cginc"

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
            sampler2D _NoiseTex;
            int _ParticleCount;
            int _Size;
            float4 _Particles[2048];
            float _MaxDistance;
            float4 _ClusterColor;

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float noise = (tex2D(_NoiseTex, i.uv).x - 0.5) * 2 * 0.2;

                float minDistance = 9999999; // Arbitrary very large number.

                for (int j = 0; j < _ParticleCount; ++j) {
                    float2 particlePosition = _Particles[j].xy;
                    float2 rasterPosition = i.uv * _Size;
                    float d = distance(rasterPosition, particlePosition);
                    float dPrime = findMinUntiledDistance(particlePosition, rasterPosition, _Size);
                    float actualMin = min(d, dPrime);
                    if (actualMin < minDistance) {
                        minDistance = actualMin;
                    }
                }
                
                float distanceParameter = minDistance / _MaxDistance;

                if (distanceParameter <= 1 - abs(noise)) {
                    distanceParameter += noise;
                    float threshold = 0.05;
                    float maxSurround = max(distanceParameter, threshold);
                    float invSurround = 1 - maxSurround;
                    float4 surroundColor = lerp(float4(0, 0, 0, 1), _ClusterColor, invSurround);

                    float centerCell = smoothMin(maxSurround, threshold, 0.3) * 25;
                    float3 centerColorHSV = rgb2hsv(_ClusterColor.rgb);
                    centerColorHSV = float3(centerColorHSV.r, centerColorHSV.g * 0.5, centerColorHSV.b * 0.5);
                    float4 centerBaseColor = float4(hsv2rgb(centerColorHSV).rgb, 1.0);
                    float4 centerColor = lerp(centerBaseColor, float4(0, 0, 0, 1), centerCell);

                    float invCenterCell = 1 - centerCell;
                    col = blendColors(blendMult(surroundColor, centerColor, invCenterCell), col);
                }

                return col;
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "BlendLichen" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Helpers.cginc"

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
            float4 _MainTex_TexelSize;
            sampler2D _AlbedoTex;
            float4 _AlbedoTex_TexelSize;

            float4 frag (v2f i) : SV_Target
            {
                float4 lichen = tex2D(_MainTex, i.uv);
                float4 albedo = tex2D(_AlbedoTex, i.uv);

                float4x4 lichenNeighbors = getNeighborsCross(_MainTex, _MainTex_TexelSize.x, i.uv);
                float4x4 albedoNeighbors = getNeighborsCross(_AlbedoTex, _AlbedoTex_TexelSize.x, i.uv);

                float4 result = blendColors(lichen, albedo);
                result += blendColors(lichenNeighbors[0], albedoNeighbors[0]);
                result += blendColors(lichenNeighbors[1], albedoNeighbors[1]);
                result += blendColors(lichenNeighbors[2], albedoNeighbors[2]);
                result += blendColors(lichenNeighbors[3], albedoNeighbors[3]);

                lichenNeighbors = getNeighborsX(_MainTex, _MainTex_TexelSize.x, i.uv);
                albedoNeighbors = getNeighborsX(_AlbedoTex, _AlbedoTex_TexelSize.x, i.uv);

                result += blendColors(lichenNeighbors[0], albedoNeighbors[0]);
                result += blendColors(lichenNeighbors[1], albedoNeighbors[1]);
                result += blendColors(lichenNeighbors[2], albedoNeighbors[2]);
                result += blendColors(lichenNeighbors[3], albedoNeighbors[3]);

                return result / 9;
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "LichenHeight" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Helpers.cginc"

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

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float average = (col.r + col.g + col.b) / 3;
                return float4(average.xxx, 1.0);
            }
            ENDCG
        }
    }
}
