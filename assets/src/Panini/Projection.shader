Shader "Custom/Panini" {
Properties {
    _MainTex ("Cubemap", CUBE) = "" {}
    _FOV ("Field of view (deg)", Range(1,359)) = 140
    _D ("Distance", Range(0.0,1.0)) = 1.0
}

SubShader {
    Tags { "RenderType" = "Opaque" }

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
            float2 MapToCylinder (float h, float v)
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
            float3 MapToSphere (float2 c)
            {
                float3 dir;
                dir.x = -(cos(c.y)*sin(c.x));
                dir.y = sin(c.y);
                dir.z = cos(c.y)*cos(c.x);
                return dir;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_TARGET
            {
                // Map point to centered view plane coordinates
                float2 p = i.uv * 2 - 1;
                float aspect = _ScreenParams.x / _ScreenParams.y;

                // Compute edges of view into scaling factors
                float phi_edge = radians(_FOV);
                float h_edge;
                if (_D == 1) {
                    h_edge = 2*tan(phi_edge * 0.25);
                } else {
                    h_edge = sin(phi_edge)*(_D+1)/(_D+cos(phi_edge));
                }

                float h = p.x * h_edge;
                float v = p.y * h_edge / aspect;

                float2 c = MapToCylinder(h, v);
                float3 dir = MapToSphere(c);

                return texCUBE(_MainTex, dir);
            }
        ENDCG
    }
}

}
