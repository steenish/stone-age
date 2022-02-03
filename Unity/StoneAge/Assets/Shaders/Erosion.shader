Shader "Custom/Erosion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TerrainTex ("Texture", 2D) = "white" {}
        _FluxTex ("Texture", 2D) = "white" {}
        _VelocityTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Texture", 2D) = "white" {}
        _TimeStep ("Time step", Float) = 0.5
        _RainScale ("Rain scale", Float) = 0.5
        _PipeArea ("Virtual pipe cross-sectional area", Float) = 0.5
        _GridPointDistance ("Real distance between grid points", Float) = 0.5
        _Gravity ("Gravity", Float) = 9.82
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Tags { "PassName" = "WaterIncrement" }

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
            sampler2D _NoiseTex;
            float _TimeStep;
            float _RainScale;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 terrain = tex2D(_MainTex, i.uv);
                fixed4 rain = tex2D(_NoiseTex, i.uv);
                float water = terrain.g;
                water += _TimeStep * rain.r * _RainScale;
                terrain.g = water;
                return terrain;
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "FlowSimulation1" }
            
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _TerrainTex;
            float4 _MainTex_TexelSize;
            float _TimeStep;
            float _PipeArea;
            float _GridPointDistance;
            float _Gravity;

            fixed4 frag(v2f i) : SV_Target
            {
                float invSize = _MainTex_TexelSize.x;
                float leftX = saturate(i.uv.x - 1 * invSize);
                float upY = saturate(i.uv.y + 1 * invSize);
                float rightX = saturate(i.uv.x + 1 * invSize);
                float downY = saturate(i.uv.y - 1 * invSize);

                fixed4 terrain = tex2D(_TerrainTex, i.uv);
                fixed4 tNeighborL = tex2D(_TerrainTex, float2(leftX, i.uv.y));
                fixed4 tNeighborT = tex2D(_TerrainTex, float2(i.uv.x, upY));
                fixed4 tNeighborR = tex2D(_TerrainTex, float2(rightX, i.uv.y));
                fixed4 tNeighborB = tex2D(_TerrainTex, float2(i.uv.x, downY));
                float dhL = terrain.x + terrain.y - tNeighborL.x - tNeighborL.y;
                float dhT = terrain.x + terrain.y - tNeighborT.x - tNeighborT.y;
                float dhR = terrain.x + terrain.y - tNeighborR.x - tNeighborR.y;
                float dhB = terrain.x + terrain.y - tNeighborB.x - tNeighborB.y;

                fixed4 flux = tex2D(_MainTex, i.uv);
                float fL = flux.x;
                float fT = flux.y;
                float fR = flux.z;
                float fB = flux.w;

                float pipeLength = _GridPointDistance;

                fL = max(0, fL + _TimeStep * _PipeArea * (_Gravity * dhL) / pipeLength);
                fT = max(0, fT + _TimeStep * _PipeArea * (_Gravity * dhT) / pipeLength);
                fR = max(0, fR + _TimeStep * _PipeArea * (_Gravity * dhR) / pipeLength);
                fB = max(0, fB + _TimeStep * _PipeArea * (_Gravity * dhB) / pipeLength);

                float K = min(1, (terrain.y * _GridPointDistance * _GridPointDistance) / ((fL + fT + fR + fB) * _TimeStep));
                flux = K * float4(fL, fT, fR, fB);

                return flux;
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "FlowSimulation2" }
            
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "FlowSimulation3" }

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
                return col;
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "ErosionDeposition" }
            
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
                return col;
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "SedimentTransport" }
            
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }

        Pass
        {
            Tags { "PassName" = "Evaporation" }
            
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
