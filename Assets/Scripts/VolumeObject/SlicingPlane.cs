using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Slice renderer.
    /// Renders a plane in two passes:
    /// Pass 1: Calculate the sample value (and gradient), and write it to a texture.
    /// Pass 2: Post-process pass that converts sample value and gradient to a colour.
    /// </summary>
    [ExecuteInEditMode]
    public class SlicingPlane : MonoBehaviour
    {
        // Render textures for each pass
        private RenderTexture prePassRenderTexture;
        private RenderTexture postPassRenderTexture;

        // Output textures for each pass
        public Texture2D prePassOutputTexture;
        public Texture2D postPassOutputTexture;

        private Material prePassMat;
        private Material postPassMat;

        public VolumeRenderedObject voldRendObj;

        private const int TEXTURE_WIDTH = 2048;
        private const int TEXTURE_HEIGHT = 2048;

        private void Awake()
        {
            prePassRenderTexture = new RenderTexture(TEXTURE_WIDTH, TEXTURE_HEIGHT, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            postPassRenderTexture = new RenderTexture(TEXTURE_WIDTH, TEXTURE_HEIGHT, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
            prePassOutputTexture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBAFloat, false);
            postPassOutputTexture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBAFloat, false);
            prePassMat = new Material(Shader.Find("VolumeRendering/SliceRenderingPrePass"));
            postPassMat = new Material(Shader.Find("VolumeRendering/SliceRenderingPostPass"));
        }

        private void Update()
        {
            if (voldRendObj == null)
                return;

            prePassMat.SetMatrix("_parentInverseMat", transform.parent.worldToLocalMatrix);
            prePassMat.SetMatrix("_planeMat", Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one)); // TODO: allow changing scale
            prePassMat.SetTexture("_DataTex", voldRendObj.dataset.GetTexture());

            postPassMat.SetTexture("_TFTex", voldRendObj.transferFunction.GetTexture());
            postPassMat.SetTexture("_InputTex", prePassRenderTexture);

            // Pass 1
            Graphics.Blit(null, prePassRenderTexture, prePassMat, 0);

            // Pass 2
            Graphics.Blit(prePassRenderTexture, postPassRenderTexture, postPassMat, 0);

            // Copy to texture
            RenderTexture.active = prePassRenderTexture;
            prePassOutputTexture.ReadPixels(new Rect(0.0f, 0.0f, TEXTURE_WIDTH, TEXTURE_HEIGHT), 0, 0, false);
            prePassOutputTexture.Apply();
            RenderTexture.active = postPassRenderTexture;
            postPassOutputTexture.ReadPixels(new Rect(0.0f, 0.0f, TEXTURE_WIDTH, TEXTURE_HEIGHT), 0, 0, false);
            postPassOutputTexture.Apply();
            RenderTexture.active = null;
        }
    }
}
