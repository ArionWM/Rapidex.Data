using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Exceptions;
internal class EntityAttachScopeException : DataExceptionBase
{
    public EntityAttachScopeException()
    {
    }

    public EntityAttachScopeException(Exception innerException) : base(innerException)
    {
    }

    public EntityAttachScopeException(string code, string message) : base(code, message)
    {
    }

    public EntityAttachScopeException(string code, string message, string helpContent) : base(code, message, helpContent)
    {
    }
}
