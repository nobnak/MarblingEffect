Shader "Hidden/Marbling" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _OffsetTex ("Offset", 2D) = "black" {}
        _BrushTex ("Brush", 2D) = "black" {}

        _Param1 ("Param 1", Vector) = (0, 0, 1, 1)
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
            float4 _MainTex_TexelSize;
            
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
                float4 uv = tex2D(_OffsetTex, i.uv);
                float4 csrc = tex2D(_MainTex, uv.xy);
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
                float2 uv = v.uv;

                float2 pos_uv = _Param1.xy + uv * _Param1.zw;
                float4 pos_ndc = float4(2.0 * pos_uv - 1.0, 0, 1);
                if (_ProjectionParams.x < 0) pos_ndc.y *= -1;

                v2f o;
                o.vertex = pos_ndc;
                o.uv = uv;
                o.clip = o.vertex;
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                float2 screen_uv = 0.5 * (i.clip.xy / i.clip.w + 1);
                if (_ProjectionParams.x < 0) screen_uv.y = 1 - screen_uv.y;

                float2 offset_uv = _Param0.xy;
                float4 brush = tex2D(_BrushTex, i.uv);
                offset_uv *= brush.x * brush.w;
                
                float2 uv = tex2D(_MainTex, screen_uv + offset_uv).xy;
                return float4(uv, 0, 1);
            }
            ENDCG
        }       
        
        // Reset
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D _BrushTex;

            v2f vert (appdata v) {
                float2 uv = v.uv;
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = uv;
                o.clip = o.vertex;
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                return float4(i.uv, 0, 1);
            }
            ENDCG
        }
    }
}
