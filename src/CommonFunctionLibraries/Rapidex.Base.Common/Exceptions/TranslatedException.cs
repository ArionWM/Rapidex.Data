using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex;

public class TranslatedException : BaseApplicationException
{
    public ExceptionTarget ExceptionTarget { get; }

    public TranslatedException()
    {
    }

    public TranslatedException(ExceptionTarget exceptionTarget, string message ) : base(message)
    {
        this.ExceptionTarget = exceptionTarget;
    }

    public TranslatedException(ExceptionTarget exceptionTarget, Exception innerException) : base(innerException)
    {
        this.ExceptionTarget = exceptionTarget;
    }

    public TranslatedException(ExceptionTarget exceptionTarget, string code, string message) : base(code, message)
    {
        this.ExceptionTarget = exceptionTarget;
    }

    public TranslatedException(ExceptionTarget exceptionTarget, string message, Exception innerException) : base(message, innerException)
    {
        this.ExceptionTarget = exceptionTarget;
    }

    public TranslatedException(ExceptionTarget exceptionTarget, string code, string message, string helpContent) : base(code, message, helpContent)
    {
        this.ExceptionTarget = exceptionTarget;
    }
}
