using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rapidex.Data
{
    public class DbVariableType
    {
        public DbFieldType DbType { get; set; }
        public int Lenght { get; set; } = 24;//Precision
        public int Scale { get; set; } = 8;

        public bool IsString { get { return this.DbType == DbFieldType.String || this.DbType == DbFieldType.AnsiString || this.DbType == DbFieldType.StringFixedLength || this.DbType == DbFieldType.AnsiStringFixedLength; } }

        public bool IsBinary { get { return this.DbType == DbFieldType.Binary; } }

        public DbVariableType()
        {

        }

        public DbVariableType(DbFieldType dbType, int lenght, int scale)
        {
            this.DbType = dbType;
            this.Lenght = lenght;
            this.Scale = scale;

            if (this.IsString && lenght >= 4000)
                this.Lenght = -1;

            if(this.IsBinary && lenght >= 4000)
                this.Lenght = -1;
        }

        public DbVariableType(DbFieldType dbType, int lenght)
        {
            this.DbType = dbType;
            this.Lenght = lenght;
            this.Scale = 0;

            if (this.IsString && lenght >= 4000)
                this.Lenght = -1;

            if (this.IsBinary && lenght >= 4000)
                this.Lenght = -1;
        }

        public DbVariableType(DbFieldType dbType)
        {
            this.DbType = dbType;
            this.Lenght = 0;
            this.Scale = 0;
        }

        public override string ToString()
        {
            return $"{this.DbType}";
        }
    }
}
