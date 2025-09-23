using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Rapidex.Data.SqlServer
{
    internal class ConfigurationValidator
    {
        protected IValidationResult ValidateNonMaster(IDbProvider provider)
        {
            ValidationResult vr = new ValidationResult();

            //...

            return vr;
        }

        protected IValidationResult ValidateMaster(IDbProvider provider)
        {
            ValidationResult vr = new ValidationResult();

            //...

            vr.Merge(this.ValidateNonMaster(provider));

            return vr;
        }

        public IValidationResult Validate(IDbProvider provider)
        {
            if (provider.ParentScope.ParentDbScope.Name == DatabaseConstants.MASTER_DB_ALIAS_NAME)
            {
                return this.ValidateMaster(provider);
            }
            else
            {
                return this.ValidateNonMaster(provider);
            }
        }
    }
}
