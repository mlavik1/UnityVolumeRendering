/*
   
    openDICOM.NET openDICOM# 0.1.1

    openDICOM# provides a library for DICOM related development on Mono.
    Copyright (C) 2006-2007  Albert Gnandt
 
    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA


    $Id: ByteConvert.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.Text;


namespace openDicom.Encoding
{

    using System.Text;

    /// <summary>
    ///     Byte arrays conversion methods.
    /// </summary>
    public class ByteConvert
    {
        /// <summary>
        ///     Converts an array of bytes into a string by given DICOM
        ///     character repertoire.
        /// </summary>
        public static string ToString(byte[] bytes, 
            CharacterRepertoire characterRepertoire)
        {
            return ToString(bytes, 0, bytes.Length, characterRepertoire);
        }

        /// <summary>
        ///     Converts count of bytes from a byte array into a string by
        ///     given DICOM character repertoire.
        /// </summary>
        public static string ToString(byte[] bytes, int count, 
            CharacterRepertoire characterRepertoire)
        {
            return ToString(bytes, 0, count, characterRepertoire);
        }

        /// <summary>
        ///     Converts count of bytes from a byte array starting at a
        ///     specified offset into a string by given DICOM character
        ///     repertoire.
        /// </summary>
        public static string ToString(byte[] bytes, int offset, int count,
            CharacterRepertoire characterRepertoire)
        {
            if (characterRepertoire != null)
                return characterRepertoire.Encoding.GetString(bytes, offset,
                    count);
            else
                throw new EncodingException("characterRepertoire", "null");
        }

        /// <summary>
        ///     Converts a string to an array of bytes by given DICOM
        ///     character repertoire.
        /// </summary>
        public static byte[] ToBytes(string s,
            CharacterRepertoire characterRepertoire)
        {
            if (characterRepertoire != null)
                return characterRepertoire.Encoding.GetBytes(s);
            else
                throw new EncodingException("characterRepertoire", "null");
        }

        /// <summary>
        ///     Converts an array of unsigned words into an array of bytes.
        /// </summary>
        public static byte[] ToBytes(ushort[] words)
        {
            byte[] bytes = new byte[words.Length * 2];
            for (int i = 0; i < words.Length; i++)
                Array.Copy(BitConverter.GetBytes(words[i]), 0, 
                    bytes, i * 2, 2);
            return bytes;
        }

        /// <summary>
        ///     Converts an array of signed words into an array of bytes.
        /// </summary>
        public static byte[] ToBytes(short[] words)
        {
            byte[] bytes = new byte[words.Length * 2];
            for (int i = 0; i < words.Length; i++)
                Array.Copy(BitConverter.GetBytes(words[i]), 0, 
                    bytes, i * 2, 2);
            return bytes;
        }

        /// <summary>
        ///     Converts an array of bytes into an array of unsigned words.
        /// </summary>
        public static ushort[] ToUnsignedWords(byte[] bytes)
        {
            if (bytes.Length % 2 == 0)
            {
                ushort[] words = new ushort[bytes.Length / 2];
                byte[] buffer = new byte[2];
                for (int i = 0; i < words.Length; i++)
                    words[i] = BitConverter.ToUInt16(buffer, i * 2);
                return words;
            }
            else
                throw new EncodingException("Odd count of bytes. Cannot " +
                    "convert to words.", "bytes.Length", 
                    bytes.Length.ToString());
        }

        /// <summary>
        ///     Converts an array of bytes into an array of signed words.
        /// </summary>
        public static short[] ToSignedWords(byte[] bytes)
        {
            if (bytes.Length % 2 == 0)
            {
                short[] words = new short[bytes.Length / 2];
                byte[] buffer = new byte[2];
                for (int i = 0; i < words.Length; i++)
                    words[i] = BitConverter.ToInt16(buffer, i * 2);
                return words;
            }
            else
                throw new EncodingException("Odd count of bytes. Cannot " +
                    "convert to words.", "bytes.Length", 
                    bytes.Length.ToString());
        }

        /// <summary>
        ///     Changes between little and big endian byte ordering of an
        ///     unsigned word and returns it.
        /// </summary>
        public static ushort SwapBytes(ushort word)
        {
            byte[] bytes = BitConverter.GetBytes(word);
            bytes = SwapBytes(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        ///     Changes between little and big endian byte ordering of a
        ///     signed word and returns it.
        /// </summary>
        public static short SwapBytes(short word)
        {
            byte[] bytes = BitConverter.GetBytes(word);
            bytes = SwapBytes(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        ///     Changes between little and big endian byte ordering of an
        ///     unsigned integer and returns it.
        /// </summary>
        public static uint SwapBytes(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            bytes = SwapBytes(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        ///     Changes between little and big endian byte ordering of a
        ///     signed integer and returns it.
        /// </summary>
        public static int SwapBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            bytes = SwapBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        ///     Changes between little and big endian byte ordering of an
        ///     unsigned long integer and returns it.
        /// </summary>
        public static ulong SwapBytes(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            bytes = SwapBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>
        ///     Changes between little and big endian byte ordering of a
        ///     signed long integer and returns it.
        /// </summary>
        public static long SwapBytes(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            bytes = SwapBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        ///     Changes between little and big endian byte ordering of a
        ///     single or float and returns it.
        /// </summary>
        public static float SwapBytes(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            bytes = SwapBytes(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        ///     Changes between little and big endian byte ordering of a
        ///     double and returns it.
        /// </summary>
        public static double SwapBytes(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            bytes = SwapBytes(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        ///     Changes between little and big endian byte ordering of an
        ///     array of bytes and returns it.
        /// </summary>
        public static byte[] SwapBytes(byte[] bytes)
        {
            byte[] buffer = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
                buffer[i] = bytes[bytes.Length - 1 - i];
            return buffer;
        }
    }

}
