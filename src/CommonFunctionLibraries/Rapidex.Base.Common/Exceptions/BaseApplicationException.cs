using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class BaseApplicationException : Exception
    {
        /// <summary>
        /// True: Exception metni ve kodunun dış dünya ile paylaşılabileceğini ifade eder.
        /// False: Sadece test sunucularında exception metninin dış dünyaya yayınlanmasına izin verilir.
        /// </summary>
        public virtual bool CanShareToUser { get; set; } = false;
        public virtual bool StopModule { get; set; } = false;
        public virtual string ExceptionCode { get; }

        public BaseApplicationException()
        {
        }

        public BaseApplicationException(string message) : base(message)
        {
        }

        public BaseApplicationException(string code, string message) : base(message)
        {
            this.ExceptionCode = code;
        }

        public BaseApplicationException(string code, string message, string helpContent) : this(code, message)
        {
            this.HelpLink = helpContent;
        }

        public BaseApplicationException(Exception innerException) : base(innerException.Message, innerException)
        {

        }

        public BaseApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
