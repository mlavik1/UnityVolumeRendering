using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Supported render pipeline types in Unity.
    /// </summary>
    public enum RenderPipelineType
    {
        /// <summary>
        /// Built-in render pipeline (default Unity rendering).
        /// </summary>
        BuiltIn,

        /// <summary>
        /// Universal Render Pipeline (URP).
        /// </summary>
        URP,

        /// <summary>
        /// High Definition Render Pipeline (HDRP).
        /// </summary>
        HDRP,

        /// <summary>
        /// Unknown or custom render pipeline.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Utility class for detecting the active render pipeline at runtime.
    /// Provides type-based detection that is more reliable than string-based checking.
    /// </summary>
    public static class RenderPipelineHelper
    {
        /// <summary>
        /// Cached pipeline type to avoid repeated type resolution.
        /// </summary>
        private static RenderPipelineType? cachedPipelineType = null;

        /// <summary>
        /// Gets the currently active render pipeline type.
        /// Uses type-based detection with caching for optimal performance.
        /// </summary>
        /// <returns>The detected render pipeline type.</returns>
        public static RenderPipelineType GetCurrentPipelineType()
        {
            // Return cached value if available
            if (cachedPipelineType.HasValue)
                return cachedPipelineType.Value;

            var currentPipeline = GraphicsSettings.currentRenderPipeline;

            // If null, we're using the Built-in pipeline
            if (currentPipeline == null)
            {
                cachedPipelineType = RenderPipelineType.BuiltIn;
                return RenderPipelineType.BuiltIn;
            }

            // Primary detection: Type.GetType() with full assembly-qualified name
            // This approach safely handles missing assemblies by returning null
            Type currentPipelineType = currentPipeline.GetType();
            Type urpAssetType = Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
            Type hdrpAssetType = Type.GetType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset, Unity.RenderPipelines.HighDefinition.Runtime");

            // Check for URP (only if URP assembly is available)
            if (urpAssetType != null && (currentPipelineType == urpAssetType || currentPipelineType.IsSubclassOf(urpAssetType)))
            {
                cachedPipelineType = RenderPipelineType.URP;
                return RenderPipelineType.URP;
            }

            // Check for HDRP (only if HDRP assembly is available)
            if (hdrpAssetType != null && (currentPipelineType == hdrpAssetType || currentPipelineType.IsSubclassOf(hdrpAssetType)))
            {
                cachedPipelineType = RenderPipelineType.HDRP;
                return RenderPipelineType.HDRP;
            }

            // Fallback: String-based type name checking (if Type.GetType fails)
            // This provides additional robustness in edge cases
            string pipelineTypeName = currentPipelineType.Name;
            if (pipelineTypeName == "UniversalRenderPipelineAsset")
            {
                cachedPipelineType = RenderPipelineType.URP;
                return RenderPipelineType.URP;
            }
            if (pipelineTypeName == "HDRenderPipelineAsset")
            {
                cachedPipelineType = RenderPipelineType.HDRP;
                return RenderPipelineType.HDRP;
            }

            // Unknown or custom pipeline type
            cachedPipelineType = RenderPipelineType.Unknown;
            return RenderPipelineType.Unknown;
        }

        /// <summary>
        /// Checks if the Built-in render pipeline is currently active.
        /// </summary>
        /// <returns>True if using Built-in pipeline, false otherwise.</returns>
        public static bool IsBuiltInPipeline()
        {
            return GetCurrentPipelineType() == RenderPipelineType.BuiltIn;
        }

        /// <summary>
        /// Checks if the Universal Render Pipeline (URP) is currently active.
        /// </summary>
        /// <returns>True if using URP, false otherwise.</returns>
        public static bool IsUniversalPipeline()
        {
            return GetCurrentPipelineType() == RenderPipelineType.URP;
        }

        /// <summary>
        /// Checks if the High Definition Render Pipeline (HDRP) is currently active.
        /// </summary>
        /// <returns>True if using HDRP, false otherwise.</returns>
        public static bool IsHDPipeline()
        {
            return GetCurrentPipelineType() == RenderPipelineType.HDRP;
        }

        /// <summary>
        /// Invalidates the cached pipeline type, forcing re-detection on next query.
        /// Call this if the render pipeline asset is changed at runtime.
        /// </summary>
        public static void InvalidateCache()
        {
            cachedPipelineType = null;
        }
    }
}
