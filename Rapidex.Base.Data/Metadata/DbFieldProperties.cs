using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class DbFieldProperties
    {
        public bool IsNullable { get; set; } = true;
        public int Length { get; set; } //Precision
        public int Scale { get; set; }


    }
}
