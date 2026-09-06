Shader "Custom/Mercator" {
    Properties {
        _MainTex ("Cubemap", CUBE) = "" {}
        _FOV ("Field of View (deg)", Range(1, 359)) = 135
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

                float arsinh(float x) {
                    return log(x + sqrt(x*x + 1));
                }
                
                // https://en.wikipedia.org/wiki/Mercator_projection#Inverse_transformations
                float2 invMercator(float2 p) {
                    // Mercator's y axis is a relatively arbitrary function of distance latitude
                    // we could omit the atan and hyperbolic sine to produce an equirectangular plate carrée
                    float lat = atan(sinh(p.y));
                    // Mercator's x axis maps directly to longitude
                    float lon = p.x;
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
                    // Fuck it HOR+ can stay
                    p.x *= _ScreenParams.x / _ScreenParams.y;
                    p *= arsinh(tan(radians(_FOV) / 2));

                    float2 globe = invMercator(p);
                    float3 dir = GeoToCartesian(globe.y, globe.x);
                    return texCUBE(_MainTex, dir);
                }
            ENDCG
        }
    }
}
