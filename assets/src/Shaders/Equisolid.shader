Shader "Custom/Equisolid" {
    Properties {
        _MainTex ("Cubemap", CUBE) = "" {}
        _FOV ("Field of View (deg)", Range(1, 359)) = 90
        _SrcBlend ("Src Blend", Int) = 1 // BlendMode.One
        _DstBlend ("Dst Blend", Int) = 0 // BlendMode.Zero
    }

    SubShader {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull Off
        ZTest Always
        ZWrite Off
        Lighting Off
        Fog { Mode Off }
        Blend [_SrcBlend] [_DstBlend]

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

                // https://en.wikipedia.org/wiki/Lambert_azimuthal_equal-area_projection#Definition
                float3 invEquisolid(float2 p) {
                    float r2 = dot(p, p);
                    return normalize(float3(
                        sqrt(1 - r2/4)*p.x,
                        sqrt(1 - r2/4)*p.y,
                        // Inverted sign to flip the handedness
                        1 - r2/2
                    ));
                }
            
                v2f vert (appdata v) {
                    v2f o;
                    o.pos = float4(v.vertex.xy, 0, 1);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target {
                    float2 p = i.uv * 2.0 - 1.0;
                    p.x *= _ScreenParams.x / _ScreenParams.y;
                    // r = 2fsin(θ/2), scaling is just matching the r function
                    p *= 2*sin(radians(_FOV) / 4);

                    float3 dir = invEquisolid(p);
                    return texCUBE(_MainTex, dir);
                }
            ENDCG
        }
    }
}
