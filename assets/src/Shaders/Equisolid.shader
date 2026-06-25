Shader "Custom/Equisolid" {
    Properties {
        _MainTex ("Cubemap", CUBE) = "" {}
        _FOV ("Field of view (deg)", Range(1, 359)) = 160
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

                // https://en.wikipedia.org/wiki/Lambert_azimuthal_equal-area_projection#Definition
                float3 DiskToSphere(float2 p)
                {
                    float r2 = dot(p, p);
                    float3 dir;
                    dir.x = sqrt(1 - (r2/4))*p.x;
                    dir.y = sqrt(1 - (r2/4))*p.y;
                    // Inverted the sign of each operand to flip the handedness
                    dir.z = (1 - (r2/2));
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
                    // Screen point to centered plane coordinates
                    float2 p = i.uv * 2.0 - 1.0;
                    // HOR+ aspect ratio scaling
                    p.x *= _ScreenParams.x / _ScreenParams.y;
                    // r = 2fsin(θ/2), z=0 on sphere is r=√2
                    // yeah it literally is just fitting the fov range into the disk
                    p *= 2*sin(radians(_FOV) / 4);

                    float3 dir = DiskToSphere(p);
                    return texCUBE(_MainTex, dir);
                }
            ENDCG
        }
    }
}
