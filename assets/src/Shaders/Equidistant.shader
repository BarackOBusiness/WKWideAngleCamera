Shader "Custom/Equidistant" {
    Properties {
        _MainTex ("Cubemap", CUBE) = "" {}
        _FOV ("Field of view (deg)", Range(1, 359)) = 145
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

                // Let's go over what we know:
                // [REDACTED OLD NOTE TO MYSELF]
                // okay so basically just https://mathworld.wolfram.com/AzimuthalEquidistantProjection.html
                // convert latitude and longitude to cartesian coordinates got it
                // (this likely has some numerical instabilities at ±90deg latitude because
                // I'm too lazy to implement the piecewise longitude inverse)
                float3 CoordToSphere(float2 p)
                {
                    // Distance of point to the center, which is the axis we're looking: 0,0
                    float c = sqrt(dot(p, p));
                    // Latitude and longitude on the "globe", haha we're not cartographing here.
                    float lat = asin(p.y*sin(c)/c);
                    float long = atan2(p.x*sin(c), c*cos(c));
                    // Now we need that in unit sphere terms
                    float3 dir;
                    dir.x = cos(lat) * sin(long);
                    dir.y = sin(lat);
                    dir.z = cos(lat) * cos(long);
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
                    // HOR+ aspect ratio scaling
                    p.x *= _ScreenParams.x / _ScreenParams.y;
                    // Scale to FOV, I straight logic'd this one out myself
                    p *= radians(_FOV) / 2;

                    float3 dir = CoordToSphere(p);
                    return texCUBE(_MainTex, dir);
                }
            ENDCG
        }
    }
}
