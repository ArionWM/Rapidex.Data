using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Exceptions;
internal class DataSerializationException : DataExceptionBase
{
    public DataSerializationException()
    {
    }

    public DataSerializationException(Exception innerException) : base(innerException)
    {
    }

    public DataSerializationException(string code, string message) : base(code, message)
    {
    }

    public DataSerializationException(string code, string message, string helpContent) : base(code, message, helpContent)
    {
    }
}
