using Rapidex.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Scopes;

internal class DefaultDbBlobRepository : IBlobRepository
{
    IDbSchemaScope parentScope;

    public DefaultDbBlobRepository(IDbSchemaScope parentScope)
    {
        this.parentScope = parentScope;
    }

    public IResult<StreamContent> Get(long id)
    {
        BlobRecord rec = this.parentScope.GetQuery<BlobRecord>().Eq("Id", id).First();
        if (rec == null)
        {
            return Result<StreamContent>.Ok(null);
        }

        return Result<StreamContent>.Ok(new StreamContent(rec.Name, rec.ContentType, new MemoryStream(rec.Data)));
    }

    public IEntityUpdateResult Delete(long id)
    {
        EntityUpdateResult ures = new EntityUpdateResult();
        if (id > -1)
        {
            var brec = this.parentScope.GetQuery<BlobRecord>().Eq("Id", id).First();

            if (brec != null)
            {
                var work = this.parentScope.CurrentWork;
                work.Delete(brec);

                ures.Deleted(brec);
                ures.Success = true;
            }
            else
            {
                ures.Success = false;
                ures.Description = "Not found";
            }
        }
        else
        {
            ures.Success = false;
            ures.Description = "Invalid id";
        }

        return ures;
    }

    public IResult<BlobRecord> Set(Stream content, string name, string contentType, long id = -1)
    {
        BlobRecord brec = null;
        if (id > -1)
        {
            brec = this.parentScope.GetQuery<BlobRecord>().Eq("Id", id).First();
        }

        if (content == null)
        { //Delete content

            if (brec != null)
            {
                this.parentScope.CurrentWork.Delete(brec);
            }

            return Result<BlobRecord>.Ok(null);
        }


        if (brec == null)
        {
            brec = this.parentScope.CurrentWork.New<BlobRecord>();
        }

        brec.Data = new byte[content.Length];
        content.Read(brec.Data, 0, brec.Data.Length);

        brec.Name = name;
        brec.ContentType = contentType;
        brec.Length = brec.Data.Length;

        IEntity bsef = brec;
        this.parentScope.CurrentWork.Save(bsef);

        return Result<BlobRecord>.Ok(brec);
    }
}
