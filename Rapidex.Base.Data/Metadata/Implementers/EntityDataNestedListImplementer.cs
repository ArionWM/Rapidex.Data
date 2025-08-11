using Rapidex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Metadata.Implementers;




/// <summary>
/// Bir diğer tanımın içerisinde olan ve "data:" etiketi ile ifade edilen veri listesi
/// </summary>
/// <see cref="">Module.yml</see>
internal class EntityDataNestedListImplementer : List<EntityDataItemImplementer>, IImplementer, IEmptyCheckObject
{
    public string[] SupportedTags => null;

    public virtual bool Implemented { get; set; }

    public virtual bool IsEmpty => this.Count == 0;

    public EntityDataNestedListImplementer()
    {

    }



    public virtual IUpdateResult Implement(IImplementHost host, IImplementer parentImplementer, ref object target)
    {


        UpdateResult ures = new UpdateResult();

        foreach (EntityDataItemImplementer imp in this)
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

    public static implicit operator EntityDataNestedListImplementer(string content)
    {
        if (content.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(content));

        throw new NotSupportedException();

        //EntityDataNestedListImplementer imp = TypeHelper.CreateInstanceWithDI<EntityDataNestedListImplementer>();
        //imp.SourceFileName = content;
        //return imp;
    }
}
