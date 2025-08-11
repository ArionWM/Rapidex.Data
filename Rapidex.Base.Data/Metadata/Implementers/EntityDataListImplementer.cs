using Rapidex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata.Implementers;
/// <summary>
/// Ayrı duran ve !data ile işaretlenmiş olan veri listesi
/// </summary>
internal class EntityDataListImplementer : IImplementer, IEmptyCheckObject
{
    public virtual bool IsEmpty => this.Data.IsNullOrEmpty();
    public string[] SupportedTags => new string[] { "!data", "data" };

    public virtual bool Implemented { get; set; }

    public virtual bool? ForceAllValues { get; set; }
    public virtual List<EntityDataItemImplementer> Data { get; set; } = new List<EntityDataItemImplementer>();


    public EntityDataListImplementer()
    {

    }


    public virtual IUpdateResult Implement(IImplementHost host, IImplementer parentImplementer, ref object target)
    {
        //ModuleMetadataImplementer mmi = host as ModuleMetadataImplementer;
        //var moduleDef = mmi.ModuleDefinition;

        //IList<IEntity> entityList = null;
        //switch (target)
        //{
        //    case IList<IEntity> list:
        //        entityList = list;
        //        break;
        //    case IApplicationModuleDefinition mdef:
        //        entityList = moduleDef.Data;
        //        break;
        //    default:
        //        throw new InvalidOperationException($"Target type '{target.GetType().Name}' is not supported.");
        //}

        //entityList.NotNull();

        UpdateResult ures = new UpdateResult();

        foreach (EntityDataItemImplementer imp in this.Data)
        {
            object entityObj = null;
            imp.Implement(host, this, ref entityObj);
            IEntity entity = entityObj.ShouldSupportTo<IEntity>();

            if (host.Parent.Data.Any(aent => aent.IsEqual(entity)))
                continue;

            ures.Added(entity);
            host.Parent.Data.Add(entity);
        }

        this.Implemented = true;

        return ures;

    }

    public static implicit operator EntityDataListImplementer(string content)
    {
        if (content.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(content));

        throw new NotSupportedException();

        //EntityDataNestedListImplementer imp = TypeHelper.CreateInstanceWithDI<EntityDataNestedListImplementer>();
        //imp.SourceFileName = content;
        //return imp;
    }

}
