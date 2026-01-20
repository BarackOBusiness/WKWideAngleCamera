Shader "Unlit/StereographicProjection"
{
    Properties
    {
        _Cube ("Cubemap", CUBE) = "" {}
        _FOV ("Field of View (deg)", Range(1, 360)) = 140
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always
        Lighting Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _Cube;
            float _FOV; // in degrees

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Map [0,1] UV to centered [-1,1] plane coordinates
            float2 UVToPlane(float2 uv)
            {
                float2 p = uv * 2.0 - 1.0; // [-1,1]
                return p;
            }

            // Stereographic projection from plane (x,y) to unit sphere direction
            float3 StereographicToDir(float2 p)
            {
                float r2 = p.x * p.x + p.y * p.y;
                float denom = 1.0 + r2;
                float3 dir;
                dir.x = 2.0 * p.x / denom;
                dir.y = 2.0 * p.y / denom;
                dir.z = (r2 - 1.0) / denom;
                dir.z = -dir.z;
                return normalize(dir);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Get camera aspect ratio for correction
                float aspect = _ScreenParams.x / _ScreenParams.y;  // width/height
    
                // Centered plane coordinates, corrected against vertical squishing by aspect ratio
                float2 p = UVToPlane(i.uv);
                p.x *= aspect;
    
                // Apply FOV scaling to the CORRECTED coordinates
                float fovRad = radians(_FOV / aspect * 0.25);
                float scale = tan(fovRad);
                p *= scale;

                float3 dir = StereographicToDir(p);
                dir.x = -dir.x;
    
                fixed4 col = texCUBE(_Cube, dir);
                return col;
            }
            ENDCG
        }
    }

    FallBack Off
}
