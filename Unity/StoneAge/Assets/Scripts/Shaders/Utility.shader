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
            float4 _Particles[2048];
            float _MaxDistance = 2;
            float4 _ClusterColor = float4(1,1,1,1);

            float findMinUntiledDistance(float2 tiledPoint, float2 targetPoint, float bound) {
                bool targetAboveMiddle = targetPoint.y > bound * 0.5;
                bool targetRightOfMiddle = targetPoint.x > bound * 0.5;

                float dist = 0.0f;

                if (targetAboveMiddle && targetRightOfMiddle) {
                    // Target is in upper right quadrant.
                    // Tiled in upper left quadrant.
                    float dist1 = distance(targetPoint, float2(tiledPoint.x + bound, tiledPoint.y));
                    // Tiled in lower left quadrant.
                    float dist2 = distance(targetPoint, float2(tiledPoint.x + bound, tiledPoint.y + bound));
                    // Tiled in lower right quadrant.
                    float dist3 = distance(targetPoint, float2(tiledPoint.x, tiledPoint.y + bound));
                    dist = min(dist1,  min(dist2, dist3));
                } else if (targetAboveMiddle && !targetRightOfMiddle) {
                    // Target is in upper left quadrant.
                    // Tiled in upper right quadrant.
                    float dist1 = distance(targetPoint, float2(tiledPoint.x - bound, tiledPoint.y));
                    // Tiled in lower left quadrant.
                    float dist2 = distance(targetPoint, float2(tiledPoint.x, tiledPoint.y + bound));
                    // Tiled in lower right quadrant.
                    float dist3 = distance(targetPoint, float2(tiledPoint.x - bound, tiledPoint.y + bound));
                    dist = min(dist1,  min(dist2, dist3));
                } else if (!targetAboveMiddle && !targetRightOfMiddle) {
                    // Target is in lower left quadrant.
                    // Tiled in upper right quadrant.
                    float dist1 = distance(targetPoint, float2(tiledPoint.x - bound, tiledPoint.y - bound));
                    // Tiled in upper left quadrant.
                    float dist2 = distance(targetPoint, float2(tiledPoint.x, tiledPoint.y - bound));
                    // Tiled in lower right quadrant.
                    float dist3 = distance(targetPoint, float2(tiledPoint.x - bound, tiledPoint.y));
                    dist = min(dist1,  min(dist2, dist3));
                } else if (!targetAboveMiddle && targetRightOfMiddle) {
                    // Target is in lower right quadrant.
                    // Tiled in upper right quadrant.
                    float dist1 = distance(targetPoint, float2(tiledPoint.x, tiledPoint.y - bound));
                    // Tiled in upper left quadrant.
                    float dist2 = distance(targetPoint, float2(tiledPoint.x + bound, tiledPoint.y - bound));
                    // Tiled in lower left quadrant.
                    float dist3 = distance(targetPoint, float2(tiledPoint.x + bound, tiledPoint.y));
                    dist = min(dist1,  min(dist2, dist3));
                }

                return dist;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                float minDistance = 9999999; // Arbitrary very large number.

                for (int j = 0; j < _ParticleCount; ++j) {
                    float2 particlePosition = _Particles[j].xy * _Scale;
                    float2 rasterPosition = i.uv * _Size;
                    float d = distance(rasterPosition, particlePosition);
                    float dPrime = findMinUntiledDistance(particlePosition, rasterPosition, _Size);
                    float actualMin = min(d, dPrime);
                    if (actualMin < minDistance) {
                        minDistance = actualMin;
                    }
                }
                
                if (minDistance < _MaxDistance * _Scale) {
                    return _ClusterColor * minDistance / _Scale;
                } else {
                    return col;
                }
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

            float4 getNeighborCoords(float invSize, float2 uv) {
                float leftX = uv.x - 1 * invSize;
                float upY = uv.y + 1 * invSize;
                float rightX = uv.x + 1 * invSize;
                float downY = uv.y - 1 * invSize;

                return float4(leftX, upY, rightX, downY);
            }

            float4x4 getNeighborsCross(sampler2D tex, float invSize, float2 uv) {
                float4 neighborCoords = getNeighborCoords(invSize, uv);

                float4 neighborL = tex2D(tex, float2(neighborCoords.x, uv.y));
                float4 neighborT = tex2D(tex, float2(uv.x, neighborCoords.y));
                float4 neighborR = tex2D(tex, float2(neighborCoords.z, uv.y));
                float4 neighborB = tex2D(tex, float2(uv.x, neighborCoords.w));

                float4x4 result;
                result[0] = neighborL;
                result[1] = neighborT;
                result[2] = neighborR;
                result[3] = neighborB;

                return result;
            }

            float4x4 getNeighborsX(sampler2D tex, float invSize, float2 uv) {
                float4 neighborCoords = getNeighborCoords(invSize, uv);
                
                float4 neighborUL = tex2D(tex, float2(neighborCoords.x, neighborCoords.y));
                float4 neighborUR = tex2D(tex, float2(neighborCoords.z, neighborCoords.y));
                float4 neighborDR = tex2D(tex, float2(neighborCoords.z, neighborCoords.w));
                float4 neighborDL = tex2D(tex, float2(neighborCoords.x, neighborCoords.w));

                float4x4 result;
                result[0] = neighborUL;
                result[1] = neighborUR;
                result[2] = neighborDR;
                result[3] = neighborDL;

                return result;
            }

            float4 blendColors(float4 source, float4 destination) {
                return source * source.a + destination * (1 - source.a);
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
    }
}
