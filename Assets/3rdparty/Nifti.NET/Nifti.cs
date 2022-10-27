
using System;

namespace Nifti.NET
{
    /// <summary>
    /// The Dynamically typed version of a nifti object. Can be used for simple and direct access from files.
    /// </summary>
    public class Nifti
    {
        /// <summary>
        /// Private storage for header object.
        /// </summary>
        private NiftiHeader _header;

        /// <summary>
        /// The NIfTI file header object.
        /// </summary>
        public NiftiHeader Header
        {
            get
            {
                return _header;
            }
            set
            {
                // Set dimensions.
                Dimensions = new int[value.dim[0]];
                Stride = new int[value.dim[0]];

                for (int i = 0; i < Dimensions.Length; ++i)
                {
                    Dimensions[i] = value.dim[i + 1];

                    // Work out stride...
                    var stride = 1;
                    if (i > 0) for (int j = i - 1; j >= 0; --j) stride *= Dimensions[j];
                    Stride[i] = stride;
                }

                _header = value;
            }
        }

        // Volume interface implementation.
        /// <summary>
        /// The stride for the indexed dimension. This allows direct access to the data.
        /// </summary>
        public int[] Stride { get; private set; }
        /// <summary>
        /// The length of each dimension.
        /// </summary>
        public int[] Dimensions { get; set; }
        /// <summary>
        /// The underlying data structure as an array for fast linear processing.
        /// </summary>
        public Array Data { get; set; }

        /// <summary>
        /// Returns the linear index for a given multi-dimensional index.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        protected int DataIndexFor(params int[] idx)
        {
            // Because this is going to be in a lot of inner loops, we'll 
            // use if statements to catch the common cases (I've heard it makes for easier optimizing in the compiler)...
            if (idx.Length == 3) return idx[0] * Stride[0] + idx[1] * Stride[1] + idx[2] * Stride[2];
            else if (idx.Length == 2) return idx[0] * Stride[0] + idx[1] * Stride[1];

            // Now we'll handle the general case...
            int index = 0;

            for (int i = 0; i < idx.Length; ++i)
            {
                index += idx[i] * Stride[i];
            }
            return index;
        }

        /// <summary>
        /// Will return true if the underlying data is of the given type, otherwise false.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsType(Type type)
        {
            if (type == null) return false;
            if (type == typeof(byte) && Header.datatype == NiftiHeader.NIFTI_TYPE_INT8) return true;
            if (type == typeof(short) && Header.datatype == NiftiHeader.NIFTI_TYPE_INT16) return true;
            if (type == typeof(int) && Header.datatype == NiftiHeader.NIFTI_TYPE_INT32) return true;
            if (type == typeof(long) && Header.datatype == NiftiHeader.NIFTI_TYPE_INT64) return true;

            if (type == typeof(byte) && Header.datatype == NiftiHeader.NIFTI_TYPE_UINT8) return true;
            if (type == typeof(ushort) && Header.datatype == NiftiHeader.NIFTI_TYPE_UINT16) return true;
            if (type == typeof(uint) && Header.datatype == NiftiHeader.NIFTI_TYPE_UINT32) return true;
            if (type == typeof(ulong) && Header.datatype == NiftiHeader.NIFTI_TYPE_UINT64) return true;

            if (type == typeof(float) && Header.datatype == NiftiHeader.NIFTI_TYPE_FLOAT32) return true;
            if (type == typeof(double) && Header.datatype == NiftiHeader.NIFTI_TYPE_FLOAT64) return true;

            return false;
        }

        /// <summary>
        /// Will create a shallow copy of this NIfTI object, which is of the given type.
        /// If the underlying data is not of type T, this method will throw a format exception,
        /// so it's best to use IsType to check the type first.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Nifti<T> AsType<T>()
        {
            if (!IsType(typeof(T))) throw new FormatException("The Nifti is not of type " + typeof(T));

            return new Nifti<T>
            {
                Header = Header,
                Data = (T[])Data
            };
        }

        public float[] ToSingleArray()
        {
            Type type = this.Data.GetType().GetElementType();
            if (type == typeof(float))
                return this.Data as float[];
            else if(type == typeof(double))
                return Array.ConvertAll<double, float>(this.Data as double[], Convert.ToSingle);
            else if(type == typeof(int))
                return Array.ConvertAll<int, float>(this.Data as int[], Convert.ToSingle);
            else if(type == typeof(uint))
                return Array.ConvertAll<uint, float>(this.Data as uint[], Convert.ToSingle);
            else if(type == typeof(short))
                return Array.ConvertAll<short, float>(this.Data as short[], Convert.ToSingle);
            else if(type == typeof(ushort))
                return Array.ConvertAll<ushort, float>(this.Data as ushort[], Convert.ToSingle);
            else
                return null;
        }
    }

    /// <summary>
    /// A typed version of the Nifti data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Nifti<T> : Nifti
    {
        public new T[] Data { get { return ((Nifti)this).Data as T[]; } set { ((Nifti)this).Data = value; } }

        public T this[params int[] idx]
        {
            get { return Data[DataIndexFor(idx)]; }
            set { Data[DataIndexFor(idx)] = value; }
        }
    }
}
