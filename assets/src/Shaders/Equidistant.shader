Shader "Custom/Equidistant" {
    Properties {
        _MainTex ("Cubemap", CUBE) = "" {}
        _FOV ("Field of View (deg)", Range(1, 359)) = 120
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
                float _FOV;

                // https://mathworld.wolfram.com/AzimuthalEquidistantProjection.html
                // (this likely has some numerical instabilities at ±90deg latitude because
                // I'm too lazy to implement the piecewise longitude inverse)
                float2 invEquidistant(float2 p) {
                    // Distance of point to the center, which is the axis we're looking: 0,0
                    float c = sqrt(dot(p, p));
                    float lat = asin(p.y*sin(c)/c);
                    float lon = atan2(p.x*sin(c), c*cos(c));
                    return float2( lon, lat );
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
                    p *= radians(_FOV) / 2;

                    float2 globe = invEquidistant(p);
                    // Perform another mapping between latlon coordinates and position
                    // on sphere in cartesian coordinates
                    float3 dir = GeoToCartesian(globe.y, globe.x);
                    return texCUBE(_MainTex, dir);
                }
            ENDCG
        }
    }
}
