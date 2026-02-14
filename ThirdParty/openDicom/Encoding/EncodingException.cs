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


    $Id: EncodingException.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom;
using openDicom.DataStructure;


namespace openDicom.Encoding
{

    /// <summary>
    ///     Parent class for all encoding exceptions.
    /// </summary>
    public class EncodingException: DicomException
    {
        public EncodingException(string paramName, string paramValue): 
            base("An encoding exception occurred.", paramName, paramValue, 
                null) {}

        public EncodingException(string paramName, string paramValue,
            Exception innerException): 
            base("An encoding exception occurred.", paramName, paramValue, 
                innerException) {}

        public EncodingException(string message, string paramName, 
            string paramValue): 
            base(message, paramName, paramValue) {}

        public EncodingException(string message, string paramName, 
            string paramValue, Exception innerException): 
            base(message, paramName, paramValue, innerException) {}

        public EncodingException(string message, long streamPosition, 
            string paramName, string paramValue):
            base(message, streamPosition, paramName, paramValue) {}

        public EncodingException(string message, long streamPosition, 
            string paramName, string paramValue, Exception innerException):
            base(message, streamPosition, paramName, paramValue, 
                innerException) {}

        public EncodingException(string message, Tag tag, string paramName, 
            string paramValue):
            base(message, tag, paramName, paramValue) {}

        public EncodingException(string message, Tag tag, string paramName, 
            string paramValue, Exception innerException):
            base(message, tag, paramName, paramValue, innerException) {}

        public EncodingException(string message, Tag tag, long streamPosition,
            string paramName, string paramValue):
            base(message, tag, streamPosition, paramName, paramValue) {}

        public EncodingException(string message, Tag tag, long streamPosition,
            string paramName, string paramValue, Exception innerException):
            base(message, tag, streamPosition, paramName, paramValue, 
                innerException) {}
    }

}
