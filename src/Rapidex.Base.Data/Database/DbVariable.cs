using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rapidex.Data
{
    public class DbVariable : DbVariableType
    {
        object value;

        public string FieldName { get; set; }
        public string ParameterName { get; set; }
        public object Value { get { return this.value ?? DBNull.Value; } set { this.value = value; } }


        public DbVariable()
        {
        }

        public DbVariable(DbFieldType dbType) : base(dbType)
        {
        }

        public DbVariable(DbFieldType dbType, int lenght) : base(dbType, lenght)
        {
        }

        public DbVariable(DbFieldType dbType, int lenght, int scale) : base(dbType, lenght, scale)
        {
        }

        public override string ToString()
        {
            return $"{this.FieldName}: {this.Value}";
        }
    }
}
