Shader "VolumeRendering/DirectVolumeRenderingShader"
{
	Properties
	{
		_MainTex ("Data", 3D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
                float3 vertexLocal : TEXCOORD1;
                float3 normal : NORMAL;
			};

			sampler3D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertexLocal = v.vertex;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                #define NUM_STEPS 20

                float4 col = float4(i.vertexLocal.x, i.vertexLocal.y, i.vertexLocal.z, 1.0f);

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float maxDensity = 0.0f;
                [unroll]
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep / (NUM_STEPS - 1.0f);
                    const float3 currPos = rayStartPos + rayDir * t;
                    if (currPos.x < 0.0f || currPos.x > 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        break;
                    
                    float dataValue = tex3D(_MainTex, currPos).r / 4095.0f;
                    if (dataValue > maxDensity)
                        maxDensity = dataValue;

                    float4 src = float4(dataValue, dataValue, dataValue, dataValue);
                    
                    col.rgb = src.a * src.rgb + (1.0f - src.a)*col.rgb;
                    col.a = src.a + (1.0f - src.a)*col.a;
                    
                    if (col.a > 1.0f) break;
                }
                // Maximum intensity projection
                //col = float4(maxDensity, 0.0f, 0.0f, 1.0f);

                return col;
			}
			ENDCG
		}
	}
}
