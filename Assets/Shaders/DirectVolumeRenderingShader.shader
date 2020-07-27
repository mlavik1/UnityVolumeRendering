Shader "VolumeRendering/DirectVolumeRenderingShader"
{
    Properties
    {
        _DataTex ("Data Texture (Generated)", 3D) = "" {}
        _GradientTex("Gradient Texture (Generated)", 3D) = "" {}
        _NoiseTex("Noise Texture (Generated)", 2D) = "white" {}
        _TFTex("Transfer Function Texture (Generated)", 2D) = "" {}
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Cull Front
        ZTest LEqual
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma multi_compile MODE_DVR MODE_MIP MODE_SURF
            #pragma multi_compile __ TF2D_ON
            #pragma multi_compile __ SLICEPLANE_ON
            #pragma multi_compile __ LIGHTING_ON
            #pragma multi_compile DEPTHWRITE_ON DEPTHWRITE_OFF
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct vert_in
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexLocal : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct frag_out
            {
                float4 colour : SV_TARGET;
#if DEPTHWRITE_ON
                float depth : SV_DEPTH;
#endif
            };

            sampler3D _DataTex;
            sampler3D _GradientTex;
            sampler2D _NoiseTex;
            sampler2D _TFTex;

            float _MinVal;
            float _MaxVal;

#if SLICEPLANE_ON
            float3 _PlanePos;
            float3 _PlaneNormal;
#endif

            // Gets the colour from a 1D Transfer Function (x = density)
            float4 getTF1DColour(float density)
            {
                return tex2Dlod(_TFTex, float4(density, 0.0f, 0.0f, 0.0f));
            }

            // Gets the colour from a 2D Transfer Function (x = density, y = gradient magnitude)
            float4 getTF2DColour(float density, float gradientMagnitude)
            {
                return tex2Dlod(_TFTex, float4(density, gradientMagnitude, 0.0f, 0.0f));
            }

            // Gets the density at the specified position
            float getDensity(float3 pos)
            {
                return tex3Dlod(_DataTex, float4(pos.x, pos.y, pos.z, 0.0f));
            }

            // Gets the gradient at the specified position
            float3 getGradient(float3 pos)
            {
                return tex3Dlod(_GradientTex, float4(pos.x, pos.y, pos.z, 0.0f)).rgb;
            }

            // Performs lighting calculations, and returns a modified colour.
            float3 calculateLighting(float3 col, float3 normal, float3 lightDir, float3 eyeDir, float specularIntensity)
            {
                float ndotl = max(lerp(0.0f, 1.5f, dot(normal, lightDir)), 0.5f); // modified, to avoid volume becoming too dark
                float3 diffuse = ndotl * col;
                float3 v = eyeDir;
                float3 r = normalize(reflect(-lightDir, normal));
                float rdotv = max( dot( r, v ), 0.0 );
                float3 specular = pow(rdotv, 32.0f) * float3(1.0f, 1.0f, 1.0f) * specularIntensity;
                return diffuse + specular;
            }

            // Converts local position to depth value
            float localToDepth(float3 localPos)
            {
                float4 clipPos = UnityObjectToClipPos(float4(localPos, 1.0f));

#if defined(SHADER_API_GLCORE) || defined(SHADER_API_OPENGL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
                return (clipPos.z / clipPos.w) * 0.5 + 0.5;
#else
                return clipPos.z / clipPos.w;
#endif
            }

            bool isSliceCulled(float3 currPos)
            {
#if SLICEPLANE_ON
                // Move the reference in the middle of the mesh, like the pivot
                float3 pivotPos = currPos - float3(0.5f, 0.5f, 0.5f);

                // Convert to world position
                float3 pivotWorldPos = mul(unity_ObjectToWorld, -pivotPos);

                // If the dot product is < 0, the current position is "below" the plane, if it's > 0 it's "above"
                // Then cull if the current position is below
                float cull = dot(_PlaneNormal, pivotWorldPos - _PlanePos);
                return cull < 0;
#else
                return false;
#endif
            }

            frag_in vert_main (vert_in v)
            {
                frag_in o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            // Direct Volume Rendering
            frag_out frag_dvr (frag_in i)
            {
                #define NUM_STEPS 512

                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                // Create a small random offset in order to remove artifacts
                rayStartPos = rayStartPos + (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;

                float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
                uint iDepth = 0;
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = rayStartPos + rayDir * t;
                    if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        break;

                    // Perform slice culling (cross section plane)
#ifdef SLICEPLANE_ON
                    if(isSliceCulled(currPos))
                    	break;
#endif

                    // Get the dansity/sample value of the current position
                    const float density = getDensity(currPos);

                    // Calculate gradient (needed for lighting and 2D transfer functions)
#if defined(TF2D_ON) || defined(LIGHTING_ON)
                    float3 gradient = getGradient(currPos);
#endif

                    // Apply transfer function
#if TF2D_ON
                    float mag = length(gradient) / 1.75f;
                    float4 src = getTF2DColour(density, mag);
#else
                    float4 src = getTF1DColour(density);
#endif

                    // Apply lighting
#ifdef LIGHTING_ON
                    src.rgb = calculateLighting(src.rgb, normalize(gradient), lightDir, rayDir, 0.3f);
#endif

                    if (density < _MinVal || density > _MaxVal)
                        src.a = 0.0f;

                    col.rgb = src.a * src.rgb + (1.0f - src.a)*col.rgb;
                    col.a = src.a + (1.0f - src.a)*col.a;

                    if (src.a > 0.15f)
                        iDepth = iStep;

                    if (col.a > 1.0f)
                        break;
                }

                // Write fragment output
                frag_out output;
                output.colour = col;
#if DEPTHWRITE_ON
                if (iDepth != 0)
                    output.depth = localToDepth(rayStartPos + rayDir * (iDepth * stepSize) - float3(0.5f, 0.5f, 0.5f));
                else
                    output.depth = 0;
#endif
                return output;
            }

            // Maximum Intensity Projection mode
            frag_out frag_mip(frag_in i)
            {
                #define NUM_STEPS 512
                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                float maxDensity = 0.0f;
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = rayStartPos + rayDir * t;
                    // Stop when we are outside the box
                    if (currPos.x < -0.0001f || currPos.x >= 1.0001f || currPos.y < -0.0001f || currPos.y > 1.0001f || currPos.z < -0.0001f || currPos.z > 1.0001f) // TODO: avoid branch?
                        break;

#ifdef SLICEPLANE_ON
                    if (isSliceCulled(currPos))
                        break;
#endif

                    const float density = getDensity(currPos);
                    if (density > _MinVal && density < _MaxVal)
                        maxDensity = max(density, maxDensity);
                }

                // Write fragment output
                frag_out output;
                output.colour = float4(1.0f, 1.0f, 1.0f, maxDensity); // maximum intensity
#if DEPTHWRITE_ON
                output.depth = localToDepth(i.vertexLocal);
#endif
                return output;
            }

            // Surface rendering mode
            // Draws the first point (closest to camera) with a density within the user-defined thresholds.
            frag_out frag_surf(frag_in i)
            {
#define NUM_STEPS 1024
                const float stepSize = 1.732f/*greatest distance in box*/ / NUM_STEPS;

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = normalize(ObjSpaceViewDir(float4(i.vertexLocal, 0.0f)));
                // Start from the end, tand trace towards the vertex
                rayStartPos += rayDir * stepSize * NUM_STEPS;
                rayDir = -rayDir;

                // Create a small random offset in order to remove artifacts
                rayStartPos = rayStartPos + (2.0f * rayDir / NUM_STEPS) * tex2D(_NoiseTex, float2(i.uv.x, i.uv.y)).r;

                float4 col = float4(0,0,0,0);
                for (uint iStep = 0; iStep < NUM_STEPS; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = rayStartPos + rayDir * t;
                    // Make sure we are inside the box
                    if (currPos.x < 0.0f || currPos.x >= 1.0f || currPos.y < 0.0f || currPos.y > 1.0f || currPos.z < 0.0f || currPos.z > 1.0f) // TODO: avoid branch?
                        continue;

#ifdef SLICEPLANE_ON
                    if (isSliceCulled(currPos))
                        continue;
#endif

                    const float density = getDensity(currPos);
                    if (density > _MinVal && density < _MaxVal)
                    {
                        float3 normal = normalize(getGradient(currPos));
                        col = getTF1DColour(density);
                        col.rgb = calculateLighting(col.rgb, normal, -rayDir, -rayDir, 0.15);
                        col.a = 1.0f;
                        break;
                    }
                }

                // Write fragment output
                frag_out output;
                output.colour = col;
#if DEPTHWRITE_ON
                output.depth = localToDepth(rayStartPos + rayDir * (iStep * stepSize) - float3(0.5f, 0.5f, 0.5f));
#endif
                return output;
            }

            frag_in vert(vert_in v)
            {
                return vert_main(v);
            }

            frag_out frag(frag_in i)
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
