using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public abstract class DataExceptionBase : BaseApplicationException
    {
        protected DataExceptionBase()
        {
        }

        protected DataExceptionBase(Exception innerException) : base(innerException)
        {
        }

        protected DataExceptionBase(string code, string message) : base(code, message)
        {
        }

        protected DataExceptionBase(string code, string message, string helpContent) : base(code, message, helpContent)
        {
        }
    }
}
