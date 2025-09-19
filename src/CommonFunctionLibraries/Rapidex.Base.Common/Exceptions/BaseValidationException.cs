using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class BaseValidationException : BaseApplicationException
    {
        public BaseValidationException()
        {
        }

        public BaseValidationException(string message) : base(message)
        {
        }

        public BaseValidationException(Exception innerException) : base(innerException)
        {
        }

        public BaseValidationException(string code, string message) : base(code, message)
        {
        }

        public BaseValidationException(string code, string message, string helpContent) : base(code, message, helpContent)
        {
        }
    }
}
