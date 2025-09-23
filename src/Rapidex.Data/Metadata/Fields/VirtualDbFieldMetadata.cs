using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Rapidex.Data.Metadata.Columns
{
    /// <summary>
    /// Veritabanına doğrudan yazılmayan sütunlar için kullanılır.
    /// Örn: CalculatedColumn
    /// 
    /// Bazı sanal sütunlar, veritabanına birden fazla gerçek sütun ekler. Örn: Currency, DateTimeStartEnd vb.
    /// </summary>
    public class VirtualDbFieldMetadata : DbFieldMetadata, IDbFieldMetadata
    {
        public override bool IsPersisted { get; set; } = false;

    }
}
