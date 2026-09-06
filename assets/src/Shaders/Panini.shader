Shader "Custom/Panini" {
    Properties {
        _MainTex ("Cubemap", CUBE) = "" {}
        _D ("Distance", Range(0.0,1.0)) = 1.0
        _FOV ("Field of View (deg)", Range(1,359)) = 135
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
                #include "WideAngle.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv     : TEXCOORD0;
                };

                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv  : TEXCOORD0;
                };

                samplerCUBE _MainTex;
                float _FOV; // Horizontal field of view in degrees
                float _D; // The distance constant

                // http://tksharpless.net/vedutismo/Pannini/cae_paper1024.pdf
                // 4.1 -> inverse horizontal mapping
                float2 invPanini(float h, float v) {
                    // Map distance constant to shorter name and to match paper
                    float d = _D;
                
                    float k = (h*h) / ((d+1)*(d+1));
                    float delta = k*k*d*d - (k+1)*(k*d*d - 1);

                    float cosphi = (-k*d + sqrt(delta)) / (k+1);
                    float S = (d+1) / (d + cosphi);

                    float phi   = atan2(h, S*cosphi);
                    float theta = atan2(v, S);

                    return float2( phi, theta );
                }

                v2f vert (appdata v) {
                    v2f o;
                    o.pos = float4(v.vertex.xy, 0, 1);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag (v2f i) : SV_TARGET {
                    float2 p = i.uv * 2.0 - 1.0;
                    p.x *= _ScreenParams.x / _ScreenParams.y;

                    // According to the panini projection paper, sin(phi)*(d+1)/(d+cos(phi)) computes x/K
                    // unlike other projections which compute r; the distance regardless of axis.
                    // I'm going to roll with it and as a result the fov value is going to be
                    // completely meaningless, however if you input it in the axis converter the horizontal
                    // fov should be correct I believe somewhat
                    float phi = radians(_FOV) * 0.5;
                    float scale = sin(phi)*(_D+1)/(_D+cos(phi));

                    float h = p.x * scale;
                    float v = p.y * scale;

                    float2 c = invPanini(h, v);
                    float3 dir = GeoToCartesian(c.y, c.x);
                    return texCUBE(_MainTex, dir);
                }
            ENDCG
        }
    }
}
