using System;
using System.Runtime.InteropServices;

namespace AsyncTextureImport
{
    public enum FREE_IMAGE_FORMAT
    {
        FIF_UNKNOWN = -1,
        FIF_BMP = 0,
        FIF_ICO = 1,
        FIF_JPEG = 2,
        FIF_JNG = 3,
        FIF_KOALA = 4,
        FIF_LBM = 5,
        FIF_MNG = 6,
        FIF_PBM = 7,
        FIF_PBMRAW = 8,
        FIF_PCD = 9,
        FIF_PCX = 10,
        FIF_PGM = 11,
        FIF_PGMRAW = 12,
        FIF_PNG = 13,
        FIF_PPM = 14,
        FIF_PPMRAW = 15,
        FIF_RAS = 16,
        FIF_TARGA = 17,
        FIF_TIFF = 18,
        FIF_WBMP = 19,
        FIF_PSD = 20,
        FIF_CUT = 21,
        FIF_IFF = FIF_LBM,
        FIF_XBM = 22,
        FIF_XPM = 23
    }

    public enum FREE_IMAGE_FILTER
    {
        FILTER_BOX = 0,
        FILTER_BICUBIC = 1,
        FILTER_BILINEAR = 2,
        FILTER_BSPLINE = 3,
        FILTER_CATMULLROM = 4,
        FILTER_LANCZOS3 = 5
    }

    public class FreeImage
    {
        private const string FreeImageLibrary = "FreeImage";

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_Load")]
        public static extern IntPtr FreeImage_Load(FREE_IMAGE_FORMAT format, string filename, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_OpenMemory")]
        public static extern IntPtr FreeImage_OpenMemory(IntPtr data, uint size_in_bytes);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_CloseMemory")]
        public static extern IntPtr FreeImage_CloseMemory(IntPtr data);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_AcquireMemory")]
        public static extern bool FreeImage_AcquireMemory(IntPtr stream, ref IntPtr data, ref uint size_in_bytes);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_LoadFromMemory")]
        public static extern IntPtr FreeImage_LoadFromMemory(FREE_IMAGE_FORMAT format, IntPtr stream, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_Unload")]
        public static extern void FreeImage_Unload(IntPtr dib);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_Save")]
        public static extern bool FreeImage_Save(FREE_IMAGE_FORMAT format, IntPtr handle, string filename, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_SaveToMemory")]
        public static extern bool FreeImage_SaveToMemory(FREE_IMAGE_FORMAT format, IntPtr dib, IntPtr stream, int flags);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_ConvertToRawBits")]
        public static extern void FreeImage_ConvertToRawBits(IntPtr bits, IntPtr dib, int pitch, uint bpp, uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_ConvertToRawBits")]
        public static extern void FreeImage_ConvertToRawBits(byte[] bits, IntPtr dib, int pitch, uint bpp, uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_ConvertTo32Bits")]
        public static extern IntPtr FreeImage_ConvertTo32Bits(IntPtr handle);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_Rescale")]
        public static extern IntPtr FreeImage_Rescale(IntPtr dib, int dst_width, int dst_height, FREE_IMAGE_FILTER filter);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_GetWidth")]
        public static extern uint FreeImage_GetWidth(IntPtr handle);

        [DllImport(FreeImageLibrary, EntryPoint = "FreeImage_GetHeight")]
        public static extern uint FreeImage_GetHeight(IntPtr handle);
    }
}
