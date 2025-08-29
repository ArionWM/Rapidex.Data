using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data;

[Obsolete("Use AuthorizationChecker")]
internal class DbValidator
{
    [Obsolete("Use AuthorizationChecker")]
    protected IValidationResult ValidateMultiSchemaNonMaster(IDbProvider provider)
    {
        string schemaName = "test" + RandomHelper.RandomText(10);
        var structureMan = provider.GetStructureProvider();
        ValidationResult vr = new ValidationResult();

        try
        {
            structureMan.CreateOrUpdateSchema(schemaName);
        }
        catch (Exception ex)
        {
            vr.Error($"Database schema rights check Fail: {ex.Message}");
        }

        try
        {
            structureMan.DestroySchema(schemaName);
        }
        catch (Exception ex)
        {
            vr.Warning($"Database schema destroy check Fail: {ex.Message}");
        }

        return vr;
    }

    [Obsolete("Use AuthorizationChecker")]
    protected IValidationResult ValidateMultiDbMaster(IDbProvider provider)
    {
        ValidationResult vr = new ValidationResult();

        try
        {
            string dbName = "test" + RandomHelper.RandomText(10);
            var structureMan = provider.GetStructureProvider();

            try
            {
                structureMan.CreateDatabase(dbName);
            }
            catch (Exception ex)
            {
                vr.Error($"Master database config invalid: {ex.Message}");
            }

            try
            {
                structureMan.DestroyDatabase(dbName);
            }
            catch (Exception ex)
            {
                vr.Warning($"Master database config invalid: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            vr.Error($"Master database rights check Fail: {ex.Message}");

        }

        vr.Merge(this.ValidateMultiSchemaNonMaster(provider));

        return vr;
    }

    public IValidationResult ValidateMultiDb(IDbProvider provider)
    {
        if (provider.ParentScope.ParentDbScope.Name == DatabaseConstants.MASTER_DB_NAME)
        {
            return this.ValidateMultiDbMaster(provider);
        }
        else
        {
            return this.ValidateMultiSchemaNonMaster(provider);
        }
    }
}
