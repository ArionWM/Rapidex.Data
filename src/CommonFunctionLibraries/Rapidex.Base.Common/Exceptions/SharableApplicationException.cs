using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class SharableApplicationException : BaseApplicationException
    {
        public override bool CanShareToUser { get; set; } = true;

        public SharableApplicationException()
        {

        }

        public SharableApplicationException(string message) : base(message)
        {

        }

        public SharableApplicationException(string code, string message) : base(code, message)
        {

        }

        public SharableApplicationException(string code, string message, string helpContent) : base(code, message, helpContent)
        {
        }

        public SharableApplicationException(Exception innerException) : base(innerException)
        {
        }

        public SharableApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
