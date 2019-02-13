Shader "VolumeRendering/DirectVolumeRenderingShader"
{
	Properties
	{
		_DataTex ("Data Texture (Generated)", 3D) = "" {}
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _TFTex("Transfer Function Texture (Generated)", 2D) = "white" {}
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
    }
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
            #pragma multi_compile MODE_DVR MODE_MIP MODE_SURF
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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexLocal : TEXCOORD1;
                float3 normal : NORMAL;
                //float3 screenPos : TEXCOORD1;
			};

			sampler3D _DataTex;
            sampler2D _NoiseTex;
            sampler2D _TFTex;

            float _MinVal;
            float _MaxVal;

            float3 getGradient(float3 pos, float gradientStep)
            {
                float3 stepX = float3(gradientStep, 0.0f, 0.0f);
                float3 stepY = float3(0.0f, gradientStep, 0.0f);
                float3 stepZ = float3(0.0f, 0.0f, gradientStep);

                float x1 = tex3Dlod(_DataTex, float4(pos + stepX, 0.0f)).x;
                float x2 = tex3Dlod(_DataTex, float4(pos - stepX, 0.0f)).x;
                float y1 = tex3Dlod(_DataTex, float4(pos + stepY, 0.0f)).x;
                float y2 = tex3Dlod(_DataTex, float4(pos - stepY, 0.0f)).x;
                float z1 = tex3Dlod(_DataTex, float4(pos + stepZ, 0.0f)).x;
                float z2 = tex3Dlod(_DataTex, float4(pos - stepZ, 0.0f)).x;
                return float3(x2 - x1, y2 - y1, z2 - z1);
            }

			v2f vert_main (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                //o.screenPos = ComputeScreenPos(o.vertex);
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag_dvr (v2f i)
			{
                #define NUM_STEPS 2048//200
                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float4 col = float4(i.vertexLocal.x, i.vertexLocal.y, i.vertexLocal.z, 1.0f);

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                // Create a small random offset in order to remove artifacts
                rayStartPos = rayStartPos + (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;

                col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                //[unroll(NUM_STEPS)]
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize + stepSize * 0.5f;
                    const float3 currPos = rayStartPos + rayDir * t;
                    if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        break;
                    
                    const float density = tex3Dlod(_DataTex, float4(currPos.x, currPos.y, currPos.z, 0.0f)).r / 4095.0f;

                    float4 src = tex2Dlod(_TFTex, float4(density, 0.0f, 0.0f, 0.0f));// tex2D(_TFTex, float2(density, 0.0f));

                    if (density < _MinVal || density > _MaxVal)
                        src.a = 0.0f;

                    col.rgb = src.a * src.rgb + (1.0f - src.a)*col.rgb;
                    col.a = src.a + (1.0f - src.a)*col.a;

                    if (col.a > 1.0f) break;
                }
                
                col.rgb = col.rgb;

                return col;
			}

            fixed4 frag_mip(v2f i)
            {
                #define NUM_STEPS 1024//200
                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                float maxDensity = 0.0f;
                //[unroll(NUM_STEPS)]
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize + stepSize * 0.5f;
                    const float3 currPos = rayStartPos + rayDir * t;
                    if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        break;

                    const float density = tex3Dlod(_DataTex, float4(currPos.x, currPos.y, currPos.z, 0.0f)).r / 4095.0f;
                    maxDensity = max(density, maxDensity);
                }
                // Maximum intensity projection
                float4 col = float4(maxDensity, 0.0f, 0.0f, maxDensity);

                return col;
            }

            fixed4 frag_surf(v2f i)
            {
                #define NUM_STEPS 2048//200
                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                // Create a small random offset in order to remove artifacts
                rayStartPos = rayStartPos + (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;

                float lastDensity = 0.0f;
                float lastT = 0.0f;
                //[unroll(NUM_STEPS)]
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize + stepSize * 0.5f;
                    const float3 currPos = rayStartPos + rayDir * t;
                    if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        break;

                    const float density = tex3Dlod(_DataTex, float4(currPos.x, currPos.y, currPos.z, 0.0f)).r / 4095.0f;
                    if (density > max(_MinVal, 0.1f)/*TODO*/ && density < _MaxVal) // TEMP TEST
                    {
                        lastDensity = density;
                        lastT = t;
                    }
                }

                float3 normal = getGradient(rayStartPos + rayDir * lastT, stepSize);
                normal = normalize(normal);
                //float lightReflection = dot(normal, normalize(float3(0.1f, 0.1f, 0.1f)));
                float lightReflection = dot(normal, rayDir);
                lightReflection = lerp(0.25f, 1.0f, lightReflection);
                
                // Maximum intensity projection
                float4 col = float4(lastDensity, 0.0f, 0.0f, lastDensity > 0.1f ? 1.0f : 0.0f);
                col = lightReflection * tex2Dlod(_TFTex, float4(lastDensity, 0.0f, 0.0f, 0.0f));
                col.a = lastDensity > 0.1f ? 1.0f : 0.0f;

                return col;
            }

            v2f vert(appdata v)
            {
                return vert_main(v);
            }

            fixed4 frag(v2f i) : SV_Target
            {
#if MODE_DVR
                return frag_dvr(i);
#elif MODE_MIP
                return frag_mip(i);
#elif MODE_SURF
                return frag_surf(i);
#endif
            }

			ENDCG
		}
	}
}
