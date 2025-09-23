using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
internal class WorkScopeNotAvailableException : DataExceptionBase
{
    public WorkScopeNotAvailableException()
    {
    }

    public WorkScopeNotAvailableException(Exception innerException) : base(innerException)
    {
    }

    public WorkScopeNotAvailableException(string code, string message) : base(code, message)
    {
    }

    public WorkScopeNotAvailableException(string code, string message, string helpContent) : base(code, message, helpContent)
    {
    }
}
