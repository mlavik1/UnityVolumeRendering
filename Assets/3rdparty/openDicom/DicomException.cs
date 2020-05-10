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


    $Id: DicomException.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;


namespace openDicom
{

    /// <summary>
    ///     Parent class of all DICOM related exceptions.
    /// </summary>
    /// <remarks>
    ///     This class will read from <see cref="DicomContext" />, if
    ///     insufficient context is given.
    /// </remarks>
    public class DicomException: Exception
    {
        private long streamPosition = -1;
        /// <summary>
        ///     Current position within a DICOM stream.
        /// </summary>
        /// <remarks>
        ///     If the current stream is undefined or unknown, a stream position
        ///     of -1 will be returned.
        /// </remarks>
        public long StreamPosition
        {
            get { return streamPosition; }
        }

        private Tag tag = null;
        /// <summary>
        ///     Current DICOM tag.
        /// </summary>
        /// <remarks>
        ///     If the current tag is undefined or unknown, null will be returned.
        /// </remarks>
        public Tag Tag
        {
            get { return tag; }
        }

        private string paramName = null;
        /// <summary>
        ///     Name of an additional parameter.
        /// </summary>
        public string ParamName
        {
            get { return paramName; }
        }

        private string paramValue = null;
        /// <summary>
        ///     Value of an additional parameter.
        /// </summary>
        public string ParamValue
        {
            get { return paramValue; }
        }


        public DicomException(): 
            this("An exception occurred.", DicomContext.CurrentTag, 
                DicomContext.StreamPosition, null, null, null) {}

        public DicomException(Exception innerException): 
            this("An exception occurred.", DicomContext.CurrentTag, 
                DicomContext.StreamPosition, null, null, innerException) {}

        public DicomException(string message): 
            this(message, DicomContext.CurrentTag, DicomContext.StreamPosition, 
                null, null, null) {}

        public DicomException(string message, Exception innerException): 
            this(message, DicomContext.CurrentTag, DicomContext.StreamPosition, 
                null, null, innerException) {}

        public DicomException(string message, long streamPosition):
            this(message, DicomContext.CurrentTag, streamPosition, null, null,
                null) {}

        public DicomException(string message, long streamPosition, 
            Exception innerException):
            this(message, DicomContext.CurrentTag, streamPosition, null, null, 
                innerException) {}

        public DicomException(string message, Tag tag):
            this(message, tag, DicomContext.StreamPosition, null, null, null) {}

        public DicomException(string message, Tag tag, Exception innerException):
            this(message, tag, DicomContext.StreamPosition, null, null, 
                innerException) {}

        public DicomException(string message, string paramName): 
            this(message, DicomContext.CurrentTag, DicomContext.StreamPosition, 
                paramName, null, null) {}

        public DicomException(string message, string paramName, 
            Exception innerException): 
            this(message, DicomContext.CurrentTag, DicomContext.StreamPosition, 
                paramName, null, innerException) {}

        public DicomException(string message, long streamPosition, 
            string paramName):
            this(message, DicomContext.CurrentTag, streamPosition, paramName,
                null, null) {}

        public DicomException(string message, long streamPosition, 
            string paramName, Exception innerException):
            this(message, DicomContext.CurrentTag, streamPosition, paramName,
                null, innerException) {}

        public DicomException(string message, Tag tag, string paramName):
            this(message, tag, DicomContext.StreamPosition, paramName, null,
                null) {}

        public DicomException(string message, Tag tag, string paramName, 
            Exception innerException):
            this(message, tag, DicomContext.StreamPosition, paramName, null,
                null) {}

        public DicomException(string message, string paramName, 
            string paramValue): 
            this(message, DicomContext.CurrentTag, DicomContext.StreamPosition, 
                paramName, paramValue, null) {}

        public DicomException(string message, string paramName, 
            string paramValue, Exception innerException): 
            this(message, DicomContext.CurrentTag, DicomContext.StreamPosition, 
                paramName, paramValue, innerException) {}

        public DicomException(string message, long streamPosition, 
            string paramName, string paramValue):
            this(message, DicomContext.CurrentTag, streamPosition, paramName, 
                paramValue, null) {}

        public DicomException(string message, long streamPosition, 
            string paramName, string paramValue, Exception innerException):
            this(message, DicomContext.CurrentTag, streamPosition, paramName, 
                paramValue, innerException) {}

        public DicomException(string message, Tag tag, string paramName, 
            string paramValue):
            this(message, tag, DicomContext.StreamPosition, paramName, 
                paramValue, null) {}

        public DicomException(string message, Tag tag, string paramName, 
            string paramValue, Exception innerException):
            this(message, tag, DicomContext.StreamPosition, paramName, 
                paramValue, innerException) {}

        public DicomException(string message, Tag tag, long streamPosition,
            string paramName, string paramValue):
            this(message, tag, streamPosition, paramName, paramValue, null) {}

        public DicomException(string message, Tag tag, long streamPosition,
            string paramName, string paramValue, Exception innerException):
            base(message, innerException)
        {
            this.tag = tag;
            if (streamPosition < -1)
                this.streamPosition = -1;
            else
                this.streamPosition = streamPosition;
            this.paramName = paramName;
            this.paramValue = paramValue;
        }

        /// <summary>
        ///     Returns a DICOM exception as string with the current DICOM tag,
        ///     current DICOM stream position and parameter value, if known.
        /// </summary>
        public override string ToString()
        {
            string context = "";
            if (Tag != null) 
                context += string.Format("   {0,-15} {1}\n", "Tag:", Tag);
            if (StreamPosition > -1) 
                context += string.Format("   {0,-15} {1}\n", "StreamPosition:",
                    StreamPosition);
            if (ParamName != null) 
                context += string.Format("   {0,-15} {1}\n", "ParamName:", 
                    ParamName);
            if (ParamValue != null) 
                context += string.Format("   {0,-15} {1}\n", "ParamValue:", 
                    ParamValue);
            return string.Format("{0}:\n   {1}\n" +
                "Context:\n{2}" +
                "StackTrace:\n{3}",
                GetType().ToString(), base.Message, context, base.StackTrace);
        }
    }

}
