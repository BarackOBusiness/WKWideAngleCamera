Shader "Custom/Stereographic" {
    Properties {
        _MainTex ("Cubemap", CUBE) = "" {}
        _FOV ("Field of View (deg)", Range(1,359)) = 145
    }

    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Cull Off
        ZTest Always
        ZWrite Off
        Lighting Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

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

                // South-pole plane orientation instead of equatorial:
                // https://en.wikipedia.org/wiki/Stereographic_projection#Other_conventions
                float3 MapToSphere(float2 p)
                {
                    float r2 = dot(p, p);
                    float3 dir;
                    dir.x = p.x / (1 + r2);
                    dir.y = p.y / (1 + r2);
                    // Inverted the signs of only the numerator to flip the handedness
                    dir.z = (1 - r2) / (2 + 2*r2);
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
                    // Point to centered plane coordinates
                    float2 p = i.uv * 2.0 - 1.0;
                    // HOR+ aspect ratio scaling
                    p.x *= _ScreenParams.x / _ScreenParams.y;
                    // Scale to FOV
                    p *= tan(radians(_FOV) * 0.25);

                    float3 dir = MapToSphere(p);
                    return texCUBE(_MainTex, dir);
                }
            ENDCG
        }
    }
}
