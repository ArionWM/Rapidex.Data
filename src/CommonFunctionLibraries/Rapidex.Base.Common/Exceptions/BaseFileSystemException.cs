using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex
{
    public class BaseFileSystemException : BaseApplicationException
    {
        public BaseFileSystemException()
        {
        }

        public BaseFileSystemException(string message) : base(message)
        {
        }

        public BaseFileSystemException(Exception innerException) : base(innerException)
        {
        }

        public BaseFileSystemException(string code, string message) : base(code, message)
        {
        }

        public BaseFileSystemException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public BaseFileSystemException(string code, string message, string helpContent) : base(code, message, helpContent)
        {
        }
    }
}
