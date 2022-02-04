Shader "Custom/Erosion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TerrainTex ("Texture", 2D) = "white" {}
        _TempTerrainTex ("Texture", 2D) = "white" {}
        _FluxTex ("Texture", 2D) = "white" {}
        _VelocityTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Texture", 2D) = "white" {}
        _TimeStep ("Time step", Float) = 0.5
        _RainScale ("Rain scale", Float) = 0.5
        _PipeArea ("Virtual pipe cross-sectional area", Float) = 0.5
        _GridPointDistance ("Real distance between grid points", Float) = 0.5
        _Gravity ("Gravity", Float) = 9.82
        _MinTilt ("Minimum tilt angle", Float) = 1.0
        _CapacityConst ("Sediment capacity constant", Float) = 1.0
        _DissolvingConst ("Dissolving constant", Float) = 1.0
        _DepositionConst ("Deposition constant", Float) = 1.0
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
                float4 terrain = tex2D(_MainTex, i.uv);
                float4 rain = tex2D(_NoiseTex, i.uv);
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
            #include "Utility.cginc"

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

                float4 terrain = tex2D(_TerrainTex, i.uv);
                float4x4 neighbors = getNeighbors(_TerrainTex, invSize, i.uv);
                float4 tNeighborL = neighbors[0];
                float4 tNeighborT = neighbors[1];
                float4 tNeighborR = neighbors[2];
                float4 tNeighborB = neighbors[3];

                float dhL = terrain.x + terrain.y - tNeighborL.x - tNeighborL.y;
                float dhT = terrain.x + terrain.y - tNeighborT.x - tNeighborT.y;
                float dhR = terrain.x + terrain.y - tNeighborR.x - tNeighborR.y;
                float dhB = terrain.x + terrain.y - tNeighborB.x - tNeighborB.y;

                float4 flux = tex2D(_MainTex, i.uv);
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
            #include "Utility.cginc"

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
            sampler2D _FluxTex;
            float4 _MainTex_TexelSize;
            float _TimeStep;
            float _GridPointDistance;

            fixed4 frag(v2f i) : SV_Target
            {
                float invSize = _MainTex_TexelSize.x;

                float4 flux = tex2D(_FluxTex, i.uv);
                float4x4 neighbors = getNeighbors(_FluxTex, invSize, i.uv);
                float4 fNeighborL = neighbors[0];
                float4 fNeighborT = neighbors[1];
                float4 fNeighborR = neighbors[2];
                float4 fNeighborB = neighbors[3];

                float fluxIn = fNeighborL.z + fNeighborT.w + fNeighborR.x + fNeighborB.y;
                float fluxOut = flux.x + flux.y + flux.z + flux.w;
                float dV = _TimeStep * (fluxIn - fluxOut);

                float4 terrain = tex2D(_MainTex, i.uv);
                terrain.y = terrain.y + dV / (_GridPointDistance * _GridPointDistance);

                return terrain;
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
            #include "Utility.cginc"

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
            sampler2D _TerrainTex;
            sampler2D _TempTerrainTex;
            sampler2D _FluxTex;
            float4 _MainTex_TexelSize;
            float _GridPointDistance;
            float _TimeStep;

            fixed4 frag (v2f i) : SV_Target
            {
                float invSize = _MainTex_TexelSize.x;

                float4 flux = tex2D(_FluxTex, i.uv);
                float4x4 neighbors = getNeighbors(_FluxTex, invSize, i.uv);
                float4 fNeighborL = neighbors[0];
                float4 fNeighborT = neighbors[1];
                float4 fNeighborR = neighbors[2];
                float4 fNeighborB = neighbors[3];

                float2 dW = 0.5 * float2(fNeighborL.z - flux.x + flux.z - fNeighborR.x, fNeighborB.y - flux.w + flux.y - fNeighborT.w);

                float2 velocity = (tex2D(_MainTex, i.uv).xy - 0.5) * 2;

                float water1 = tex2D(_TerrainTex, i.uv).y;
                float water2 = tex2D(_TempTerrainTex, i.uv).y;
                float dBar = (water1 + water2) * 0.5;
                
                velocity += _TimeStep * (dW / (_GridPointDistance * dBar));

                return float4((velocity.xy + 1) * 0.5, 0.0, 1.0);
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
            #include "Utility.cginc"

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
            sampler2D _VelocityTex;
            float4 _MainTex_TexelSize;
            float _CapacityConst;
            float _MinTilt;
            float _DissolvingConst;
            float _DepositionConst;

            fixed4 frag (v2f i) : SV_Target
            {
                float invSize = _MainTex_TexelSize.x;

                float3 terrain = tex2D(_MainTex, i.uv).xyz;
                float4x4 neighbors = getNeighbors(_MainTex, invSize, i.uv);
                float4 tNeighborL = neighbors[0];
                float4 tNeighborT = neighbors[1];
                float4 tNeighborR = neighbors[2];
                float4 tNeighborB = neighbors[3];

                float speed = length((tex2D(_VelocityTex, i.uv).xy - 0.5) * 2);

                float3 grad = normalize(float3(tNeighborR.x - tNeighborL.x, tNeighborT.x - tNeighborB.x, 0.1));
                float3 up = float3(0, 0, 1);
                float tiltAngle = max(radians(_MinTilt), acos(dot(grad, up)));
                float capacity = _CapacityConst * sin(tiltAngle) * speed;
                
                // If under capacity (water can pick up more), dissolve ground into suspended sediment.
                // Otherwise, over capacity (water cannot hold this much), so drop some suspended sediment onto the ground.
                float overCapacity = capacity <= terrain.z ? 1 : 0;
                terrain.x = terrain.x - _DissolvingConst * (capacity - terrain.z) * (1 - overCapacity);
                terrain.z = terrain.z + _DissolvingConst * (capacity - terrain.z) * (1 - overCapacity);

                terrain.x = terrain.x + _DepositionConst * (capacity - terrain.z) * overCapacity;
                terrain.z = terrain.z - _DepositionConst * (capacity - terrain.z) * overCapacity;

                return float4(terrain.xyz, 1.0);
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
                float4 col = tex2D(_MainTex, i.uv);
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
                float4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
