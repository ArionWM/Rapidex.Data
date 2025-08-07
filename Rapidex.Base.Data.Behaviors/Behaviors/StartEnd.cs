using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{
    public class StartEnd : EntityBehaviorBase<StartEnd>
    {

        public const string FIELD_START_TIME = "StartTime";
        public const string FIELD_END_TIME = "EndTime";

        public override string Descripton => "Ensures that the Entity has StartTime and EndTime fields and StartTime < EndTime validation rules ";

        public StartEnd()
        {
        }

        public StartEnd(IEntity entity) : base(entity)
        {
        }

        public DateTimeOffset StartTime
        {
            get
            {
                return this.Entity.GetValue<DateTimeOffset>(FIELD_START_TIME);
            }
            set
            {
                this.Entity.SetValue(FIELD_START_TIME, value);
            }
        }

        public DateTimeOffset EndTime
        {
            get
            {
                return this.Entity.GetValue<DateTimeOffset>(FIELD_END_TIME);
            }
            set
            {
                this.Entity.SetValue(FIELD_END_TIME, value);
            }
        }

        public override IUpdateResult SetupMetadata(IDbEntityMetadata em)
        {
            em.AddFieldIfNotExist<DateTimeOffset>(FIELD_START_TIME);
            em.AddFieldIfNotExist<DateTimeOffset>(FIELD_END_TIME);


            //Filters

            //Başlamış olanlar

            
            //


            return new UpdateResult();
        }

    }
}
