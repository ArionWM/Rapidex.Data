using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class MetadataException : DataExceptionBase
    {
        public MetadataException()
        {
        }

        public MetadataException(Exception innerException) : base(innerException)
        {
        }

        public MetadataException(string message) : base(ExceptionCodes.ERR_INVALIDMETADATA, message)
        {
        }

        public MetadataException(string message, string helpContent) : base(ExceptionCodes.ERR_INVALIDMETADATA, message, helpContent)
        {
        }
    }
}
