//using Rapidex.Data.SerializationAndMapping.MetadataImplementers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Rapidex.Data.Metadata;
//internal class EntityMetadataBuilderFromJson : EntityMetadataBuilderBase
//{
//    public EntityMetadataBuilderFromJson(IDbEntityMetadataFactory dbEntityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory) : base(dbEntityMetadataFactory, fieldMetadataFactory)
//    {
//    }

//    public EntityMetadataBuilderFromJson(IDbEntityMetadataFactory dbEntityMetadataFactory, IFieldMetadataFactory fieldMetadataFactory, IDbMetadataContainer parent) : base(dbEntityMetadataFactory, fieldMetadataFactory, parent)
//    {
//    }

//    public IDbEntityMetadata AddJson(string json)
//    {
//        EntityDefinitionImplementer imp = json.FromJson<EntityDefinitionImplementer>();
//        object target = null;
//        imp.Implement(this, null, ref target);
//        return target as IDbEntityMetadata;
//    }
//}
