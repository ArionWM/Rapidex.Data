using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class BaseVariableRequiredException : BaseApplicationException
    {
        public BaseVariableRequiredException()
        {
        }

        public BaseVariableRequiredException(string variableName) : base(variableName + " can't be empty")
        {
        }

        public BaseVariableRequiredException(Exception innerException) : base(innerException)
        {
        }

        //public BaseVariableRequiredException(string code, string variableName) : base(code, variableName)
        //{
        //}

        //public BaseVariableRequiredException(string code, string variableName, string helpContent) : base(code, variableName, helpContent)
        //{
        //}
    }
}
