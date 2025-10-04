using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Rapidex.Data
{
    public class EntityChangeResultItem : IEntityChangeResultItem
    {
        [JsonPropertyOrder(-9000)]
        public string Name { get; set; }

        public long Id { get; set; }

        public long OldId { get; set; }

        public string ExternalId { get; set; }
    }

    public class EntityUpdateResult : IEntityUpdateResult
    {
        List<IEntityChangeResultItem> modifieds;
        List<IEntityChangeResultItem> addeds;
        List<IEntityChangeResultItem> deleteds;

        public IReadOnlyList<IEntityChangeResultItem> ModifiedItems => modifieds.AsReadOnly();

        public IReadOnlyList<IEntityChangeResultItem> AddedItems => addeds.AsReadOnly();

        public IReadOnlyList<IEntityChangeResultItem> DeletedItems => deleteds.AsReadOnly();

        public bool Success { get; set; }
        public string Description { get; set; }

        public EntityUpdateResult()
        {
            modifieds = new List<IEntityChangeResultItem>();
            addeds = new List<IEntityChangeResultItem>();
            deleteds = new List<IEntityChangeResultItem>();
        }

        public void Modified(IEntity ent)
        {
            modifieds.Add(new EntityChangeResultItem() { Name = ent._Metadata.Name, Id = (long)ent.GetId(), OldId = (long)ent.GetId(), ExternalId = ent.ExternalId });
        }

        public void Modified(long id)
        {
            modifieds.Add(new EntityChangeResultItem() { Id = id });
        }

        public void Modified(IEntityChangeResultItem itm)
        {
            modifieds.Add(itm);
        }

        public void Added(IEntity ent, long oldId = 0)
        {
            addeds.Add(new EntityChangeResultItem() { Name = ent._Metadata.Name, Id = (long)ent.GetId(), OldId = oldId == 0 ? (long)ent.GetId() : oldId, ExternalId = ent.ExternalId });
        }

        public void Added(IEntityChangeResultItem itm)
        {
            addeds.Add(itm);
        }

        public void Deleted(IEntity ent)
        {
            deleteds.Add(new EntityChangeResultItem() { Name = ent._Metadata.Name, Id = (long)ent.GetId(), OldId = (long)ent.GetId(), ExternalId = ent.ExternalId });
        }

        public void Deleted(string entityName, long id)
        {
            deleteds.Add(new EntityChangeResultItem() { Name = entityName, Id = id });
        }

        public void Deleted(IEntityChangeResultItem itm)
        {
            deleteds.Add(itm);
        }

        public void MergeWith(IUpdateResult<IEntityChangeResultItem> with)
        {
            modifieds.AddRange(with.ModifiedItems);
            addeds.AddRange(with.AddedItems);
            deleteds.AddRange(with.DeletedItems);
        }
    }
}
