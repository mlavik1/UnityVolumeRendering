Shader "Custom/RimOutlineShader" {
    Properties{
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimThickness("Rim Thickness", Range(0, 1)) = 0.1
    }

        SubShader{
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100

            Pass {
                Blend SrcAlpha OneMinusSrcAlpha

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                float _RimThickness;
                float4 _RimColor;

                struct appdata {
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f {
                    UNITY_VERTEX_OUTPUT_STEREO
                    float3 worldNormal : TEXCOORD0;
                    float3 worldPos : TEXCOORD1;
                    float4 vertex : SV_POSITION;
                };

                v2f vert(appdata v) {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.worldNormal = UnityObjectToWorldNormal(v.normal);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target{
                    // Calculate the rim effect
                    float3 viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos);
                    float rim = 1.0 - saturate(dot(viewDirection, i.worldNormal));
                    rim = (rim >= (1.0 - _RimThickness)) ? 1.0 : 0.0;

                    float4 col;
                    col.rgb = _RimColor.rgb * rim;
                    col.a = rim * _RimColor.a;

                    return col;
                }
                ENDCG
            }
    }
        FallBack "Diffuse"
}