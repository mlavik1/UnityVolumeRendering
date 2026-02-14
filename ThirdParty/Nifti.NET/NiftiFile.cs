using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Nifti.NET
{
    public static class NiftiFile
    {
        public enum FileType { NII, HDR, UNKNOWN }

        /// <summary>
        /// Read a Nifti file (or hdr/img pair).
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Nifti Read(string path, short forceType = NiftiHeader.DT_UNKNOWN)
        {
            Nifti result = new Nifti();

            using (var stream = ReadStream(path))
            {
                var hdr = ReadHeader(stream);
                result.Header = hdr;
                if (FileType.NII == TypeOf(hdr))
                {
                    result.Data = ReadData(stream, hdr, forceType);
                }
            }

            if(FileType.HDR == TypeOf(result.Header))
            {
                var imgpath = path.ToLower().Replace(".hdr", ".img");
                if(File.Exists(imgpath))
                {
                    using (var stream = ReadStream(imgpath))
                    {
                        result.Data = ReadData(stream, result.Header, forceType);
                    }
                }
            }

            if (TypeOf(result.Header) == FileType.UNKNOWN) throw new InvalidDataException("Not a NIfTI file (no magic bytes)");

            return result;
        }

        /// <summary>
        /// Read only the header of a nifti file (or header).
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static NiftiHeader ReadHeader(string path)
        {
            using (var stream = ReadStream(path))
            {
                var hdr = ReadHeader(stream);
                return hdr;
            }
        }

        /// <summary>
        /// Write the Nifti file according to the path.
        /// If the Header.magic[1] byte is 0x69 then the file will be split as hdr and img (we will assume the .hdr file is the path).
        /// Otherwise the magic[1] should be 0x2B
        /// </summary>
        /// <param name="nifti"></param>
        /// <param name="path"></param>
        /// <param name="gzip"></param>
        public static void Write(Nifti nifti, string path, bool gzip = false)
        {
            if (typeof(Color32[]) == nifti.Data.GetType())
            {
                System.Console.WriteLine("Detected Color32 data. Converting to 24bit RBG representation.");
                nifti = ConvertToRGB(nifti);
            }

            FileType type = TypeOf(nifti.Header);
            if (gzip && !path.EndsWith(".gz")) path += ".gz";
            using (var stream = WriteStream(path, gzip))
            {
                Write(stream, nifti.Header);

                // For a nifti file we can just keep on writing.
                if (FileType.NII == type) WriteData(stream, nifti.Data, nifti.Header.datatype);
                stream.Flush();
            }

            // If the file type is hdr/img we want to split the data.
            if (FileType.HDR == type && nifti.Data != null)
            {
                var newPath = path.Replace(".hdr", ".img");
                if (newPath.Equals(path)) newPath += ".img";
                using (var stream = WriteStream(newPath, gzip))
                {
                    WriteData(stream, nifti.Data, nifti.Header.datatype);
                    stream.Flush();
                }
            }
        }

        /// <summary>
        /// Returns true is the file is GZipped (according to the magic bytes).
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsCompressed(string path)
        {
            bool isCompressed = false;
            using (FileStream fs = File.OpenRead(path))
            {
                var b1 = fs.ReadByte();
                var b2 = fs.ReadByte();
                isCompressed = b1 == 0x01f && b2 == 0x8b;
            }
            return isCompressed;
        }

        /// <summary>
        /// Write the Nifti header to an .hdr file.
        /// </summary>
        /// <param name="hdr"></param>
        /// <param name="path"></param>
        /// <param name="gzip"></param>
        public static void Write(NiftiHeader hdr, string path, bool gzip = false)
        {
            using (var stream = WriteStream(path, gzip))
            {
                Write(stream, hdr);
            }
        }

        private static Stream ReadStream(string path)
        {
            MemoryStream ms = new MemoryStream();
            using (var fs = File.OpenRead(path))
            {
                if(IsCompressed(path))
                {
                    new GZipStream(fs, CompressionMode.Decompress).CopyTo(ms);
                }
                else
                {
                    fs.CopyTo(ms);
                }
            }

            ms.Position = 0;
            return ms;
        }

        private static Stream WriteStream(string path, bool gzip)
        {
            var fs = File.OpenWrite(path);
            if (gzip) return new GZipStream(fs, CompressionMode.Compress);
            return fs;
        }

        private static FileType TypeOf(NiftiHeader hdr)
        {
            // Magic bytes are "ni1\0" for hdr/img files and "n+1\0" for nii.
            if (hdr.magic[0] != 0x6E || hdr.magic[2] != 0x31 || hdr.magic[3] != 0x00) return FileType.UNKNOWN;
            else if (hdr.magic[1] == 0x69) return FileType.HDR;
            else if (hdr.magic[1] == 0x2B) return FileType.NII;
            else return FileType.UNKNOWN;
        }

        private static Array ReadData(Stream stream, NiftiHeader hdr, short forceType)
        {
            var datatype = forceType != NiftiHeader.DT_UNKNOWN ? forceType : hdr.datatype;
            var reverseBytes = hdr.SourceIsBigEndian();
            var bytelen = stream.Length - stream.Position;
            Array resultData = null;

            switch(datatype)
            {
                case NiftiHeader.DT_FLOAT32:
                {
                    float[] data = new float[bytelen / sizeof(float)];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadFloat(stream, reverseBytes);
                    resultData = data;
                    break;
                }
                case NiftiHeader.DT_INT32:
                {
                    int[] data = new int[bytelen / sizeof(int)];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadInt(stream, reverseBytes);
                    resultData = data;
                    break;
                }
                case NiftiHeader.DT_UINT32:
                {
                    int[] data = new int[bytelen / sizeof(uint)];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadInt(stream, reverseBytes);
                    resultData = data;
                    break;
                }
                case NiftiHeader.DT_INT16:
                {
                    short[] data = new short[bytelen / sizeof(short)];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadShort(stream, reverseBytes);
                    resultData = data;
                    break;
                }
                case NiftiHeader.DT_UINT16:
                {
                    ushort[] data = new ushort[bytelen / sizeof(ushort)];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadUShort(stream, reverseBytes);
                    resultData = data;
                    break;
                }
                case NiftiHeader.DT_DOUBLE:
                {
                    double[] data = new double[bytelen / sizeof(double)];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadDouble(stream, reverseBytes);
                    resultData = data;
                    break;
                }
                case NiftiHeader.DT_COMPLEX:
                {
                    long[] data = new long[bytelen / sizeof(long)];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadLong(stream, reverseBytes);
                    resultData = data;
                    break;
                }
                case NiftiHeader.DT_RGB24:
                {
                    Color32[] data = new Color32[bytelen / 3];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadRGB(stream, reverseBytes);
                    resultData = data;
                    break;
                }
                case NiftiHeader.DT_RGBA32:
                {
                    Color32[] data = new Color32[bytelen / 4];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadRGBA(stream, reverseBytes);
                    resultData = data;
                    break;
                }
                default:
                {
                    byte[] data = new byte[bytelen];
                    for (int i = 0; i < data.Length; ++i)
                        data[i] = ReadByte(stream);
                    resultData = data;
                    break;
                }
            }

            return resultData;
        }

        private static void WriteData(Stream stream, object data, short datatype)
        {
            switch(datatype)
            {
                case NiftiHeader.DT_FLOAT32:
                    foreach(var voxel in data as float[])
                        Write(stream, voxel);
                    break;
                case NiftiHeader.DT_INT32:
                    foreach(var voxel in data as int[])
                        Write(stream, voxel);
                    break;
                case NiftiHeader.DT_UINT32:
                    foreach(var voxel in data as uint[])
                        Write(stream, voxel);
                    break;
                case NiftiHeader.DT_INT16:
                    foreach(var voxel in data as short[])
                        Write(stream, voxel);
                    break;
                case NiftiHeader.DT_UINT16:
                    foreach(var voxel in data as ushort[])
                        Write(stream, voxel);
                    break;
                case NiftiHeader.DT_DOUBLE:
                    foreach(var voxel in data as double[])
                        Write(stream, voxel);
                    break;
                case NiftiHeader.DT_COMPLEX:
                    foreach(var voxel in data as long[])
                        Write(stream, voxel);
                    break;
                case NiftiHeader.DT_RGB24:
                    foreach(var voxel in data as Color32[])
                        WriteRGB(stream, voxel);
                    break;
                case NiftiHeader.DT_RGBA32:
                    foreach(var voxel in data as Color32[])
                        WriteRGBA(stream, voxel);
                    break;
                default:
                    foreach(var voxel in data as byte[])    
                        Write(stream, voxel);
                    break;
            }
        }

        private static NiftiHeader ReadHeader(Stream stream)
        {
            bool reverseBytes = false;

            //var memstrem = new MemoryStream();
            //stream.CopyTo(memstrem);

            var streamLen = stream.Length;

            NiftiHeader hdr = new NiftiHeader();
            hdr.sizeof_hdr = ReadInt(stream, reverseBytes);

            reverseBytes = hdr.SourceIsBigEndian();

            hdr.data_type = ReadBytes(stream, 10);
            hdr.db_name = ReadBytes(stream, 18);
            hdr.extents = ReadInt(stream, reverseBytes);
            hdr.session_error = ReadShort(stream, reverseBytes);
            hdr.regular = ReadByte(stream);
            hdr.dim_info = ReadByte(stream);

            hdr.dim = ReadMyShorts(stream, 8, reverseBytes);
            hdr.intent_p1 = ReadFloat(stream, reverseBytes);
            hdr.intent_p2 = ReadFloat(stream, reverseBytes);
            hdr.intent_p3 = ReadFloat(stream, reverseBytes);
            hdr.intent_code = ReadShort(stream, reverseBytes);
            hdr.datatype = ReadShort(stream, reverseBytes);
            hdr.bitpix = ReadShort(stream, reverseBytes);
            hdr.slice_start = ReadShort(stream, reverseBytes);
            hdr.pixdim = ReadFloats(stream, 8, reverseBytes);
            hdr.vox_offset = ReadFloat(stream, reverseBytes);
            hdr.scl_slope = ReadFloat(stream, reverseBytes);
            hdr.scl_inter = ReadFloat(stream, reverseBytes);
            hdr.slice_end = ReadShort(stream, reverseBytes);
            hdr.slice_code = ReadByte(stream);
            hdr.xyzt_units = ReadByte(stream);
            hdr.cal_max = ReadFloat(stream, reverseBytes);
            hdr.cal_min = ReadFloat(stream, reverseBytes);
            hdr.slice_duration = ReadFloat(stream, reverseBytes);
            hdr.toffset = ReadFloat(stream, reverseBytes);
            hdr.glmax = ReadInt(stream, reverseBytes);
            hdr.glmin = ReadInt(stream, reverseBytes);

            hdr.descrip = ReadBytes(stream, 80);
            hdr.aux_file = ReadBytes(stream, 24);

            hdr.qform_code = ReadShort(stream, reverseBytes);
            hdr.sform_code = ReadShort(stream, reverseBytes);

            hdr.quatern_b = ReadFloat(stream, reverseBytes);
            hdr.quatern_c = ReadFloat(stream, reverseBytes);
            hdr.quatern_d = ReadFloat(stream, reverseBytes);
            hdr.qoffset_x = ReadFloat(stream, reverseBytes);
            hdr.qoffset_y = ReadFloat(stream, reverseBytes);
            hdr.qoffset_z = ReadFloat(stream, reverseBytes);

            hdr.srow_x = ReadFloats(stream, 4, reverseBytes);
            hdr.srow_y = ReadFloats(stream, 4, reverseBytes);
            hdr.srow_z = ReadFloats(stream, 4, reverseBytes);

            hdr.intent_name = ReadBytes(stream, 16);
            hdr.magic = ReadBytes(stream, 4);

            if (streamLen >= 352)
            {
                hdr.extension = ReadBytes(stream, 4);

                if (hdr.extension[0] == 1) // Extension is present
                {
                    hdr.esize = ReadInt(stream, reverseBytes);
                    hdr.ecode = ReadInt(stream, reverseBytes);
                    hdr.edata = ReadBytes(stream, hdr.esize - 8);
                }
            }

            if (TypeOf(hdr) == FileType.UNKNOWN) throw new InvalidDataException("Not a NIfTI file (no magic bytes)");
            if (hdr.dim[0] > 7) throw new InvalidDataException("NIFTI header is using more than 7 dimensions. I don't really know how to handle that :\\");
            else if (hdr.dim[0] < 0) throw new InvalidDataException("Somethings broken with the dimensions...");

            return hdr;
        }
         
        private static void Write(Stream stream, NiftiHeader hdr)
        {
            Write(stream, NiftiHeader.EXPECTED_SIZE_OF_HDR);
            Write(stream, hdr.data_type);
            Write(stream, hdr.db_name);
            Write(stream, hdr.extents);
            Write(stream, hdr.session_error);
            Write(stream, hdr.regular);
            Write(stream, hdr.dim_info);
            Write(stream, hdr.dim);
            Write(stream, hdr.intent_p1);
            Write(stream, hdr.intent_p2);
            Write(stream, hdr.intent_p3);
            Write(stream, hdr.intent_code);
            Write(stream, hdr.datatype);
            Write(stream, hdr.bitpix);
            Write(stream, hdr.slice_start);
            Write(stream, hdr.pixdim);
            Write(stream, hdr.vox_offset);
            Write(stream, hdr.scl_slope);
            Write(stream, hdr.scl_inter);
            Write(stream, hdr.slice_end);
            Write(stream, hdr.slice_code);
            Write(stream, hdr.xyzt_units);
            Write(stream, hdr.cal_max);
            Write(stream, hdr.cal_min);
            Write(stream, hdr.slice_duration);
            Write(stream, hdr.toffset);
            Write(stream, hdr.glmax);
            Write(stream, hdr.glmin);

            Write(stream, hdr.descrip);
            Write(stream, hdr.aux_file);

            Write(stream, hdr.qform_code);
            Write(stream, hdr.sform_code);

            Write(stream, hdr.quatern_b);
            Write(stream, hdr.quatern_c);
            Write(stream, hdr.quatern_d);
            Write(stream, hdr.qoffset_x);
            Write(stream, hdr.qoffset_y);
            Write(stream, hdr.qoffset_z);

            Write(stream, hdr.srow_x);
            Write(stream, hdr.srow_y);
            Write(stream, hdr.srow_z);

            Write(stream, hdr.intent_name);
            Write(stream, hdr.magic);

            Write(stream, hdr.extension);
            if (hdr.extension[0] == 1)
            {
                Write(stream, hdr.esize);
                Write(stream, hdr.ecode);
                Write(stream, hdr.edata);
            }
        }

        private static void Write(Stream stream, int val)
        {
            stream.Write(BitConverter.GetBytes(val), 0, sizeof(int));
        }

        private static void Write(Stream stream, uint val)
        {
            stream.Write(BitConverter.GetBytes(val), 0, sizeof(uint));
        }

        private static void Write(Stream stream, short val)
        {
            stream.Write(BitConverter.GetBytes(val), 0, sizeof(short));
        }

        private static void Write(Stream stream, ushort val)
        {
            stream.Write(BitConverter.GetBytes(val), 0, sizeof(ushort));
        }

        private static void Write(Stream stream, float val)
        {
            stream.Write(BitConverter.GetBytes(val), 0, sizeof(float));
        }
        private static void Write(Stream stream, long val)
        {
            stream.Write(BitConverter.GetBytes(val), 0, sizeof(long));
        }
        private static void Write(Stream stream, double val)
        {
            stream.Write(BitConverter.GetBytes(val), 0, sizeof(double));
        }

        private static void Write(Stream stream, byte val)
        {
            stream.WriteByte(val);
        }

        private static void WriteRGB(Stream stream, Color32 val)
        {
            stream.WriteByte(val.r);
            stream.WriteByte(val.g);
            stream.WriteByte(val.b);
        }

        private static void WriteRGBA(Stream stream, Color32 val)
        {
            stream.WriteByte(val.r);
            stream.WriteByte(val.g);
            stream.WriteByte(val.b);
            stream.WriteByte(val.a);
        }

        //private static void Write(Stream stream, byte[] vals) { foreach (var val in vals) Write(stream, val); }
        private static void Write(Stream stream, byte[] vals) 
        {
            stream.Write(vals, 0, vals.Length);
        }

        private static void Write(Stream stream, int[] vals) { foreach (var val in vals) Write(stream, val); }
        private static void Write(Stream stream, float[] vals) { foreach (var val in vals) Write(stream, val); }
        private static void Write(Stream stream, short[] vals) { foreach (var val in vals) Write(stream, val); }
        private static void Write(Stream stream, long[] vals) { foreach (var val in vals) Write(stream, val); }
        private static void Write(Stream stream, double[] vals) { foreach (var val in vals) Write(stream, val); }

        private static float[] ReadFloats(Stream stream, int count, bool reverseBytes)
        {
            var result = new float[count];
            for (var i = 0; i < count; ++i) result[i] = ReadFloat(stream, reverseBytes);
            return result;
        }

        private static float ReadFloat(Stream stream, bool reverseBytes)
        {
            return !reverseBytes ? 
                BitConverter.ToSingle(ReadBytes(stream, 4), 0) 
                : BitConverter.ToSingle(ReadBytesReversed(stream, 4), 0);
        }

        private static byte ReadByte(Stream stream)
        {
            return (byte)stream.ReadByte();
        }

        private static int ReadInt(Stream stream, bool reverseBytes)
        {
            return !reverseBytes ?
                BitConverter.ToInt32(ReadBytes(stream, 4), 0) 
                : BitConverter.ToInt32(ReadBytesReversed(stream, 4), 0);
        }

        private static uint ReadUInt(Stream stream, bool reverseBytes)
        {
            return !reverseBytes ?
                BitConverter.ToUInt32(ReadBytes(stream, 4), 0)
                : BitConverter.ToUInt32(ReadBytesReversed(stream, 4), 0);
        }

        private static ushort ReadUShort(Stream stream, bool reverseBytes)
        {
            return !reverseBytes ?
                BitConverter.ToUInt16(ReadBytes(stream, 2), 0)
                : BitConverter.ToUInt16(ReadBytesReversed(stream, 2), 0);
        }
        private static ushort[] ReadUrShorts(Stream stream, int count, bool reverseBytes)
        {
            var result = new ushort[count];
            for(var i = 0; i < count; ++i) result[i] = ReadUShort(stream, reverseBytes);
            return result;
        }

        private static short ReadShort(Stream stream, bool reverseBytes)
        {
            return !reverseBytes ?
                BitConverter.ToInt16(ReadBytes(stream, 2), 0)
                : BitConverter.ToInt16(ReadBytesReversed(stream, 2), 0);
        }

        private static short[] ReadMyShorts(Stream stream, int count, bool reverseBytes)
        {
            var result = new short[count];
            for(var i = 0; i < count; ++i) result[i] = ReadShort(stream, reverseBytes);
            return result;
        }

        private static double ReadDouble(Stream stream, bool reverseBytes)
        {
            return !reverseBytes ?
                BitConverter.ToDouble(ReadBytes(stream, 8), 0)
                : BitConverter.ToDouble(ReadBytesReversed(stream, 8), 0);
        }

        private static long ReadLong(Stream stream, bool reverseBytes)
        {
            return !reverseBytes ?
                BitConverter.ToInt64(ReadBytes(stream, 8), 0)
                : BitConverter.ToInt64(ReadBytesReversed(stream, 8), 0);
        }

        private static Color32 ReadRGB(Stream stream, bool reverseBytes)
        {
            byte[] rgb = ReadBytes(stream, 3);
            return new Color32(rgb[0], rgb[1], rgb[2], 255);
        }

        private static Color32 ReadRGBA(Stream stream, bool reverseBytes)
        {
            byte[] rgba = ReadBytes(stream, 4);
            return new Color32(rgba[3], rgba[0], rgba[1], rgba[2]);

        }

        private static byte[] ReadBytes(Stream stream, int count)
        {
            var result = new byte[count];
            for (var i = 0; i < count; ++i) result[i] = (byte)stream.ReadByte();
            return result;
        }

        private static byte[] ReadBytesReversed(Stream stream, int count)
        {
            var result = new byte[count];
            for (int i = count; i > 0; --i) result[i - 1] = (byte)stream.ReadByte();
            return result;
        }

        private static Nifti ConvertToRGB(Nifti nifti)
        {
            // Setup the header info in case someone converted from non-Color32 input
            nifti.Header.dim[0] = 5; // RGB and RGBA both have 5 dimensions
            nifti.Header.dim[4] = 1;
            nifti.Header.dim[5] = 1;
            nifti.Header.bitpix = 24;
            nifti.Header.datatype = NiftiHeader.DT_RGB;
            nifti.Header.intent_code = NiftiHeader.NIFTI_INTENT_RGB_VECTOR;

            return nifti;
        }
    }
}
