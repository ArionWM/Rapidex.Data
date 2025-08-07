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
        List<IEntityChangeResultItem> _modifieds;
        List<IEntityChangeResultItem> _addeds;
        List<IEntityChangeResultItem> _deleteds;

        public IReadOnlyList<IEntityChangeResultItem> ModifiedItems => _modifieds.AsReadOnly();

        public IReadOnlyList<IEntityChangeResultItem> AddedItems => _addeds.AsReadOnly();

        public IReadOnlyList<IEntityChangeResultItem> DeletedItems => _deleteds.AsReadOnly();

        public bool Success { get; set; }
        public string Description { get; set; }

        public EntityUpdateResult()
        {
            _modifieds = new List<IEntityChangeResultItem>();
            _addeds = new List<IEntityChangeResultItem>();
            _deleteds = new List<IEntityChangeResultItem>();
        }

        public void Modified(IEntity ent)
        {
            _modifieds.Add(new EntityChangeResultItem() { Name = ent._Metadata.Name, Id = (long)ent.GetId(), OldId = (long)ent.GetId(), ExternalId = ent.ExternalId });
        }

        public void Modified(long id)
        {
            _modifieds.Add(new EntityChangeResultItem() { Id = id });
        }

        public void Modified(IEntityChangeResultItem itm)
        {
            _modifieds.Add(itm);
        }

        public void Added(IEntity ent, long oldId = 0)
        {
            _addeds.Add(new EntityChangeResultItem() { Name = ent._Metadata.Name, Id = (long)ent.GetId(), OldId = oldId == 0 ? (long)ent.GetId() : oldId, ExternalId = ent.ExternalId });
        }

        public void Added(IEntityChangeResultItem itm)
        {
            _addeds.Add(itm);
        }

        public void Deleted(IEntity ent)
        {
            _deleteds.Add(new EntityChangeResultItem() { Name = ent._Metadata.Name, Id = (long)ent.GetId(), OldId = (long)ent.GetId(), ExternalId = ent.ExternalId });
        }

        public void Deleted(string entityName, long id)
        {
            _deleteds.Add(new EntityChangeResultItem() { Name = entityName, Id = id });
        }

        public void Deleted(IEntityChangeResultItem itm)
        {
            _deleteds.Add(itm);
        }

        public void MergeWith(IUpdateResult<IEntityChangeResultItem> with)
        {
            _modifieds.AddRange(with.ModifiedItems);
            _addeds.AddRange(with.AddedItems);
            _deleteds.AddRange(with.DeletedItems);
        }
    }
}
