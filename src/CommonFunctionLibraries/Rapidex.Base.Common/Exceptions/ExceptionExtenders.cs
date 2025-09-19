using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public static class ExceptionExtenders
    {
        public static Exception Translate(this Exception ex)
        {
            return Rapidex.Common.ExceptionManager.Translate(ex);
        }
    }
}
