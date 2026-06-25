Shader "Custom/Panini" {
Properties {
    _MainTex ("Cubemap", CUBE) = "" {}
    _FOV ("Field of view (deg)", Range(1,359)) = 160
    _D ("Distance", Range(0.0,1.0)) = 1.0
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
            float _FOV; // Horizontal field of view in degrees
            float _D; // The distance constant

            // Takes pre-scaled point and maps it to azimuth and altitude
            float2 MapToCylinder(float h, float v)
            {
                // Map distance constant and point parameter to shorter names
                float d = _D;
                
                float k = (h*h) / ((d+1)*(d+1));
                float delta = k*k*d*d - (k+1)*(k*d*d - 1);

                float cosphi = (-k*d + sqrt(delta)) / (k+1);
                float S = (d+1) / (d + cosphi);

                float phi   = atan2(h, S*cosphi);
                float theta = atan2(v, S);

                return float2(
                    phi,
                    theta
                );
            }

            // Maps the cylindrical coordinate to a direction on the unit-sphere
            float3 MapToSphere(float2 c)
            {
                float3 dir;
                dir.x = cos(c.y)*sin(c.x);
                dir.y = sin(c.y);
                dir.z = cos(c.y)*cos(c.x);
                return normalize(dir);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = float4(v.vertex.xy, 0, 1);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_TARGET
            {
                // Map point to centered view plane coordinates
                float2 p = i.uv * 2.0 - 1.0;
                // HOR+ aspect ratio scaling
                p.x *= _ScreenParams.x / _ScreenParams.y;

                // According to the panini projection paper, sin(phi)*(d+1)/(d+cos(phi)) computes x/K
                // unlike other projections which compute r; the distance regardless of axis.
                // I'm going to roll with it and as a result the fov value is going to be
                // completely meaningless, however if you input it in the axis converter the horizontal
                // fov should be correct I believe somewhat
                float phi = radians(_FOV) * 0.5;
                float scale;
                if (_D == 1) {
                    scale = tan(phi);
                } else {
                    scale = sin(phi)*(_D+1)/(_D+cos(phi));
                }

                float h = p.x * scale;
                float v = p.y * scale;

                float2 c = MapToCylinder(h, v);
                float3 dir = MapToSphere(c);

                return texCUBE(_MainTex, dir);
            }
        ENDCG
    }
}

}
