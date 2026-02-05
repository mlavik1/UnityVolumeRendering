Shader "VolumeRendering/URP/CrossSectionSphere" {
    Properties{
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimThickness("Rim Thickness", Range(0, 1)) = 0.1
    }

        SubShader
        {
            PackageRequirements { "com.unity.render-pipelines.universal" }
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
            LOD 100

            Pass
            {
                Blend SrcAlpha OneMinusSrcAlpha

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                float _RimThickness;
                float4 _RimColor;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float3 worldNormal : TEXCOORD0;
                    float3 worldPos : TEXCOORD1;
                    float4 vertex : SV_POSITION;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                    o.vertex = TransformWorldToHClip(worldPos);
                    o.worldNormal = TransformObjectToWorldNormal(v.normal);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    return o;
                }

                half4 frag(v2f i) : SV_Target
                {
                    // Calculate the rim effect
                    float3 viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos);
                    float rim = 1.0 - saturate(dot(viewDirection, i.worldNormal));
                    rim = (rim >= (1.0 - _RimThickness)) ? 1.0 : 0.0;

                    half4 col;
                    col.rgb = _RimColor.rgb * rim;
                    col.a = rim * _RimColor.a;

                    return col;
                }
                ENDHLSL
            }
    }
        FallBack "Diffuse"
}