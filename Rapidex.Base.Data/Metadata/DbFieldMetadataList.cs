using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class DbFieldMetadataList : ComponentDictionary<IDbFieldMetadata>
    {
        public IDbEntityMetadata EntityMetadata { get; }

        //public TwoKeyList<Type, IDbFieldMetadata> FieldsByType { get; } = new TwoKeyList<Type, IDbFieldMetadata>();

        public DbFieldMetadataList(IDbEntityMetadata entityMetadata)
        {
            EntityMetadata = entityMetadata;
        }

        internal DbFieldMetadataList AddIfNotExist(IDbFieldMetadata fm)
        {
            if (this.ContainsKey(fm.Name))
            {
                //for debug
            }   
            else
            {
                this.Add(fm.Name, fm);
            }
            return this;

        }
    }
}
