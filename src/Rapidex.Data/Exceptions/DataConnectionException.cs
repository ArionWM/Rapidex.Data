using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class DataConnectionException : DataExceptionBase
    {
        public DataConnectionException()
        {
        }

        public DataConnectionException(Exception innerException) : base(innerException)
        {
        }

        public DataConnectionException(string message) : base(ExceptionCodes.ERR_CONNECTIONFAIL, message)
        {
        }

        public DataConnectionException(string message, string helpContent) : base(ExceptionCodes.ERR_CONNECTIONFAIL, message, helpContent)
        {
        }
    }
}
