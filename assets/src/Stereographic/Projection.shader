Shader "Custom/Stereographic" {
Properties {
    _MainTex ("Cubemap", CUBE) = "" {}
    _FOV ("Field of view (deg)", Range(1, 359)) = 160
}

SubShader {
    Tags { "RenderType" = "Opaque" }
    Cull Off
    ZTest Always
    ZWrite Off
    Lighting Off
    Fog { Mode Off }

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            samplerCUBE _MainTex;
            float _FOV;

            float3 MapToSphere(float2 p)
            {
                float r2 = dot(p, p);
                float denom = 1 + r2;
                float3 dir;
                dir.x = 2.0 * p.x / denom;
                dir.y = 2.0 * p.y / denom;
                dir.z = (r2 - 1.0) / denom;
                return normalize(dir);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = float4(v.vertex.xy, 0, 1);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Convert the point on screen into centered plane coordinates
                float2 p = i.uv * 2.0 - 1.0;
                float aspect = _ScreenParams.x / _ScreenParams.y;

                float fovRad = radians(_FOV);
                float scale = tan(fovRad * 0.25);
                // Field of view setting scales the greater axis of the display
                if (aspect > 1.0) {
                    p.y /= aspect;
                } else {
                    p.x /= aspect;
                }
                p *= scale;

                float3 dir = MapToSphere(p);
                return texCUBE(_MainTex, dir);
            }
        ENDCG
    }
}

}
