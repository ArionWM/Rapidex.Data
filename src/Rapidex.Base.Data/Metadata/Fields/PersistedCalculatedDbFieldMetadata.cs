using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Rapidex.Data.Metadata.Columns
{
    public class PersistedCalculatedDbFieldMetadata : DbFieldMetadata, ICalculatedColumnMetadata
    {

        public string Expression { get; set; }

        [YamlIgnore]
        [JsonIgnore]
        public override DbFieldProperties DbProperties { get; set; } = new DbFieldProperties();

        [YamlIgnore]
        [JsonIgnore]
        public override EntityFieldValueGetterDelegate ValueGetterLower { get; set; }

        [YamlIgnore]
        [JsonIgnore]
        public override ValueSetterDelegate ValueSetter { get; set; }

    }
}
