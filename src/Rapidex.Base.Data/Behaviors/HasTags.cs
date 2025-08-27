
using Rapidex.Data.Entities;
using Rapidex.Data.Scopes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data
{
    public class HasTags : EntityBehaviorBase<HasTags>
    {
        public const string FIELD_TAGS = "Tags";
        public const string DEFAULT_COLOR = "white";
        public override string Descripton => "Ensures that the Entity has Tags field and UI supports tags component";

        public class TagInfo
        {
            public string Name { get; set; }
            public string Color { get; set; }

            public TagInfo()
            {
            }

            public TagInfo(string name, string color)
            {
                Name = name;
                Color = color;
            }


        }



        public HasTags()
        {
        }

        public HasTags(IEntity entity) : base(entity)
        {
        }

        public override IUpdateResult SetupMetadata(IDbEntityMetadata em)
        {
            em.AddFieldIfNotExist<Tags>(FIELD_TAGS);

            return new UpdateResult();
        }


        #region instance



        public string[] Tags
        {
            get
            {
                return Entity.GetValue<Tags>(FIELD_TAGS).Get();
            }
        }


        public void Add(params string[] tags)
        {
            Entity.GetValue<Tags>(FIELD_TAGS).Add(tags);
        }

        #endregion

        #region Base

        //TODO: taşınacak, common cache?
        static TwoLevelDictionary<string, string, string> _cache = new TwoLevelDictionary<string, string, string>();

        protected static void CheckCache(IDbSchemaScope dbScope, string entityName)
        {
            if (!_cache.ContainsKey(entityName))
            {
                var availableTags = dbScope.GetQuery<TagRecord>()
                    .Eq(nameof(TagRecord.Entity), entityName)
                    .Load();

                foreach (var tag in availableTags)
                {
                    _cache.Set(entityName, tag.Name, tag.Color);
                }
            }
        }

        protected static void Save(TagRecord rec)
        {
            rec.Save();
            _cache.Set(rec.Entity, rec.Name, rec.Color);
        }

        public static TagInfo SplitTag(string tag)
        {
            tag = tag.Trim();
            if (tag.Contains('/'))
            {
                var parts = tag.Split('/');
                return new TagInfo(parts[0], parts[1]);
            }
            else
                return new TagInfo(tag, null);
        }

        protected static void InternalCheckEntityTags(IDbSchemaScope dbScope, IDbEntityMetadata em, string[] tags)
        {
            dbScope.NotNull();

            tags = tags.DistinctWithTrimElements();

            HashSet<string> tagNames = new HashSet<string>();
            foreach (var tag in tags)
            {
                var _tag = SplitTag(tag);
                tagNames.Add(_tag.Name);
            }

            //Tags base schema da yer alır
            //TagRecord'lar üzerinden kontrol edilecek. Ancak Jobman vs. henüz yok?


            using var _ = new LockScope("TagsUpdate_" + em.Name);

            var query = dbScope.GetQuery<TagRecord>()
                  .Eq(nameof(TagRecord.Entity), em.Name);

            if (tagNames.IsNOTNullOrEmpty() && tagNames.Count > 0)
                query = query.In(nameof(TagRecord.Name), tagNames);

            var availableTags = query.Load();

            //TODO: var trn = dbScope.Begin("tags");
            try
            {
                foreach (var tag in tags)
                {
                    var _tag = SplitTag(tag);
                    TagRecord rec = availableTags.FirstOrDefault(x => string.Compare(x.Name, _tag.Name, true) == 0);
                    if (rec != null)
                    {
                        if (rec.Color != _tag.Color && _tag.Color.IsNOTNullOrEmpty())
                        {
                            rec.Color = _tag.Color;
                            Save(rec);
                        }
                    }
                    else
                    {
                        TagRecord tagrec = dbScope.New<TagRecord>();
                        tagrec.Entity = em.Name;
                        tagrec.Name = _tag.Name;
                        tagrec.Color = _tag.Color;
                        Save(tagrec);
                    }
                }

                dbScope.ApplyChanges();
                //trn.Commit();
            }
            catch (Exception ex)
            {
                //trn.Rollback();
                ex.Log();
                throw;
            }
            finally
            {

            }
        }

        public static void CheckEntityTagsAsync(IDbSchemaScope dbScope, IDbEntityMetadata em, params string[] tags)
        {

            Task.Run(() =>
            {
                try
                {
                    InternalCheckEntityTags(dbScope, em, tags);
                }
                catch (Exception ex)
                {
                    ex.Log();
                    throw;
                }
            });

        }

        public static void CheckEntityTags(IDbSchemaScope dbScope, IDbEntityMetadata em, string tags)
        {
            tags = tags?.Trim();
            if (tags.IsNullOrEmpty())
                return;
            CheckEntityTagsAsync(dbScope, em, tags.Split('|'));
        }

        public static IEntityLoadResult<TagRecord> GetEntityTags(IDbSchemaScope dbScope, IDbEntityMetadata em)
        {
            //TODO: From cache
            return dbScope.GetQuery<TagRecord>()
                .Eq(nameof(TagRecord.Entity), em.Name)
                .Load();
        }

        public static TagInfo[] GetTagInfo(IDbSchemaScope dbScope, IDbEntityMetadata em, params string[] tags)
        {
            CheckCache(dbScope, em.Name);

            List<TagInfo> result = new List<TagInfo>();

            foreach (var tag in tags)
            {
                var _tag = SplitTag(tag);
                string color = _cache.Get(em.Name, _tag.Name);
                result.Add(new TagInfo(_tag.Name, color ?? DEFAULT_COLOR));
            }

            return result.ToArray();
        }

        #endregion

    }
}
