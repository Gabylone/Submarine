Shader "Custom/SphereMask" {
    Properties {
        _Center ("Center", Vector) = (0,0,0,0)
        _Radius ("Radius", Float) = 1.0
        _Color ("Color", Color) = (1,1,1,1)
        _Alpha ("Alpha", Range(0,1)) = 0.5
        _Emission ("Emission", Color) = (0,0,0,0)
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 1.0
        _NoiseStrength ("Noise Strength", Float) = 0.1
        _Transparency ("Transparency", Range(0,1)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            float4 _Center;
            float _Radius;
            float4 _Color;
            float _Alpha;
            float4 _Emission;
            sampler2D _NoiseTexture;
            float _NoiseScale;
            float _NoiseStrength;
            float _Transparency;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                float dist = distance(i.worldPos, _Center.xyz);
                if (dist > _Radius) {
                    discard;
                }
                
                float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                float3 normal = normalize(i.normal);
                float diffuse = max(0.0, dot(normal, viewDir));

                half4 emissionColor = _Emission;

                half4 finalColor = half4(_Color.rgb * diffuse, _Alpha) + emissionColor;

                float2 noiseUV = i.worldPos.xy * _NoiseScale;
                float noiseValue = tex2D(_NoiseTexture, noiseUV).r * _NoiseStrength;

                finalColor.rgb += noiseValue;
                finalColor.a *= _Transparency;

                return finalColor;
            }
            ENDCG
        }
    }
}
