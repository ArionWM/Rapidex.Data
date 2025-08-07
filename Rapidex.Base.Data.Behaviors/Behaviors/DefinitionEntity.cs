using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class DefinitionEntity : EntityBehaviorBase<DefinitionEntity>
    {
        public override string Descripton => "Ensures that the Entity has Name and Description fields. 'Ayarlar' sayfalarında görüntülenen, arşivlenen ve geri alınabilen kayıtlardır";

        public DefinitionEntity()
        {
        }

        public DefinitionEntity(IEntity entity) : base(entity)
        {
        }

        public override IUpdateResult SetupMetadata(IDbEntityMetadata em)
        {
            em.AddBehavior<ArchiveEntity>(true, false);

            em.AddFieldIfNotExist<string>("Name");
            em.AddFieldIfNotExist<Text>("Description");

            return new UpdateResult();
        }
    }
}
