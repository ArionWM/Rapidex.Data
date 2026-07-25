using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
//using static Rapidex.Data.ConcreteEntityMapper;

namespace Rapidex.Data;

public class EntityMapper
{
    /// <summary>
    /// Bir alanın entity üzerine nasıl yazılacağı
    /// </summary>
    public enum FieldWriteMode
    {
        /// <summary>Entity'nin değer sözlüğüne yazılır (property karşılığı yok)</summary>
        Dictionary = 0,
        /// <summary>CLR property'sine yazılır</summary>
        Property = 1,
        /// <summary>Yazılmaz (property var ama yazılabilir değil)</summary>
        Skip = 2
    }

    /// <summary>
    /// Bir alanın yükleme planı. Entity metadata'sına bağlıdır, tabloya bağlı değildir;
    /// bu yüzden entity tipi başına bir kez üretilip cache'lenir.
    /// </summary>
    public sealed class FieldPlan
    {
        public IDbFieldMetadata FieldMetadata { get; set; }
        public string Name { get; set; }
        public Type TargetType { get; set; }
        public bool IsDataType { get; set; }
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Değer null geldiğinde yazılacak hazır değer. Value type alanlarda
        /// (Converter'ın null davranışıyla uyumlu olacak şekilde) default örnektir.
        /// </summary>
        public object NullValue { get; set; }

        public FieldWriteMode WriteMode { get; set; }
        public PropertyInfo Property { get; set; }
        public Type PropertyType { get; set; }

        /// <summary>
        /// Derlenmiş property setter. Public setter yoksa null olur, bu durumda
        /// reflection ile yazılır (mevcut davranış korunur).
        /// </summary>
        public Action<object, object> Setter { get; set; }

        /// <summary>
        /// Property tipi alan tipinden farklıysa true. Bu durumda değer yazılmadan önce
        /// ikinci bir dönüşüm gerekip gerekmediği kontrol edilir.
        /// </summary>
        public bool NeedsPropertyConversion { get; set; }
    }

    /// <summary>
    /// Bir alan planının somut bir tabloya bağlanmış hali
    /// </summary>
    public readonly struct BoundField
    {
        public readonly FieldPlan Plan;
        public readonly DataColumn Column;
        public readonly bool NeedsConversion;

        /// <param name="plan">Alanın tablodan bağımsız planı</param>
        /// <param name="column">Değerin okunacağı kolon</param>
        /// <param name="needsConversion">Kolon tipi alan tipine atanamıyorsa true</param>
        public BoundField(FieldPlan plan, DataColumn column, bool needsConversion)
        {
            this.Plan = plan;
            this.Column = column;
            this.NeedsConversion = needsConversion;
        }
    }

    /// <summary>
    /// Bir alan planının somut bir reader'a bağlanmış hali
    /// </summary>
    public readonly struct BoundReaderField
    {
        public readonly FieldPlan Plan;
        public readonly int Ordinal;
        public readonly bool NeedsConversion;

        /// <param name="plan">Alanın sorgudan bağımsız planı</param>
        /// <param name="ordinal">Değerin okunacağı kolon sırası</param>
        /// <param name="needsConversion">Kolon tipi alan tipine atanamıyorsa true</param>
        public BoundReaderField(FieldPlan plan, int ordinal, bool needsConversion)
        {
            this.Plan = plan;
            this.Ordinal = ordinal;
            this.NeedsConversion = needsConversion;
        }
    }

    /// <summary>
    /// Planın somut bir sorgu sonucuna bağlanmış halinin ortak bilgileri. Her sorgu yeni bir
    /// sonuç kümesi ürettiği için (kolon referansları paylaşılamaz) sorgu başına üretilir.
    /// </summary>
    public abstract class QueryBinding
    {
        /// <summary>
        /// Sonuçta karşılığı bulunamayan ilk alanın adı. Satır varsa hata üretilir
        /// (mevcut davranış: eksik kolona erişimde ArgumentException).
        /// </summary>
        public string MissingColumnFieldName { get; set; }

        /// <summary>
        /// Sonuçtaki kolon adları. Yalnızca eksik alan varsa doldurulur (hata mesajı için).
        /// </summary>
        public string[] Columns { get; set; }
    }

    /// <summary>
    /// Planın somut bir DataTable'a bağlanmış hali
    /// </summary>
    public sealed class TableBinding : QueryBinding
    {
        public BoundField[] Fields { get; set; }
    }

    /// <summary>
    /// Planın somut bir reader'a bağlanmış hali
    /// </summary>
    public sealed class ReaderBinding : QueryBinding
    {
        public BoundReaderField[] Fields { get; set; }
    }

    protected IDbSchemaScope Parent { get; }
    public class EntityTypeMap
    {
        protected ReaderWriterLockSlim SyncLock { get; } = new ReaderWriterLockSlim();

        public IDbEntityMetadata EntityMetadata { get; set; }
        public List<IDbFieldMetadata> AdvancedFields { get; } = new();

        public ConcurrentDictionary<string, string> TableFieldMappings { get; } = new ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Maintains the count of TableFieldMappings to avoid the locking overhead of ConcurrentDictionary.Count
        /// </summary>
        private int tableFieldMappingsCount;

        /// <summary>
        /// Entity'nin CLR tipi (concrete entity'ler için). Sözlük tabanlı entity'lerde null.
        /// </summary>
        public Type EntityType { get; protected set; }

        /// <summary>
        /// Veritabanından yüklenen alanların planı (IsPersisted ve !SkipDirectLoad)
        /// </summary>
        public FieldPlan[] LoadFields { get; protected set; } = Array.Empty<FieldPlan>();

        /// <summary>
        /// Yükleme planında yer almayan IDataType alanları. Bunlar mapping sonrası
        /// ayrıca ilklendirilir.
        /// </summary>
        public IDbFieldMetadata[] NonLoadAdvancedFields { get; protected set; } = Array.Empty<IDbFieldMetadata>();

        public void CreateMap()
        {
            this.SyncLock.EnterWriteLock();
            try
            {
                foreach (IDbFieldMetadata fm in this.EntityMetadata.Fields.Values)
                {
                    if (fm.IsAdvancedDataType)
                    {
                        this.AdvancedFields.Add(fm);
                    }
                }

                this.EntityType = this.EntityMetadata.ConcreteTypeName.IsNullOrEmpty()
                    ? null
                    : DbEntityFactory.ResolveConcreteType(this.EntityMetadata.ConcreteTypeName);

                this.CreateFieldPlans();
            }
            finally
            {
                this.SyncLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Alan planlarını üretir. Satır başına tekrarlanan tüm metadata kararları
        /// (property çözümü, setter derlemesi, IDataType tespiti, null karşılığı) burada bir kez verilir.
        /// </summary>
        protected void CreateFieldPlans()
        {
            IDbFieldMetadata primaryKey = this.EntityMetadata.PrimaryKey;
            List<FieldPlan> loadFields = new List<FieldPlan>(this.EntityMetadata.Fields.Count);
            List<IDbFieldMetadata> nonLoadAdvanced = new List<IDbFieldMetadata>();

            foreach (IDbFieldMetadata fm in this.EntityMetadata.Fields.Values)
            {
                if (!fm.IsPersisted || fm.SkipDirectLoad)
                {
                    if (fm.IsAdvancedDataType)
                        nonLoadAdvanced.Add(fm);
                    continue;
                }

                loadFields.Add(CreateFieldPlan(this.EntityType, fm, fm == primaryKey));
            }

            this.LoadFields = loadFields.ToArray();
            this.NonLoadAdvancedFields = nonLoadAdvanced.ToArray();
        }

        /// <summary>
        /// Tek bir alan için planı üretir
        /// </summary>
        /// <param name="entityType">Entity'nin CLR tipi (yoksa null)</param>
        /// <param name="fm">Alan metadata'sı</param>
        /// <param name="isPrimaryKey">Alan birincil anahtar mı</param>
        protected static FieldPlan CreateFieldPlan(Type entityType, IDbFieldMetadata fm, bool isPrimaryKey)
        {
            Type targetType = fm.Type;
            FieldPlan plan = new FieldPlan();
            plan.FieldMetadata = fm;
            plan.Name = fm.Name;
            plan.TargetType = targetType;
            plan.IsDataType = fm.IsAdvancedDataType;
            plan.IsPrimaryKey = isPrimaryKey;

            //Converter null değerleri value type'lar için default örneğe çevirir. Bu karar
            //tip başına sabit olduğu için hazır değer olarak tutulur.
            if (!plan.IsDataType && targetType != null && targetType.IsValueType)
                plan.NullValue = TypeHelper.CreateInstance(targetType);

            PropertyInfo property = entityType?.GetPropertyCached(fm.Name);
            if (property == null)
            {
                plan.WriteMode = FieldWriteMode.Dictionary;
            }
            else if (!property.CanWrite)
            {
                plan.WriteMode = FieldWriteMode.Skip;
            }
            else
            {
                plan.WriteMode = FieldWriteMode.Property;
                plan.Property = property;
                plan.PropertyType = property.PropertyType;
                plan.Setter = property.GetPropertySetterCached();
                plan.NeedsPropertyConversion = property.PropertyType != targetType;
            }

            return plan;
        }

        /// <summary>
        /// Planı verilen tabloya bağlar (kolon çözümü ve dönüşüm kararı)
        /// </summary>
        /// <param name="table">Bağlanacak tablo</param>
        public TableBinding Bind(DataTable table)
        {
            this.CheckUpdateMap(table);

            FieldPlan[] plans = this.LoadFields;
            BoundField[] bound = new BoundField[plans.Length];
            IDbNamingHelper namingHelper = null;
            string missing = null;
            int count = 0;

            for (int i = 0; i < plans.Length; i++)
            {
                FieldPlan plan = plans[i];

                this.TableFieldMappings.TryGetValue(plan.Name, out string columnName);
                columnName ??= plan.Name;

                DataColumn column = table.Columns[columnName];
                if (column == null)
                {
                    //Alan adı kolon adına dönüştürülüyor olabilir (Örn; "My.Field" -> "my_field")
                    namingHelper ??= this.EntityMetadata.Parent.DbScope.DbProvider.GetNamingHelper();
                    column = table.Columns[namingHelper.CheckColumnName(columnName)];
                }

                if (column == null)
                {
                    missing ??= plan.Name;
                    continue;
                }

                bool needsConversion = !plan.IsDataType && !plan.TargetType.IsAssignableFrom(column.DataType);
                bound[count++] = new BoundField(plan, column, needsConversion);
            }

            TableBinding binding = new TableBinding();
            binding.Fields = count == bound.Length ? bound : bound[..count];
            binding.MissingColumnFieldName = missing;

            if (missing != null)
            {
                string[] names = new string[table.Columns.Count];
                for (int i = 0; i < names.Length; i++)
                    names[i] = table.Columns[i].ColumnName;
                binding.Columns = names;
            }

            return binding;
        }

        /// <summary>
        /// Planı verilen reader'a bağlar (ordinal çözümü ve dönüşüm kararı). Ara bir DataTable üretilmez.
        /// </summary>
        /// <param name="reader">Bağlanacak reader</param>
        public ReaderBinding Bind(DbDataReader reader)
        {
            int fieldCount = reader.FieldCount;

            //DataTable yolunda kolon arama CaseSensitive=false ile yapılıyor; burada da
            //aynı davranışı vermek için harf duyarsız bir ad->sıra sözlüğü kullanılır.
            Dictionary<string, int> ordinals = new Dictionary<string, int>(fieldCount, StringComparer.InvariantCultureIgnoreCase);
            for (int i = 0; i < fieldCount; i++)
            {
                //Nested sorgularda aynı ad birden fazla tablodan gelebilir (Örn; iki tabloda da "Name").
                //DataTable yinelenenleri yeniden adlandırdığı için arama ilk kolona düşer; TryAdd ile
                //aynı davranış korunur.
                ordinals.TryAdd(reader.GetName(i), i);
            }

            this.CheckUpdateMap(ordinals.Keys, fieldCount);

            FieldPlan[] plans = this.LoadFields;
            BoundReaderField[] bound = new BoundReaderField[plans.Length];
            IDbNamingHelper namingHelper = null;
            string missing = null;
            int count = 0;

            for (int i = 0; i < plans.Length; i++)
            {
                FieldPlan plan = plans[i];

                this.TableFieldMappings.TryGetValue(plan.Name, out string columnName);
                columnName ??= plan.Name;

                if (!ordinals.TryGetValue(columnName, out int ordinal))
                {
                    //Alan adı kolon adına dönüştürülüyor olabilir (Örn; "My.Field" -> "my_field")
                    namingHelper ??= this.EntityMetadata.Parent.DbScope.DbProvider.GetNamingHelper();
                    if (!ordinals.TryGetValue(namingHelper.CheckColumnName(columnName), out ordinal))
                    {
                        missing ??= plan.Name;
                        continue;
                    }
                }

                bool needsConversion = !plan.IsDataType && !plan.TargetType.IsAssignableFrom(reader.GetFieldType(ordinal));
                bound[count++] = new BoundReaderField(plan, ordinal, needsConversion);
            }

            ReaderBinding binding = new ReaderBinding();
            binding.Fields = count == bound.Length ? bound : bound[..count];
            binding.MissingColumnFieldName = missing;

            if (missing != null)
                binding.Columns = ordinals.Keys.ToArray();

            return binding;
        }

        protected void UpdateMapInternal(DataTable table)
        {
            this.TableFieldMappings.Clear();
            int count = 0;
            foreach (DataColumn col in table.Columns)
            {
                var fm = this.EntityMetadata.Fields.Get(col.ColumnName);
                if (fm != null)
                {
                    this.TableFieldMappings.Set(fm.Name, col.ColumnName);
                    count++;
                }
            }
            Interlocked.Exchange(ref this.tableFieldMappingsCount, count);
        }

        public void UpdateMap(DataTable table)
        {
            this.SyncLock.EnterWriteLock();
            try
            {
                this.UpdateMapInternal(table);
            }
            finally
            {
                this.SyncLock.ExitWriteLock();
            }
        }

        public void CheckUpdateMap(DataTable table)
        {
            this.SyncLock.EnterUpgradeableReadLock();
            try
            {
                if (this.tableFieldMappingsCount != table.Columns.Count)
                {
                    this.SyncLock.EnterWriteLock();
                    try
                    {
                        this.UpdateMapInternal(table);
                    }
                    finally
                    {
                        this.SyncLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                this.SyncLock.ExitUpgradeableReadLock();
            }
        }

        protected void UpdateMapInternal(ICollection<string> columnNames)
        {
            this.TableFieldMappings.Clear();
            int count = 0;
            foreach (string columnName in columnNames)
            {
                var fm = this.EntityMetadata.Fields.Get(columnName);
                if (fm != null)
                {
                    this.TableFieldMappings.Set(fm.Name, columnName);
                    count++;
                }
            }
            Interlocked.Exchange(ref this.tableFieldMappingsCount, count);
        }

        /// <summary>
        /// Sonuç kümesinin kolonları değiştiyse alan-kolon eşlemesini yeniler
        /// </summary>
        /// <param name="columnNames">Sonuçtaki kolon adları</param>
        /// <param name="columnCount">Sonuçtaki kolon sayısı</param>
        public void CheckUpdateMap(ICollection<string> columnNames, int columnCount)
        {
            this.SyncLock.EnterUpgradeableReadLock();
            try
            {
                if (this.tableFieldMappingsCount != columnCount)
                {
                    this.SyncLock.EnterWriteLock();
                    try
                    {
                        this.UpdateMapInternal(columnNames);
                    }
                    finally
                    {
                        this.SyncLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                this.SyncLock.ExitUpgradeableReadLock();
            }
        }
    }

    protected DictionaryA<EntityTypeMap> Index { get; } = new DictionaryA<EntityTypeMap>();

    protected ReaderWriterLockSlim Lock { get; } = new ReaderWriterLockSlim();

    public EntityMapper()
    {
    }

    public EntityMapper(IDbSchemaScope parent)
    {
        this.Parent = parent;
    }

    public EntityTypeMap GetMapping(IDbEntityMetadata em)
    {
        this.Lock.EnterUpgradeableReadLock();
        try
        {
            EntityTypeMap etm = this.Index.Get(em.Name);
            if (etm != null)
            {
                return etm;
            }
            else
            {
                this.Lock.EnterWriteLock();
                try
                {
                    //Error: Should update after entity metadata change
                    EntityTypeMap tpl = new EntityTypeMap();
                    tpl.EntityMetadata = em;
                    tpl.CreateMap();
                    this.Index.Add(em.Name, tpl);
                    return tpl;
                }
                finally
                {
                    this.Lock.ExitWriteLock();
                }
            }
        }
        finally
        {
            this.Lock.ExitUpgradeableReadLock();
        }
    }

    public void EnsureAdvancedDataTypes(IEntity entity)
    {
        entity.ShouldNotSupportTo<IPartialEntity>("Partial entities can't be check. Use EnsureAdvancedDataTypesForPartial");

        var em = entity.GetMetadata();
        var map = this.GetMapping(em);

        foreach (IDbFieldMetadata fm in map.AdvancedFields)
        {
            EnsureAdvancedDataType(entity, fm);
        }
    }

    /// <summary>
    /// Verilen alanlar için IDataType örneklerini garanti eder. Yükleme yolunda
    /// mapper tarafından zaten doldurulmuş alanların tekrar ilklendirilmemesi için kullanılır.
    /// </summary>
    /// <param name="entity">İlgili entity</param>
    /// <param name="fields">İlklendirilecek alanlar</param>
    protected void EnsureAdvancedDataTypes(IEntity entity, IDbFieldMetadata[] fields)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            EnsureAdvancedDataType(entity, fields[i]);
        }
    }

    /// <summary>
    /// Tek bir IDataType alanının entity üzerinde hazır olmasını sağlar
    /// </summary>
    /// <param name="entity">İlgili entity</param>
    /// <param name="fm">Alan metadata'sı</param>
    protected static void EnsureAdvancedDataType(IEntity entity, IDbFieldMetadata fm)
    {
        object value = entity.GetValue(fm.Name);
        IDataType available = value as IDataType;

        //EnsureValueType?
        if (available == null)
        {
            var dt = TypeHelper.CreateInstance<IDataType>(fm.Type); //TODO: Field factory
            dt.SetupInstance(entity, fm);

            if (value != null)
            {
                dt.SetValue(entity, fm.Name, value, true);
            }

            entity.SetValue(fm.Name, dt);

        }
        else
        {
            available.SetupInstance(entity, fm);
        }
    }

    public void EnsureAdvancedDataTypesForPartial(IPartialEntity entity)
    {
        var em = entity.GetMetadata();
        var map = this.GetMapping(em);

        var availableFields = entity.GetFieldNames();

        foreach (IDbFieldMetadata fm in map.AdvancedFields)
        {
            bool avail = availableFields.Contains(fm.Name, StringComparer.InvariantCultureIgnoreCase);
            if (!avail)
                continue;

            object value = entity.GetValue(fm.Name);
            IDataType available = value as IDataType;

            //EnsureValueType?
            if (available == null)
            {
                var dt = TypeHelper.CreateInstance<IDataType>(fm.Type);
                dt.SetupInstance(entity, fm);

                if (value != null)
                {
                    dt.SetValue(entity, fm.Name, value, true);
                }

                entity.SetValue(fm.Name, dt);

            }
            else
            {
                available.SetupInstance(entity, fm);
            }
        }
    }

    public static IDataType CreateDataTypeField(IDbFieldMetadata fm, IEntity parent)
    {
        IDataType dt = TypeHelper.CreateInstance<IDataType>(fm.Type);
        dt.SetupInstance(parent, fm);
        return dt;
    }

    public static object EnsureValueType(IDbFieldMetadata fm, IEntity parent, object value, bool applyToEntity = true)
    {
        Type fieldType = fm.Type;

        if (value != null && fieldType.IsInstanceOfType(value))
            return value;

        if (value is JsonElement jelement)
        {
            object jConvValue1 = jelement.GetValueWithOriginalType(e => fieldType);
            bool directSupport = jConvValue1.IsSupportTo(fieldType) || jConvValue1.IsSupportTo(fm.BaseType);
            if (!directSupport)
            {
                object jConvValue2;

                if (Common.Converter.TryConvert(jConvValue1, fieldType, out jConvValue2) || Common.Converter.TryConvert(jConvValue1, fm.BaseType, out jConvValue2))
                    jConvValue1 = jConvValue2;
            }
            value = jConvValue1;
        }

        if (fm.IsAdvancedDataType)
        {
            IDataType dt = CreateDataTypeField(fm, parent);
            dt.SetValue(parent, fm.Name, value, applyToEntity);
            return dt;
        }
        else
        {
            return value.As(fieldType);
        }

    }

    public static object EnsureValueType(PropertyInfo pinfo, IEntity parent, object value, bool applyToEntity = true)
    {
        Type propertyType = pinfo.PropertyType;

        if (value != null && propertyType.IsInstanceOfType(value))
            return value;

        if (propertyType.IsSupportTo<IDataType>())
        {
            var em = parent.GetMetadata();
            var fm = em.Fields[pinfo.Name];

            if (fm.SkipDirectSet)
                return value;

            IDataType dt = CreateDataTypeField(fm, parent);
            dt.SetValue(parent, pinfo.Name, value, applyToEntity);
            return dt;
        }
        else
        {
            return value.As(propertyType);
        }

    }

    public IEntity Clone(IEntity from, IDbSchemaScope targetScope = null)
    {
        var em = from.GetMetadata();
        IEntity clone = Database.EntityFactory.Create(em, this.Parent, false);

        if (targetScope == null)
        {
            clone._DbName = from._DbName;
            clone._Schema = from._Schema;
            clone._SchemaName = from._SchemaName;
            clone.SetId((long)from.GetId());
            clone.DbVersion = from.DbVersion;
        }
        else
        {
            clone._DbName = targetScope.ParentDbScope.Name;
            clone._Schema = targetScope;
            clone._SchemaName = targetScope.SchemaName;
            clone._IsNew = true;
            clone.SetId(0L);
        }


        return this.Copy(from, clone);
    }


    public IEntity Copy(IEntity from, IEntity to)
    {
        var em = from.GetMetadata();

        to._TypeName = from._TypeName;
        to.ExternalId = from.ExternalId;
        to._loadSource = from._loadSource;

        foreach (var fm in em.Fields.Values)
        {
            if (fm == em.PrimaryKey)
                continue;

            if (fm.SkipDirectSet)
                continue;

            object val = fm.ValueGetterUpper(from, fm.Name);

            if (val is IDataType dt)
            {
                val = dt.CloneFor(to, fm);
            }

            to.SetValue(fm.Name, val);
        }

        this.EnsureAdvancedDataTypes(to);

        return to;
    }

    public IEntity Map(IDbSchemaScope schema, IDbEntityMetadata em, EntityTypeMap map, DataRow from, IEntity fillTo, Action<IEntity> set = null)
    {
        TableBinding binding = map.Bind(from.Table);
        this.CheckBinding(binding, em);
        return this.Map(em, map, binding, from, fillTo, set);
    }

    /// <summary>
    /// Ham kolon değerini alan planına göre entity'ye yazılabilir değere çevirir
    /// </summary>
    /// <param name="plan">Alan planı</param>
    /// <param name="needsConversion">Kaynak kolon tipi alan tipine atanamıyorsa true</param>
    /// <param name="value">Ham değer (DBNull olabilir)</param>
    /// <param name="fillTo">Doldurulan entity (IDataType örnekleri buna bağlanır)</param>
    static object PrepareValue(FieldPlan plan, bool needsConversion, object value, IEntity fillTo)
    {
        if (ReferenceEquals(value, DBNull.Value))
            value = null;

        if (plan.IsDataType)
        {
            if (value == null || !plan.TargetType.IsInstanceOfType(value))
            {
                //applyToEntity: false -> IDataType kendini entity'ye geri yazmaz, yazma işini
                //tek bir write üstlenir (bugünkü çifte yazma ortadan kalkar).
                IDataType dt = CreateDataTypeField(plan.FieldMetadata, fillTo);
                dt.SetValue(fillTo, plan.Name, value, false);
                value = dt;
            }
        }
        else if (value == null)
        {
            value = plan.NullValue;
        }
        else if (needsConversion)
        {
            value = value.As(plan.TargetType);
        }

        if (plan.NeedsPropertyConversion && !(value != null && plan.PropertyType.IsInstanceOfType(value)))
            value = EnsureValueType(plan.Property, fillTo, value);

        return value;
    }

    /// <summary>
    /// Hazırlanmış değeri planın belirlediği yere yazar
    /// </summary>
    /// <param name="plan">Alan planı</param>
    /// <param name="fillTo">Doldurulan entity</param>
    /// <param name="value">Yazılacak değer</param>
    /// <param name="values">Sözlük tabanlı alanlar için entity'nin değer sözlüğü (ilk ihtiyaçta doldurulur)</param>
    static void WriteValue(FieldPlan plan, IEntity fillTo, object value, ref ObjDictionary values)
    {
        switch (plan.WriteMode)
        {
            case FieldWriteMode.Property:
                if (plan.Setter != null)
                    plan.Setter(fillTo, value);
                else
                    plan.Property.SetValue(fillTo, value);
                break;
            case FieldWriteMode.Dictionary:
                //DictionaryHelper.Set yerine doğrudan indexer: entity bu noktada yalnızca
                //bu thread'e ait olduğu için kilit gerekmez.
                values ??= fillTo.GetAllValues();
                values[plan.Name] = value;
                break;
        }
    }

    /// <summary>
    /// Alanlar yazıldıktan sonraki ortak adımlar: şema, birincil anahtar ve yüklenmeyen IDataType alanları
    /// </summary>
    /// <param name="em">Entity metadata'sı</param>
    /// <param name="map">Entity tip eşlemesi</param>
    /// <param name="fillTo">Doldurulan entity</param>
    /// <param name="primaryKeyValue">Döngüde okunan birincil anahtar değeri</param>
    /// <param name="primaryKeyFound">Birincil anahtar sonuç kümesinde bulunduysa true</param>
    /// <param name="set">Entity hazır olduğunda çağrılacak işlem</param>
    void FinishMap(IDbEntityMetadata em, EntityTypeMap map, IEntity fillTo, object primaryKeyValue, bool primaryKeyFound, Action<IEntity> set)
    {
        fillTo._Schema = this.Parent;

        if (!primaryKeyFound)
            primaryKeyValue = em.PrimaryKey.ValueGetterLower(fillTo, em.PrimaryKey.Name);
        else if (primaryKeyValue is IDataType primaryKeyDataType)
            primaryKeyValue = primaryKeyDataType.GetValueLower();

        fillTo.SetId(primaryKeyValue);

        this.EnsureAdvancedDataTypes(fillTo, map.NonLoadAdvancedFields);

        set?.Invoke(fillTo);
    }

    /// <summary>
    /// Bir satırı hazır bir plan üzerinden entity'ye yazar. Alan başına metadata çözümü,
    /// kolon adı araması ve ikinci bir tip dönüşümü yapılmaz.
    /// </summary>
    /// <param name="em">Entity metadata'sı</param>
    /// <param name="map">Entity tip eşlemesi</param>
    /// <param name="binding">Tabloya bağlanmış plan</param>
    /// <param name="from">Kaynak satır</param>
    /// <param name="fillTo">Doldurulacak entity</param>
    /// <param name="set">Entity hazır olduğunda çağrılacak işlem</param>
    protected IEntity Map(IDbEntityMetadata em, EntityTypeMap map, TableBinding binding, DataRow from, IEntity fillTo, Action<IEntity> set = null)
    {
        BoundField[] fields = binding.Fields;
        ObjDictionary values = null;
        object primaryKeyValue = null;
        bool primaryKeyFound = false;

        for (int i = 0; i < fields.Length; i++)
        {
            FieldPlan plan = fields[i].Plan;
            object value = PrepareValue(plan, fields[i].NeedsConversion, from[fields[i].Column], fillTo);

            if (plan.IsPrimaryKey)
            {
                primaryKeyValue = value;
                primaryKeyFound = true;
            }

            WriteValue(plan, fillTo, value, ref values);
        }

        this.FinishMap(em, map, fillTo, primaryKeyValue, primaryKeyFound, set);

        return fillTo;
    }

    /// <summary>
    /// Reader'ın bulunduğu satırı hazır bir plan üzerinden entity'ye yazar
    /// </summary>
    /// <param name="em">Entity metadata'sı</param>
    /// <param name="map">Entity tip eşlemesi</param>
    /// <param name="binding">Reader'a bağlanmış plan</param>
    /// <param name="from">Kaynak reader (geçerli satırda)</param>
    /// <param name="fillTo">Doldurulacak entity</param>
    /// <param name="set">Entity hazır olduğunda çağrılacak işlem</param>
    protected IEntity Map(IDbEntityMetadata em, EntityTypeMap map, ReaderBinding binding, DbDataReader from, IEntity fillTo, Action<IEntity> set = null)
    {
        BoundReaderField[] fields = binding.Fields;
        ObjDictionary values = null;
        object primaryKeyValue = null;
        bool primaryKeyFound = false;

        for (int i = 0; i < fields.Length; i++)
        {
            FieldPlan plan = fields[i].Plan;
            object value = PrepareValue(plan, fields[i].NeedsConversion, from.GetValue(fields[i].Ordinal), fillTo);

            if (plan.IsPrimaryKey)
            {
                primaryKeyValue = value;
                primaryKeyFound = true;
            }

            WriteValue(plan, fillTo, value, ref values);
        }

        this.FinishMap(em, map, fillTo, primaryKeyValue, primaryKeyFound, set);

        return fillTo;
    }

    /// <summary>
    /// Sonuç kümesinde karşılığı olmayan alan varsa, mevcut davranışla uyumlu olacak şekilde hata üretir.
    /// </summary>
    /// <param name="binding">Kontrol edilecek binding</param>
    /// <param name="em">Entity metadata'sı</param>
    protected void CheckBinding(QueryBinding binding, IDbEntityMetadata em)
    {
        if (binding.MissingColumnFieldName != null)
            throw new ArgumentException($"Column '{binding.MissingColumnFieldName}' (of entity '{em.Name}') does not belong to table. Available columns: {binding.Columns.Join(", ")}");
    }

    public IEntity MapToNew(IDbEntityMetadata em, EntityTypeMap map, DataRow from, Action<IEntity> set = null)
    {
        TableBinding binding = map.Bind(from.Table);
        this.CheckBinding(binding, em);

        IEntity instance = Database.EntityFactory.Create(em, this.Parent, false, false);
        return this.Map(em, map, binding, from, instance, set);
    }


    public IEntity[] MapToNew(IDbEntityMetadata em, DataTable table, Action<IEntity> set = null)
    {
        var mapping = this.GetMapping(em);

        table.CaseSensitive = false;

        TableBinding binding = mapping.Bind(table);

        DataRowCollection rows = table.Rows;
        int rowCount = rows.Count;

        if (rowCount > 0)
            this.CheckBinding(binding, em);

        IEntity[] entities = new IEntity[rowCount];
        for (int i = 0; i < rowCount; i++)
        {
            IEntity instance = Database.EntityFactory.Create(em, this.Parent, false, false);
            entities[i] = this.Map(em, mapping, binding, rows[i], instance, set);
        }

        return entities;
    }

    /// <summary>
    /// Reader'daki tüm satırları yeni entity'lere yazar. Ara bir DataTable üretilmediği için
    /// satır başına DataRow/DataColumn tahsisi ve iki kez değer kopyalama ortadan kalkar.
    /// </summary>
    /// <param name="em">Entity metadata'sı</param>
    /// <param name="reader">Okunacak reader (henüz ilk satıra ilerlemiş olmamalı)</param>
    /// <param name="set">Her entity hazır olduğunda çağrılacak işlem</param>
    public IEntity[] MapToNew(IDbEntityMetadata em, DbDataReader reader, Action<IEntity> set = null)
    {
        EntityTypeMap mapping = this.GetMapping(em);
        ReaderBinding binding = mapping.Bind(reader);

        //Eksik kolon kontrolü yalnızca satır varsa yapılır (DataTable yolundaki davranış)
        if (!reader.Read())
            return Array.Empty<IEntity>();

        this.CheckBinding(binding, em);

        List<IEntity> entities = new List<IEntity>();
        do
        {
            IEntity instance = Database.EntityFactory.Create(em, this.Parent, false, false);
            entities.Add(this.Map(em, mapping, binding, reader, instance, set));
        }
        while (reader.Read());

        return entities.ToArray();
    }

    public IPartialEntity Map(IDbEntityMetadata em, EntityTypeMap map, DataRow from, IPartialEntity fillTo, string[] fields, Action<IEntity> set = null)
    {
        foreach (string fieldName in fields)
        {
            IDbFieldMetadata fm = em.Fields.Get(fieldName);
            if (!fm.IsPersisted)
                continue;

            if (fm.SkipDirectLoad)
                continue;

            if (fields.IsNOTNullOrEmpty() && !fields.Contains(fm.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            object value = null;

            map.TableFieldMappings.TryGetValue(fm.Name, out string columnName);
            columnName = columnName ?? fm.Name;

            value = from[columnName];
            if (Convert.IsDBNull(value))
                value = null;

            value = EnsureValueType(fm, fillTo, value);

            fillTo.SetValue(fm.Name, value);
        }

        fillTo.SetValue(DatabaseConstants.FIELD_ID, from[DatabaseConstants.FIELD_ID]);
        fillTo.SetValue(DatabaseConstants.FIELD_VERSION, from[DatabaseConstants.FIELD_VERSION]);

        fillTo._Schema = this.Parent;
        fillTo.SetId(em.PrimaryKey.ValueGetterLower(fillTo, em.PrimaryKey.Name));

        set?.Invoke(fillTo);

        return fillTo;
    }


    public IEntity Map(IDbEntityMetadata em, IDictionary<string, object> from, IEntity fillTo, Action<IEntity> set = null)
    {
        fillTo.ShouldNotSupportTo<IPartialEntity>("Partial entities can't be map");

        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (!fm.IsPersisted)
                continue;

            if (fm.SkipDirectLoad)
                continue;

            object value = from.Get(fm.Name);
            if (Convert.IsDBNull(value))
                value = null;

            //Json element ise !!

            value = EnsureValueType(fm, fillTo, value);

            fillTo.SetValue(fm.Name, value);
        }

        fillTo._Schema = this.Parent;
        fillTo.SetId(em.PrimaryKey.ValueGetterLower(fillTo, em.PrimaryKey.Name));

        set?.Invoke(fillTo);

        return fillTo;
    }

    public IEntity Map(IDbEntityMetadata em, IPartialEntity from, IEntity fillTo, Action<IEntity> set = null)
    {
        fillTo.ShouldNotSupportTo<IPartialEntity>("Partial entities can't be map");

        var values = from.GetAllValues();

        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (!fm.IsPersisted)
                continue;

            if (fm.SkipDirectLoad)
                continue;

            if (!values.ContainsKey(fm.Name))
                continue;

            object value = values.Get(fm.Name);
            if (Convert.IsDBNull(value))
                value = null;

            value = EnsureValueType(fm, fillTo, value);

            fillTo.SetValue(fm.Name, value);
        }

        fillTo._Schema = this.Parent;
        fillTo.SetId(em.PrimaryKey.ValueGetterLower(fillTo, em.PrimaryKey.Name));

        set?.Invoke(fillTo);

        return fillTo;
    }

    public IEntity MapToNew(IDbEntityMetadata em, IDictionary<string, object> from, Action<IEntity> set = null)
    {
        object obj = from.Get(DatabaseConstants.FIELD_ID);
        bool isNew = obj == null || Convert.IsDBNull(obj) || Convert.ToInt64(obj) <= 0;
        IEntity instance = Database.EntityFactory.Create(em, this.Parent, isNew);
        instance._IsNew = true;
        return this.Map(em, from, instance, set);
    }

    public IEntity Map(IDbSchemaScope schema, IDbEntityMetadata em, IDictionary<string, object> from, Action<IEntity> set = null)
    {
        IEntity instance = Database.EntityFactory.Create(em, schema, false);
        return this.Map(em, from, instance, set);
    }

    public IEntity Map(IDbEntityMetadata em, IDictionary<string, object> from, Action<IEntity> set = null)
    {
        return this.Map(this.Parent, em, from, set);
    }

    public static ObjDictionary MapToDict(IDbEntityMetadata em, IEntity entity, bool withSimpleValues = false)
    {
        ObjDictionary dict = new ObjDictionary();

        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (!fm.IsPersisted)
                continue;

            object value = entity.GetValue(fm.Name);
            if (value is IDataType dt)
            {
                if (withSimpleValues)
                    value = dt.GetValueLower();
                else
                    value = dt.Clone();
            }

            dict.Add(fm.Name, value);
        }
        return dict;
    }

    public static IDictionary<string, object> MapToDict(IEntity entity, bool withSimpleValues = false)
    {
        var em = entity.GetMetadata() ?? entity._Schema.ParentDbScope.Metadata.Get(entity.GetType().Name);
        em.NotNull();
        return EntityMapper.MapToDict(em, entity, withSimpleValues);
    }

    public static IEnumerable<IDictionary<string, object>> MapToDict(IEnumerable<IEntity> entities, bool withSimpleValues = false)
    {
        if (entities.IsNullOrEmpty())
        {
            return new IDictionary<string, object>[0];
        }

        var em = entities.First().GetMetadata();
        return entities.Select(e => EntityMapper.MapToDict(em, e, withSimpleValues));
    }

    public T Map<T>(IEntity source) where T : IConcreteEntity
    {
        Type conctType = typeof(T);

        T concEntity = (T)Database.EntityFactory.Create<T>(source._Schema, false);

        concEntity._IsNew = source._IsNew;
        concEntity.Id = (long)source.GetId();
        concEntity.ExternalId = source.ExternalId;
        concEntity.DbVersion = source.DbVersion;

        this.Copy(source, concEntity);

        concEntity.EnsureDataTypeInitialization();

        return concEntity;
    }

}
