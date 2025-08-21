
using Rapidex.Base.Common.Assemblies;
using Rapidex.Data.Metadata;
using Rapidex.Data.Metadata.Implementers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public static class ImplementerExtender
{

    public static IDbEntityMetadata Add<T>(this IDbMetadataContainer mContainer, string module = null, string prefix = null) where T : IConcreteEntity
    {
        IDbEntityMetadataFactory mf = Rapidex.Common.ServiceProvider.GetRapidexService<IDbEntityMetadataFactory>();
        IFieldMetadataFactory ff = Rapidex.Common.ServiceProvider.GetRapidexService<IFieldMetadataFactory>();

        EntityMetadataBuilderFromConcrete cmi = new(mContainer, mf, ff);
        return cmi.Add(typeof(T), module, prefix);
    }


    public static IDbEntityMetadata AddEnum<E>(this IDbMetadataContainer mContainer, string module = null, string prefix = null) where E : Enum
    {
        IDbEntityMetadataFactory mf = Rapidex.Common.ServiceProvider.GetRapidexService<IDbEntityMetadataFactory>();
        IFieldMetadataFactory ff = Rapidex.Common.ServiceProvider.GetRapidexService<IFieldMetadataFactory>();

        EntityMetadataBuilderFromConcrete cmi = new(mContainer, mf, ff);
        return cmi.Add(typeof(E), module, prefix);
    }

    public static IDbEntityMetadata Add(this IDbMetadataContainer mContainer, Type concreteType, string module = null, string prefix = null) 
    {
        IDbEntityMetadataFactory mf = Rapidex.Common.ServiceProvider.GetRapidexService<IDbEntityMetadataFactory>();
        IFieldMetadataFactory ff = Rapidex.Common.ServiceProvider.GetRapidexService<IFieldMetadataFactory>();

        EntityMetadataBuilderFromConcrete cmi = new(mContainer, mf, ff);
        return cmi.Add(concreteType, module, prefix);
    }

    public static IDbEntityMetadata AddIfNotExist(this IDbMetadataContainer mContainer, Type concreteType, string module = null, string prefix = null)
    {
        var _em = mContainer.Get(concreteType.Name);
        if (_em == null || _em.IsPremature)
        {
           _em = mContainer.Add(concreteType, module, prefix);

        }
        return _em;
    }

    public static IDbEntityMetadata[] Add(this IDbMetadataContainer mContainer, AssemblyInfo ainfo)
    {
        IDbEntityMetadataFactory mf = Rapidex.Common.ServiceProvider.GetRapidexService<IDbEntityMetadataFactory>();
        IFieldMetadataFactory ff = Rapidex.Common.ServiceProvider.GetRapidexService<IFieldMetadataFactory>();

        EntityMetadataBuilderFromConcrete cmi = new(mContainer, mf, ff);
        List<IDbEntityMetadata> entities = new List<IDbEntityMetadata>();
        var types = Common.Assembly.FindDerivedClassTypes(ainfo.Assembly, typeof(IConcreteEntity));
        foreach (Type type in types)
        {
            entities.Add(
                cmi.Add(type, ainfo.NavigationName, ainfo.DatabaseEntityPrefix));
        }
        return entities.ToArray();
    }

    public static IDbEntityMetadata[] ScanConcreteDefinitions(this IDbMetadataContainer mContainer)
    {
        mContainer.NotNull();

        IDbEntityMetadataFactory mf = Rapidex.Common.ServiceProvider.GetRapidexService<IDbEntityMetadataFactory>();
        IFieldMetadataFactory ff = Rapidex.Common.ServiceProvider.GetRapidexService<IFieldMetadataFactory>();

        EntityMetadataBuilderFromConcrete cmi = new(mContainer, mf, ff);
        List<IDbEntityMetadata> entities = new List<IDbEntityMetadata>();
        var aInfos = Common.Assembly.AssemblyDefinitions;
        foreach (AssemblyInfo ainfo in aInfos)
        {
            var types = Common.Assembly.FindDerivedClassTypes(ainfo.Assembly, typeof(IConcreteEntity));
            foreach (Type type in types)
            {
                if (type.IsAbstract || type.IsInterface)
                    continue; //Skip abstract and interface types

                entities.Add(
                    cmi.Add(type, ainfo.NavigationName, ainfo.DatabaseEntityPrefix));
            }
        }
        return entities.ToArray();
    }

    public static IDbEntityMetadata[] AddJson(this IDbMetadataContainer mContainer, string json)
    {
        IDbEntityMetadataFactory mf = Rapidex.Common.ServiceProvider.GetRapidexService<IDbEntityMetadataFactory>();
        IFieldMetadataFactory ff = Rapidex.Common.ServiceProvider.GetRapidexService<IFieldMetadataFactory>();

        IMetadataImplementHost ihost = Rapidex.Common.ServiceProvider.GetRapidexService<IMetadataImplementHost>();
        ihost.SetParent(mContainer);
        //DefaultMetadataImplementHost cmi = new(mContainer);
        var ures = ihost.AddJson(json);

        var ems = ures.GetModifieds().Where(x => x is IDbEntityMetadata).Cast<IDbEntityMetadata>();
        return ems.ToArray();
    }

    public static IDbEntityMetadata[] AddYaml(this IDbMetadataContainer mContainer, string yaml)
    {
        IDbEntityMetadataFactory mf = Rapidex.Common.ServiceProvider.GetRapidexService<IDbEntityMetadataFactory>();
        IFieldMetadataFactory ff = Rapidex.Common.ServiceProvider.GetRapidexService<IFieldMetadataFactory>();

        IMetadataImplementHost ihost = Rapidex.Common.ServiceProvider.GetRapidexService<IMetadataImplementHost>();
        ihost.SetParent(mContainer);
        
        var ures = ihost.AddYaml(yaml);

        var ems = ures.GetModifieds().Where(x => x is IDbEntityMetadata).Cast<IDbEntityMetadata>();
        return ems.ToArray();
    }

    public static IDbEntityMetadata[] AddFile(this IDbMetadataContainer mContainer, string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        switch (ext)
        {
            case ".json":
                return mContainer.AddJson(File.ReadAllText(filePath));
            case ".yaml":
            case ".yml":
                return mContainer.AddYaml(File.ReadAllText(filePath));
            default:
                throw new NotSupportedException($"File type '{ext}' is not supported for metadata addition.");
        }
    }

    public static IDbEntityMetadata[] ScanDefinitions(this IDbMetadataContainer mContainer, string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"The directory '{folderPath}' does not exist.");

        List<IDbEntityMetadata> entities = new List<IDbEntityMetadata>();

        var files = Directory.GetFiles(folderPath, "*.json|*.yaml|*.yml", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            try
            {
                var ems = AddFile(mContainer, file);
                entities.AddRange(ems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file '{file}': {ex.Message}");
            }
        }
        return entities.ToArray();

    }
}
