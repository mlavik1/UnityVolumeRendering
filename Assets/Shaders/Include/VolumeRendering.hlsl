
#include "TricubicSampling.cginc"

#define AMBIENT_LIGHTING_FACTOR 0.2
#define JITTER_FACTOR 5.0

struct volrend_result
{
    float4 colour;
    float depth;
};

Texture3D _DataTex;           SamplerState sampler_DataTex;
Texture3D _GradientTex;       SamplerState sampler_GradientTex;
Texture2D _NoiseTex;          SamplerState sampler_NoiseTex;
Texture2D _TFTex;             SamplerState sampler_TFTex;
Texture3D _ShadowVolume;      SamplerState sampler_ShadowVolume;
Texture3D _SecondaryDataTex;  SamplerState sampler_SecondaryDataTex;
Texture2D _SecondaryTFTex;    SamplerState sampler_SecondaryTFTex;

float _MinVal;
float _MaxVal;
float3 _TextureSize;
float3 _ShadowVolumeTextureSize;

float _MinGradient;
float _LightingGradientThresholdStart;
float _LightingGradientThresholdEnd;

float _SamplingRateMultiplier;

#if CROSS_SECTION_ON
#include "VolumeCutout.cginc"
#else
bool IsCutout(float3 currPos)
{
    return false;
}
#endif

struct RayInfo
{
    float3 startPos;
    float3 endPos;
    float3 direction;
    float2 aabbInters;
};

struct RaymarchInfo
{
    RayInfo ray;
    int numSteps;
    float numStepsRecip;
    float stepSize;
};

float3 getViewRayDir(float3 vertexLocal)
{
    if(unity_OrthoParams.w == 0)
    {
        // Perspective
        return normalize(ObjSpaceViewDir(float4(vertexLocal, 0.0f)));
    }
    else
    {
        // Orthographic
        float3 camfwd = mul((float3x3)unity_CameraToWorld, float3(0,0,-1));
        float4 camfwdobjspace = mul(unity_WorldToObject, camfwd);
        return normalize(camfwdobjspace);
    }
}

// Find ray intersection points with axis aligned bounding box
float2 intersectAABB(float3 rayOrigin, float3 rayDir, float3 boxMin, float3 boxMax)
{
    float3 tMin = (boxMin - rayOrigin) / rayDir;
    float3 tMax = (boxMax - rayOrigin) / rayDir;
    float3 t1 = min(tMin, tMax);
    float3 t2 = max(tMin, tMax);
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
    return float2(tNear, tFar);
};

// Get a ray for the specified fragment (back-to-front)
RayInfo getRayBack2Front(float3 vertexLocal)
{
    RayInfo ray;
    ray.direction = getViewRayDir(vertexLocal);
    ray.startPos = vertexLocal + float3(0.5f, 0.5f, 0.5f);
    // Find intersections with axis aligned boundinng box (the volume)
    ray.aabbInters = intersectAABB(ray.startPos, ray.direction, float3(0.0, 0.0, 0.0), float3(1.0f, 1.0f, 1.0));

    // Check if camera is inside AABB
    const float3 farPos = ray.startPos + ray.direction * ray.aabbInters.y - float3(0.5f, 0.5f, 0.5f);
    float4 clipPos = UnityObjectToClipPos(float4(farPos, 1.0f));
    if(unity_OrthoParams.w == 0)
    {
        float3 viewDir = ObjSpaceViewDir(float4(vertexLocal, 0.0f));
        float viewDist = length(viewDir);
        if (ray.aabbInters.y > viewDist)
        {
            ray.aabbInters.y = viewDist;
        }
    }

    ray.endPos = ray.startPos + ray.direction * ray.aabbInters.y;
    return ray;
}

// Get a ray for the specified fragment (front-to-back)
RayInfo getRayFront2Back(float3 vertexLocal)
{
    RayInfo ray = getRayBack2Front(vertexLocal);
    ray.direction = -ray.direction;
    float3 tmp = ray.startPos;
    ray.startPos = ray.endPos;
    ray.endPos = tmp;
    return ray;
}

RaymarchInfo initRaymarch(RayInfo ray, int maxNumSteps)
{
    RaymarchInfo raymarchInfo;
    raymarchInfo.stepSize = 1.732f/*greatest distance in box*/ / maxNumSteps;
    raymarchInfo.numSteps = (int)clamp(abs(ray.aabbInters.x - ray.aabbInters.y) / raymarchInfo.stepSize, 1, maxNumSteps);
    raymarchInfo.numStepsRecip = 1.0 / raymarchInfo.numSteps;
    return raymarchInfo;
}

// Gets the colour from a 1D Transfer Function (x = density)
float4 getTF1DColour(float density)
{
    return _TFTex.SampleLevel(sampler_TFTex, float2(density, 0.0), 0);
}

// Gets the colour from a 2D Transfer Function (x = density, y = gradient magnitude)
float4 getTF2DColour(float density, float gradientMagnitude)
{
    return _TFTex.SampleLevel(sampler_TFTex, float2(density, gradientMagnitude), 0);
}

// Gets the colour from a secondary 1D Transfer Function (x = density)
float4 getSecondaryTF1DColour(float density)
{
    return _SecondaryTFTex.SampleLevel(sampler_SecondaryTFTex, float2(density, 0.0), 0);
}

// Gets the density at the specified position
float getDensity(float3 pos)
{
#if CUBIC_INTERPOLATION_ON
    return interpolateTricubicFast(_DataTex, sampler_DataTex, pos, _TextureSize);
#else
    return _DataTex.SampleLevel(sampler_DataTex, pos, 0).r;
#endif
}

// Gets the density of the secondary volume at the specified position
float getSecondaryDensity(float3 pos)
{
    return _SecondaryDataTex.SampleLevel(sampler_SecondaryDataTex, pos, 0).r;
}

// Gets the density at the specified position, without tricubic interpolation
float getDensityNoTricubic(float3 pos)
{
    return _DataTex.SampleLevel(sampler_DataTex, pos, 0).r;
}

// Gets the gradient at the specified position
float3 getGradient(float3 pos)
{
#if CUBIC_INTERPOLATION_ON
    return interpolateTricubicFast(_GradientTex, sampler_GradientTex, pos, _TextureSize).rgb;
#else
    return _GradientTex.SampleLevel(sampler_GradientTex, pos, 0).rgb;
#endif
}

// Get the light direction (using main light or view direction, based on setting)
float3 getLightDirection(float3 viewDir)
{
#if defined(USE_MAIN_LIGHT)
    return normalize(mul(unity_WorldToObject, _WorldSpaceLightPos0.xyz));
#else
    return viewDir;
#endif
}

// Performs lighting calculations, and returns a modified colour.
float3 calculateLighting(float3 col, float3 normal, float3 lightDir, float3 eyeDir, float specularIntensity)
{
    // Invert normal if facing opposite direction of view direction.
    // Optimised version of: if(dot(normal, eyeDir) < 0.0) normal *= -1.0
    normal *= (step(0.0, dot(normal, eyeDir)) * 2.0 - 1.0);

    float ndotl = max(dot(normal, lightDir), 0.0f);
    float3 diffuse = ndotl * col;
    float3 ambient = AMBIENT_LIGHTING_FACTOR * col;
    float3 v = eyeDir;
    float3 r = normalize(reflect(-lightDir, normal));
    float rdotv = max( dot( r, v ), 0.0 );
    float3 specular = pow(rdotv, 32.0f) * float3(1.0f, 1.0f, 1.0f) * specularIntensity;
    float3 result = diffuse + ambient + specular;
    return float3(min(result.r, 1.0f), min(result.g, 1.0f), min(result.b, 1.0f));
}

float getNoise(float2 uv)
{
    return _NoiseTex.Sample(sampler_NoiseTex, uv).r;
}

float calculateShadow(float3 pos, float3 lightDir)
{
#if CUBIC_INTERPOLATION_ON
    return interpolateTricubicFast(_ShadowVolume, sampler_ShadowVolume, float3(pos.x, pos.y, pos.z), _ShadowVolumeTextureSize);
#else
    return _ShadowVolume.SampleLevel(sampler_ShadowVolume, pos, 0).r;
#endif
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

// Direct Volume Rendering
volrend_result volrend_dvr(float3 vertexLocal, float2 uv)
{
    #define MAX_NUM_STEPS 512
    #define OPACITY_THRESHOLD (1.0 - 1.0 / 255.0)
    const int samplingRate = (int)(MAX_NUM_STEPS * _SamplingRateMultiplier);

    RayInfo ray = getRayFront2Back(vertexLocal);
    RaymarchInfo raymarchInfo = initRaymarch(ray, samplingRate);

    float3 lightDir = normalize(ObjSpaceViewDir(float4(float3(0.0f, 0.0f, 0.0f), 0.0f)));

    // Create a small random offset in order to remove artifacts
    ray.startPos += (JITTER_FACTOR * ray.direction * raymarchInfo.stepSize) * getNoise(float2(uv.x, uv.y)).r;

    float4 col = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float tDepth = raymarchInfo.numStepsRecip * (raymarchInfo.numSteps - 1);
    for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
    {
        const float t = iStep * raymarchInfo.numStepsRecip;
        const float3 currPos = lerp(ray.startPos, ray.endPos, t);

        // Perform slice culling (cross section plane)
#ifdef CROSS_SECTION_ON
        if(IsCutout(currPos))
            continue;
#endif

#if CUBIC_INTERPOLATION_ON && !defined(MULTIVOLUME_OVERLAY) && !defined(MULTIVOLUME_ISOLATE)
        // Optimisation: First get density without tricubic interpolation, before doing an early return
        if (getTF1DColour(getDensityNoTricubic(currPos)).a == 0.0)
            continue;
#endif

        // Get the dansity/sample value of the current position
        const float density = getDensity(currPos);

        // Apply visibility window
        if (density < _MinVal || density > _MaxVal) continue;

        // Apply 1D transfer function
#if !TF2D_ON
        float4 src = getTF1DColour(density);
#if !defined(MULTIVOLUME_OVERLAY) && !defined(MULTIVOLUME_ISOLATE)
        if (src.a == 0.0)
            continue;
#endif

#if defined(MULTIVOLUME_OVERLAY) || defined(MULTIVOLUME_ISOLATE)
        const float secondaryDensity = getSecondaryDensity(currPos);
        float4 secondaryColour = getSecondaryTF1DColour(secondaryDensity);
#if MULTIVOLUME_OVERLAY
        src = secondaryColour.a > 0.0 ? secondaryColour : src;
#elif MULTIVOLUME_ISOLATE
        src.a = secondaryColour.a > 0.0 ? src.a : 0.0;
#endif
#endif
#endif

        // Calculate gradient (needed for lighting and 2D transfer functions)
#if defined(TF2D_ON) || defined(LIGHTING_ON)
        float3 gradient = getGradient(currPos);
        float gradMag = length(gradient);

        float gradMagNorm = gradMag / 1.75f;
#endif

        // Apply 2D transfer function
#if TF2D_ON
        float4 src = getTF2DColour(density, gradMagNorm);
        if (src.a == 0.0)
            continue;
#endif

        // Apply lighting
#if defined(LIGHTING_ON)
        float factor = smoothstep(_LightingGradientThresholdStart, _LightingGradientThresholdEnd, gradMag);
        float3 shaded = calculateLighting(src.rgb, gradient / gradMag, getLightDirection(-ray.direction), -ray.direction, 0.3f);
        src.rgb = lerp(src.rgb, shaded, factor);
#if defined(SHADOWS_ON)
        float shadow = calculateShadow(currPos, getLightDirection(-ray.direction));
        src.rgb *= (1.0f - shadow);
#endif
#endif

        // Opacity correction
        float blendFactor = 1.0f / _SamplingRateMultiplier;
        src.a = 1.0f - pow(1.0f - src.a, blendFactor);
        src.rgb *= src.a;
        col = (1.0f - col.a) * src + col;

        if (col.a > 0.15 && t < tDepth) {
            tDepth = t;
        }

        // Early ray termination
#if defined(RAY_TERMINATE_ON)
        if (col.a > OPACITY_THRESHOLD) {
            break;
        }
#endif
    }

    // Write fragment output
    volrend_result output;
    output.colour = col;
#if DEPTHWRITE_ON
    tDepth += (step(col.a, 0.0) * 1000.0); // Write large depth if no hit
    const float3 depthPos = lerp(ray.startPos, ray.endPos, tDepth) - float3(0.5f, 0.5f, 0.5f);
    output.depth = localToDepth(depthPos);
#endif
    return output;
}

// Maximum Intensity Projection mode
volrend_result volrend_mip(float3 vertexLocal, float2 uv)
{
    #define MAX_NUM_STEPS 512
    const int samplingRate = (int)(MAX_NUM_STEPS * _SamplingRateMultiplier);

    RayInfo ray = getRayBack2Front(vertexLocal);
    RaymarchInfo raymarchInfo = initRaymarch(ray, samplingRate);

    float maxDensity = 0.0f;
    float3 maxDensityPos = ray.startPos;
    for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
    {
        const float t = iStep * raymarchInfo.numStepsRecip;
        const float3 currPos = lerp(ray.startPos, ray.endPos, t);
        
#ifdef CROSS_SECTION_ON
        if (IsCutout(currPos))
            continue;
#endif

        const float density = getDensity(currPos);
        if (density > maxDensity && density > _MinVal && density < _MaxVal)
        {
            maxDensity = density;
            maxDensityPos = currPos;
        }
    }

    // Write fragment output
    volrend_result output;
    output.colour = float4(1.0f, 1.0f, 1.0f, maxDensity); // maximum intensity
#if DEPTHWRITE_ON
    output.depth = localToDepth(maxDensityPos - float3(0.5f, 0.5f, 0.5f));
#endif
    return output;
}

// Surface rendering mode
// Draws the first point (closest to camera) with a density within the user-defined thresholds.
volrend_result frag_surf(float3 vertexLocal, float2 uv)
{
    #define MAX_NUM_STEPS 1024
    const int samplingRate = (int)(MAX_NUM_STEPS * _SamplingRateMultiplier);

    RayInfo ray = getRayFront2Back(vertexLocal);
    RaymarchInfo raymarchInfo = initRaymarch(ray, samplingRate);

    // Create a small random offset in order to remove artifacts
    ray.startPos = ray.startPos + (JITTER_FACTOR * ray.direction * raymarchInfo.stepSize) * getNoise(float2(uv.x, uv.y)).r;

    float4 col = float4(0,0,0,0);
    for (int iStep = 0; iStep < raymarchInfo.numSteps; iStep++)
    {
        const float t = iStep * raymarchInfo.numStepsRecip;
        const float3 currPos = lerp(ray.startPos, ray.endPos, t);
        
#ifdef CROSS_SECTION_ON
        if (IsCutout(currPos))
            continue;
#endif

        const float density = getDensity(currPos);
#if MULTIVOLUME_ISOLATE
        const float secondaryDensity = getSecondaryDensity(currPos);
        if (secondaryDensity <= 0.0)
            continue;
#elif MULTIVOLUME_OVERLAY
        const float secondaryDensity = getSecondaryDensity(currPos);
        if (secondaryDensity > 0.0)
        {
            float4 secondaryColour = getSecondaryTF1DColour(secondaryDensity);
            if (secondaryColour.a > 0.0)
            {
                col = secondaryColour;
                float3 gradient = getGradient(currPos);
                float gradMag = length(gradient);
                float3 normal = gradient / gradMag;
                col.rgb = calculateLighting(col.rgb, normal, getLightDirection(-ray.direction), -ray.direction, 0.15);
                col.a = 1.0;
                break;
            }
        }
#endif
        if (density > _MinVal && density < _MaxVal)
        {
            float3 gradient = getGradient(currPos);
            float gradMag = length(gradient);
            if (gradMag > _MinGradient)
            {
                float3 normal = gradient / gradMag;
                col = getTF1DColour(density);
                col.rgb = calculateLighting(col.rgb, normal, getLightDirection(-ray.direction), -ray.direction, 0.15);
                col.a = 1.0f;
                break;
            }
        }
    }

    // Write fragment output
    volrend_result output;
    output.colour = col;
#if DEPTHWRITE_ON
    
    const float tDepth = iStep * raymarchInfo.numStepsRecip + (step(col.a, 0.0) * 1000.0); // Write large depth if no hit
    output.depth = localToDepth(lerp(ray.startPos, ray.endPos, tDepth) - float3(0.5f, 0.5f, 0.5f));
#endif
    return output;
}

volrend_result volrend(float3 vertexLocal, float2 uv)
{
#if MODE_DVR
    return volrend_dvr(vertexLocal, uv);
#elif MODE_MIP
    return volrend_mip(vertexLocal, uv);
#elif MODE_SURF
    return volrend_surf(vertexLocal, uv);
#endif
}
