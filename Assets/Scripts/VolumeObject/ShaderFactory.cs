using UnityEngine;
using UnityEngine.Rendering;

namespace UnityVolumeRendering
{
    public class ShaderFactory
    {
        /// <summary>
        /// Helper method to get the appropriate shader based on the current render pipeline.
        /// </summary>
        /// <param name="builtinPath">Shader path for Built-in render pipeline.</param>
        /// <param name="urpPath">Shader path for Universal Render Pipeline (URP).</param>
        /// <param name="hdrpPath">Shader path for High Definition Render Pipeline (HDRP).</param>
        /// <returns>The shader for the current render pipeline.</returns>
        private static Shader GetShaderByPipeline(string builtinPath, string urpPath, string hdrpPath)
        {
            Shader shader = null;

            switch (RenderPipelineHelper.GetCurrentPipelineType())
            {
                case RenderPipelineType.URP:
                    shader = Shader.Find(urpPath);
                    break;
                case RenderPipelineType.HDRP:
                    shader = Shader.Find(hdrpPath);
                    break;
                case RenderPipelineType.BuiltIn:
                case RenderPipelineType.Unknown:
                default:
                    shader = Shader.Find(builtinPath);
                    break;
            }

            Debug.Assert(shader != null, $"Could not find shader. Searched: BuiltIn='{builtinPath}', URP='{urpPath}', HDRP='{hdrpPath}'");
            return shader;
        }

        public static Shader GetVolumeRenderingShader()
        {
            return GetShaderByPipeline(
                "VolumeRendering/Builtin/VolumeRendering",
                "VolumeRendering/URP/VolumeRendering",
                "VolumeRendering/HDRP/VolumeRendering"
            );
        }

        public static Shader GetCrossSectionPlaneShader()
        {
            return GetShaderByPipeline(
                "VolumeRendering/Builtin/CrossSectionPlane",
                "VolumeRendering/URP/CrossSectionPlane",
                "VolumeRendering/HDRP/CrossSectionPlane"
            );
        }

        public static Shader GetCrossSectionSphereShader()
        {
            return GetShaderByPipeline(
                "VolumeRendering/Builtin/CrossSectionSphere",
                "VolumeRendering/URP/CrossSectionSphere",
                "VolumeRendering/HDRP/CrossSectionSphere"
            );
        }

        public static Shader GetSliceRenderingShader()
        {
            return GetShaderByPipeline(
                "VolumeRendering/Builtin/SliceRenderingShader",
                "VolumeRendering/URP/SliceRenderingShader",
                "VolumeRendering/HDRP/SliceRenderingShader"
            );
        }

        public static Shader GetTransferFunctionShader()
        {
            return GetShaderByPipeline(
                "VolumeRendering/Builtin/TransferFunctionShader",
                "VolumeRendering/URP/TransferFunctionShader",
                "VolumeRendering/HDRP/TransferFunctionShader"
            );
        }

        public static Shader GetTransferFunction2DShader()
        {
            return GetShaderByPipeline(
                "VolumeRendering/Builtin/TransferFunction2DShader",
                "VolumeRendering/URP/TransferFunction2DShader",
                "VolumeRendering/HDRP/TransferFunction2DShader"
            );
        }

        public static Shader GetTransferFunctionPaletteShader()
        {
            return GetShaderByPipeline(
                "VolumeRendering/Builtin/TransferFunctionPaletteShader",
                "VolumeRendering/URP/TransferFunctionPaletteShader",
                "VolumeRendering/HDRP/TransferFunctionPaletteShader"
            );
        }
    }
}
