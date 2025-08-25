using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.Metadata
{
    public class DbEntityMetadataFactory : IDbEntityMetadataFactory
    {
        public virtual IDbEntityMetadata Create(string entityName, string module = null, string prefix = null)
        {
            return new DbEntityMetadata(entityName, module, prefix);
        }

        public void Setup(IServiceCollection services)
        {

        }

        public void Start(IServiceProvider serviceProvider)
        {

        }
    }
}
