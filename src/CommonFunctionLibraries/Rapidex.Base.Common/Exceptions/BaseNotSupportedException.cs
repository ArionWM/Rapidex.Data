using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class BaseNotSupportedException : BaseApplicationException
    {

        public BaseNotSupportedException()
        {
        }

        public BaseNotSupportedException(string message) : base("Not supported: " + message)
        {
        }

        public BaseNotSupportedException(string code, string message) : base(code, "Not supported: " + message)
        {
        }

        public BaseNotSupportedException(string code, string message, string helpContent) : base(code, "Not supported: " + message, helpContent)
        {
        }
    }
}
