using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace AsyncTextureImport
{
    /// <summary>
    /// Async texture importer.
    /// You can use this from a coroutine, like this:
    ///    TextureImporter importer = new TextureImporter();
    ///    yield return importer.ImportTexture(...);
    /// The result will be available in the "texture" member variable.
    /// </summary>
    public class TextureImporter
    {
        /// <summary>
        /// 
        /// </summary>
        public Texture2D texture = null;

        /// <summary>
        /// Image filtering (affects the mipmap quality).
        /// </summary>
        public FREE_IMAGE_FILTER imageFilter = FREE_IMAGE_FILTER.FILTER_BILINEAR;

        /// <summary>
        /// Import texture from file.
        /// </summary>
        /// <param name="texturePath">Filepath of texture to load</param>
        /// <param name="format">Image format of the texture file (JPG, PNG, etc.)</param>
        /// /// <param name="mipLevels">(optional) Number of mip levels. The default is -1 (auto)</param>
        /// <returns></returns>
        public IEnumerator ImportTexture(string texturePath, FREE_IMAGE_FORMAT format, int mipLevels = -1)
        {
            this.texture = null;

            Task<RawTextureData> task = Task.Run(() => { return ImportTextureFromFile(texturePath, format, mipLevels); });
            while (!task.IsCompleted)
                yield return null;
            this.texture = CreateTexture(task.Result);
        }

        /// <summary>
        /// Import texture from memory.
        /// </summary>
        /// <param name="textureData">Texture file data (loaded into memory).</param>
        /// <param name="format">Image format of the texture file (JPG, PNG, etc.)</param>
        /// /// <param name="mipLevels">(optional) Number of mip levels. The default is -1 (auto)</param>
        /// <returns></returns>
        public IEnumerator ImportTexture(byte[] textureData, FREE_IMAGE_FORMAT format, int mipLevels = -1)
        {
            this.texture = null;

            Task<RawTextureData> task = Task.Run(() => { return ImportTextureFromMemory(textureData, format, mipLevels); });
            while (!task.IsCompleted)
                yield return null;
            this.texture = CreateTexture(task.Result);
        }

        private RawTextureData ImportTextureFromFile(string texturePath, FREE_IMAGE_FORMAT format, int mipLevels)
        {
            if(!File.Exists(texturePath))
            {
                Debug.LogError($"File does not exist: {texturePath}");
                return null;
            }

            // Load from file
            IntPtr texHandle = FreeImage.FreeImage_Load(format, texturePath, 0);
            // Import texture data
            RawTextureData textureData = ImportTextureData(texHandle, mipLevels);

            if (texHandle != IntPtr.Zero)
                FreeImage.FreeImage_Unload(texHandle);

            return textureData;
        }

        private RawTextureData ImportTextureFromMemory(byte[] textureBytes, FREE_IMAGE_FORMAT format, int mipLevels)
        {
            if (textureBytes == null || textureBytes.Length == 0)
            {
                Debug.LogError("Empty texture data");
                return null;
            }

            // Load from memory
            IntPtr texMem = FreeImage.FreeImage_OpenMemory(Marshal.UnsafeAddrOfPinnedArrayElement(textureBytes, 0), (uint)textureBytes.Length);
            IntPtr texHandle = FreeImage.FreeImage_LoadFromMemory(format, texMem, 0);
            // Import texture data
            RawTextureData textureData = ImportTextureData(texHandle, mipLevels);

            if (texHandle != IntPtr.Zero)
                FreeImage.FreeImage_Unload(texHandle);

            return textureData;
        }

        private RawTextureData ImportTextureData(IntPtr texHandle, int mipLevels)
        {
            uint width = FreeImage.FreeImage_GetWidth(texHandle);
            uint height = FreeImage.FreeImage_GetHeight(texHandle);
            uint size = width * height * 4;

            byte[] data = new byte[size];
            FreeImage.FreeImage_ConvertToRawBits(Marshal.UnsafeAddrOfPinnedArrayElement(data, 0), texHandle, (int)width * 4, 32, 0, 0, 0, false);

            RawTextureData texData = new RawTextureData();
            texData.data = data;
            texData.width = (int)width;
            texData.height = (int)height;
            texData.mipLevels = mipLevels;

            GenerateMipMaps(texHandle, texData);

            return texData;
        }

        private void GenerateMipMaps(IntPtr texHandle, RawTextureData texData)
        {
            if (texData.mipLevels == 1)
                return; // Only one level => nothing to do

            int texSize = texData.width * texData.height * 4;

            MemoryStream imgStream = new MemoryStream();
            imgStream.Write(texData.data, 0, (int)texSize);

            // Calculate mip levels (-1 means auto)
            if(texData.mipLevels == -1)
            {
                float f = Mathf.Min((float)texData.width, (float)texData.height);
                texData.mipLevels = Math.Min((int)Mathf.Log(f, 2), 11);
            }

            for (int iMip = 1; iMip < texData.mipLevels; iMip++)
            {
                int mipWidth = texData.width / (int)Mathf.Pow(2.0f, (float)iMip);
                int mipHeight = texData.height / (int)Mathf.Pow(2.0f, (float)iMip);
                int mipSize = mipWidth * mipHeight * 4;

                byte[] mipData = new byte[mipSize];

                IntPtr mipHandle = FreeImage.FreeImage_Rescale(texHandle, mipWidth, mipHeight, imageFilter);
                IntPtr mipBmpHandle = FreeImage.FreeImage_ConvertTo32Bits(mipHandle);
                FreeImage.FreeImage_ConvertToRawBits(Marshal.UnsafeAddrOfPinnedArrayElement(mipData, 0), mipBmpHandle, mipWidth * 4, 32, 0, 0, 0, false);

                imgStream.Write(mipData, 0, mipSize);

                FreeImage.FreeImage_Unload(mipHandle);
                FreeImage.FreeImage_Unload(mipBmpHandle);
            }

            texData.data = imgStream.ToArray();
        }

        private Texture2D CreateTexture(RawTextureData texData)
        {
            if (texData == null)
                return null;

            Texture2D tex = new Texture2D(texData.width, texData.height, TextureFormat.BGRA32, texData.mipLevels, false);
            tex.filterMode = FilterMode.Trilinear;
            tex.LoadRawTextureData(texData.data);
            tex.Apply(false, true);
            return tex;
        }

        private class RawTextureData
        {
            public byte[] data;
            public int mipLevels;
            public int width;
            public int height;
        }
    }
}
