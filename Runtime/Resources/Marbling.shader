Shader "Hidden/Marbling" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _OffsetTex ("Offset", 2D) = "black" {}
        _BrushTex ("Brush", 2D) = "black" {}
    }
    SubShader {
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 clip : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            
            float4 _Param0;
            float4 _Param1;
        ENDCG

        // Render
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _OffsetTex;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.clip = o.vertex;
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                float4 coffset = tex2D(_OffsetTex, i.uv);
                float4 csrc = tex2D(_MainTex, i.uv + coffset.xy);
                return csrc;
            }
            ENDCG
        }

        // Add
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D _BrushTex;

            v2f vert (appdata v) {
                float2 pos_uv = dot(_Param1, float4(v.uv.xy, 1, 1));
                float3 pos_ndc = float3(2.0 * pos_uv - 1.0, 0);

                v2f o;
                o.vertex = float4(pos_ndc, 1);
                o.uv = v.uv;
                o.clip = o.vertex;
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                float2 screen_uv = i.clip.xy / i.clip.w;
                float2 offset_uv_add = _Param0.xy;
                float4 brush = tex2D(_BrushTex, i.uv);
                float4 offset_uv = tex2D(_MainTex, screen_uv);
                offset_uv.xy += offset_uv_add;
                return offset_uv;
            }
            ENDCG
        }
    }
}
