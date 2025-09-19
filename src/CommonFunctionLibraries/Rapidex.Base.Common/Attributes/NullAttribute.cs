using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    /// <summary>
    /// Belirtildiği enum değerinin null anlamına geldiğini belirtir. Bu değer; IsNullOrEmpty: True olarak yorumlanır
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class NullAttribute : Attribute
    {
        public NullAttribute()
        {
        }
    }
}
