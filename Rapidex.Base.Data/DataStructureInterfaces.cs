using Rapidex.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rapidex.Data;


//TODO: ObjDictionary to IDictionary<string, object>

public interface IDbDefinition //TODO: To be remove
{

}


public interface ILazy
{

    bool? SkipDirectLoad { get; }

    object GetContent();

}

public interface ILazy<T> : ILazy where T : IEntity
{
    new T GetContent();
}

public interface IReference : ILazy, IEmptyCheckObject
{
    long TargetId { get; set; }
    string ReferencedEntity { get; }
}

public interface IEnumeration //TODO: Remove? 
{

}

public interface IRelation //TODO: Remove? 
{

}






public interface IEntityChangeResultItem
{
    [JsonPropertyOrder(-9000)]
    string Name { get; set; }

    long Id { get; set; }

    long OldId { get; set; }

    string ExternalId { get; set; }

}

public interface IEntityUpdateResult : IUpdateResult<IEntityChangeResultItem>
{

}


public interface IIntSequence
{
    string DbName { get; set; }
    string SchemaName { get; set; }
    string Name { get; set; }
    long CurrentValue { get; }


    long Relocate(long startAt);

    /// <summary>
    /// Sequence'lar transaction'dan bağımsız çalışırlar. (mı?)
    /// </summary>
    /// <returns></returns>
    long GetNext();

    long[] GetNextN(int count);

    //void ScanAndLocateLast();
}

public delegate object EntityFieldValueGetterDelegate(IEntity source, string fieldName);
public delegate object ValueGetterDelegate();
public delegate void ValueSetterDelegate(IEntity source, string fieldName, object value, bool applyToEntity);

public interface IDbFieldMetadata : IImplementTarget, IComponent
{
    //[YamlMember(Order = -9999)]
    //[JsonPropertyOrder(-9999)]
    //string Name { get; set; }



    [YamlIgnore]
    [JsonIgnore]
    IDbEntityMetadata ParentMetadata { get; set; }

    [YamlIgnore]
    [JsonIgnore]
    Type Type { get; set; }

    [JsonPropertyName("type")]
    [YamlMember(Alias = "type", Order = -9998)]
    [JsonPropertyOrder(-9998)]
    string TypeName { get; set; }

    [YamlMember(Order = -9997)]
    [JsonPropertyOrder(-9997)]
    string Caption { get; set; }

    [YamlIgnore]
    [JsonIgnore]
    Type BaseType { get; set; }

    [YamlIgnore]
    [JsonIgnore]
    //[YamlMember(Order = -9996)]
    //[JsonPropertyOrder(-9996)]
    DbFieldType? DbType { get; set; }


    ///// <summary>
    ///// True ise metadata tanımlarında bu alan gizlenir
    ///// </summary>
    //bool IsHideForSerialization { get; set; }

    bool Invisible { get; set; }

    bool IsSealed { get; set; }

    /// <summary>
    /// False ise bu alan veritabanı işlemlerine dahil edilmez (sanaldır)
    /// </summary>
    [YamlIgnore]
    [JsonIgnore]
    bool IsPersisted { get; set; }

    /// <summary>
    /// True ise veritabanından yükler iken bu alan sorgulanmaz
    /// </summary>
    [YamlIgnore]
    [JsonIgnore]
    bool SkipDirectLoad { get; set; }

    /// <summary>
    /// True ise bu alan güncellenmeye çalışılmaz
    /// </summary>
    [YamlIgnore]
    [JsonIgnore]
    bool SkipDirectSet { get; set; }

    /// <summary>
    /// True ise bu alanın değişikliği veritabanı kaydında sürüm arttırmaya neden olmaz
    /// </summary>
    [YamlIgnore]
    [JsonIgnore]
    bool SkipDbVersioning { get; set; } //Karmaşıklaştırıyor mu?

    //[JsonPropertyOrder(100)]
    [YamlIgnore]
    [JsonIgnore]
    DbFieldProperties DbProperties { get; }


    /// <summary>
    /// Alt seviyeden (veritabanına yazılacak) değerini döndürür. Örn; bir reference için id değeri
    /// </summary>
    [YamlIgnore]
    [JsonIgnore]
    EntityFieldValueGetterDelegate ValueGetterLower { get; }

    /// <summary>
    /// Üst seviyeden değerini döndürür. Örn; bir reference için entity'nin kendisi
    /// Basit bir değerse ValueGetterLower ile aynı olabilir
    /// </summary>
    [YamlIgnore]
    [JsonIgnore]
    EntityFieldValueGetterDelegate ValueGetterUpper { get; }

    [YamlIgnore]
    [JsonIgnore]
    ValueSetterDelegate ValueSetter { get; }

    //DictionaryA<object> ExtraData { get; }

    /// <summary>
    /// Metadata ve layout servislerinde sunmak için özel anahtar/veri çiftlerini elde eder
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="placeOptions"></param>
    /// <returns></returns>
    void GetDefinitionData(IDbSchemaScope scope, ref ItemDefinitionExtraData data, bool placeOptions);
    void Setup(IDbEntityMetadata parentMetadata);
}

public interface ICalculatedColumnMetadata : IDbFieldMetadata
{
    public string Expression { get; set; }

    //Expression type? C# code, SQL, etc.
}



public interface IDbEntityMetadataValidator
{
    /// <summary>
    /// Validate entity metadata
    /// </summary>
    /// <param name="em"></param>
    /// <returns>True if valid</returns>
    IValidationResult Validate(IDbEntityMetadata em);

}

public interface IDbEntityMetadata : IImplementTarget, IComponent //İki katmanlı olsa? İlk katman kolayca serileşebilecek bir DTO, ikinci katmanda metotlar + filtreler?
{

    IDbMetadataContainer Parent { get; set; }


    /// <summary>
    /// For multi module applications, this is the module name
    /// </summary>
    string ModuleName { get; set; }
    string Prefix { get; set; }
    string TableName { get; internal set; }

    [YamlMember(Order = -9990)]
    [JsonPropertyOrder(-9990)]
    string ConcreteTypeName { get; internal set; }


    bool IsPremature { get; internal set; }

    /// <summary>
    /// If is 'true' this entity don't create other schemas
    /// </summary>
    bool OnlyBaseSchema { get; internal set; }

    List<string> Tags { get; }

    [JsonIgnore]
    [YamlIgnore]
    IDbFieldMetadata PrimaryKey { get; internal set; }

    [JsonIgnore]
    [YamlIgnore]
    IDbFieldMetadata Caption { get; internal set; }

    [JsonIgnore]
    [YamlIgnore]
    DbFieldMetadataList Fields { get; }


    [JsonPropertyOrder(9999)]
    [YamlMember(Alias = "Fields", Order = 9999)]
    List<IDbFieldMetadata> FieldsList { get; set; }

    [JsonIgnore]
    [YamlIgnore]
    ComponentDictionary<IEntityBehaviorDefinition> BehaviorDefinitions { get; }

    List<string> Behaviors { get; }

    ComponentDictionary<IPredefinedFilter> Filters { get; }

    //void AddFieldIfNotExist(IDbFieldMetadata column);
    void AddField(IDbFieldMetadata column);


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sealed">True: Definition can't change by administrators</param>
    IDbEntityMetadata AddBehavior<T>(bool @sealed, bool directApply) where T : IEntityBehaviorDefinition;

    IDbEntityMetadata AddBehavior(string name, bool @sealed, bool directApply);

    IDbEntityMetadata AddFilter<T>(T filter) where T : IPredefinedFilter;

    [Obsolete("Use AddFilter<T>() instead", true)]
    IDbEntityMetadata AddFilter(string name);


    IDbEntityMetadata RemoveBehavior<T>() where T : IEntityBehaviorDefinition;

    bool Is<T>() where T : IEntityBehaviorDefinition;

    IUpdateResult ApplyBehaviors();

    /// <summary>
    /// Behavior vb. içeriğin Scope'a uygulanması için çağrılır.
    /// </summary>
    /// <param name="scope"></param>
    void ApplyToScope(IDbSchemaScope scope);

}


//Senaryoya göre nasıl farklılıklar yapılacak? Required, Editing props? vs


public interface IFieldMetadataFactory 
{
    void SetParent(IDbMetadataContainer parent);
    IDbFieldMetadata Create(IDbEntityMetadata em, string fieldType, string name, ObjDictionary values);
    IDbFieldMetadata Create(IDbEntityMetadata em, Type type, string name);
    IDbFieldMetadata Create(IDbEntityMetadata em, Type type, string name, ObjDictionary values);
}

public interface IDbEntityMetadataFactory : IManager
{
    IDbEntityMetadata Create(string entityName, string module = null, string prefix = null);
}

[Obsolete("Use DbScope.Metadata instead", true)]
public interface IDbEntityMetadataManager : IManager
{
    IFieldMetadataFactory FieldMetadataFactory { get; }
    IDbEntityMetadataFactory EntityMetadataFactory { get; }

    void SetEntityMetadataFactory(IDbEntityMetadataFactory factory);

    IDbEntityMetadata AddPremature(string entityName);


    IDbEntityMetadata Add(Type concreteType, string module = null, string prefix = null);
    IDbEntityMetadata Add<TDeclaration>(string module = null, string prefix = null) where TDeclaration : IDbDefinition;

    ///// <summary>
    ///// Add all concrete IEntity classes in entity
    ///// </summary>
    ///// <param name="assembly"></param>
    //void Add(Assembly assembly);

    void Add(IDbEntityMetadata em);

    /// <summary>
    /// Metadata'nın son kontrollerini yapar
    /// </summary>
    /// <param name="em"></param>
    void Check(IDbEntityMetadata em);

    //IDbEntityMetadata AddFromXml(string xmlDefinitionFileContent, string module = null);
    IDbEntityMetadata AddFromJson(string jsonDefinitionFileContent, string module = null);
    IDbEntityMetadata AddFromEnum<TEnum>(string module = null, string prefix = null, Action<Enum, ObjDictionary> callb = null) where TEnum : System.Enum;

    IDbEntityMetadata Get(string entityName);

    IDbEntityMetadata[] GetAll();

    internal void Remove(IDbEntityMetadata em); //For tests

    void LoadFromDb(); //Zaten Setup() ile çağırılabilir?

    void Reload();

    //For tests
    internal void Clear();
}




public interface IDbMetadataContainer
{
    IDbScope DbScope { get; }
    ComponentDictionary<IDbEntityMetadata> Entities { get; }

    /// <summary>
    /// Predefined data for entity / module activation
    /// </summary>
    PredefinedDataCollection Data { get; }

    PredefinedDataCollection DemoData { get; }

    void Add(IDbEntityMetadata em);

    IDbEntityMetadata Get(string entityName);

    IDbEntityMetadata[] GetAll();

    void Remove(string entityName);
}




public interface IDbProvider : IManager
{
    string ConnectionString { get; }
    string DatabaseName { get; }
    IDbSchemaScope ParentScope { get; }

    IExceptionTranslator ExceptionTranslator { get; }

    void UseDb(string dbName);

    void SetParentScope(IDbSchemaScope parent);

    IValidationResult ValidateConnection();

    IDbStructureProvider GetStructureProvider();

    IDbDataModificationPovider GetDataModificationPovider();

    IDataUnitTestHelper GetTestHelper();

    void Start();
}




/// <summary>
/// Storage for changed (with Entity.Save) entities
/// This object must have thread isolated storages
/// storage will change while access with different threats
/// </summary>
public interface IDbChangesScope
{
    IReadOnlyCollection<IEntity> ChangedEntities { get; }

    IReadOnlyCollection<IEntity> DeletedEntities { get; }

    IReadOnlyCollection<IQueryUpdater> BulkUpdates { get; }

    void CheckNewEntities();

    IDbChangesScope[] SplitForTypesAndDependencies();

    void Add(IEntity entity);

    void Add(IEnumerable<IEntity> entities);

    void Add(IQueryUpdater updater);

    void Delete(IEntity entity);

    void Clear();
}

/// <summary>
/// Created db transaction
/// </summary>
public interface IDbTransactionScope : IDbChangesScope, IDisposable
{
    void Commit();

    void Rollback();

    void IDisposable.Dispose()
    {
        try
        {
            this.Commit();
        }
        catch (Exception ex)
        {
            var tex = Common.ExceptionManager.Translate(ex);
            tex.Log();

            this.Rollback();
            throw tex;
        }
    }
}



public struct DbEntityId //TODO: Rename?
{
    public long Id { get; set; }
    public int Version { get; set; }

    public DbEntityId()
    {

    }

    public DbEntityId(long id, int version)
    {
        Id = id;
        Version = version;
    }
}


public class DbEntityIdEqualityComparerById : IEqualityComparer<DbEntityId>
{
    public bool Equals(DbEntityId x, DbEntityId y)
    {
        return string.Equals(x.Id, y.Id);
    }

    public int GetHashCode(DbEntityId obj)
    {
        return obj.Id.GetHashCode();
    }
}

public interface ISerializationDataProvider
{
    object GetSerializationData(EntitySerializationOptions options);
    object SetWithSerializationData(string memberName, object value);
}

public interface IEntitySerializationDataCreator
{
    object ConvertToFieldData(IEntity entity, IDbFieldMetadata fm);
    T ConvertToEntityData<T>(IEntity entity, EntitySerializationOptions options, params string[] fields) where T : EntityDataDtoBase, new();
    EntityDataDtoBase ConvertToEntityData(IEntity entity, EntitySerializationOptions options, params string[] fields);

    ListDataDtoCollection<T> ConvertToListData<T>(IEnumerable<IEntity> entities, EntitySerializationOptions options, Dictionary<string, object> properties, params string[] fields) where T : EntityDataDtoBase, new();
    ListDataDtoCollection<EntityDataDtoBase> ConvertToListData(IEnumerable<IEntity> entities, EntitySerializationOptions options, Dictionary<string, object> properties, params string[] fields);
}

public interface IDbEntityLoader
{
    void Setup(params IDbEntityLoader[] loaders);

    IEntityLoadResult Load(IDbEntityMetadata em, IEnumerable<DbEntityId> ids);

    IEntityLoadResult Load(IQueryLoader loader);

    ILoadResult<DataRow> LoadRaw(IQueryLoader loader);
}

public interface IDbEntityUpdater
{
    IEntityUpdateResult InsertOrUpdate(IDbEntityMetadata em, IEnumerable<IEntity> entities);
    IEntityUpdateResult Delete(IDbEntityMetadata em, IEnumerable<long> ids);
    IEntityUpdateResult BulkUpdate(IDbEntityMetadata em, IQueryUpdater query);

}



public interface IDataUnitTestHelper
{
    void DropAllTablesInDatabase(string connectionString);


}

public interface IDbDataModificationPovider : IDbEntityUpdater, IDbEntityLoader
{
    IDbProvider ParentProvider { get; }


    IIntSequence Sequence(string name);

    IDbTransactionScope BeginTransaction(string transactionName = null);

    void IDbEntityLoader.Setup(params IDbEntityLoader[] loaders)
    {
        //Do nothing
    }
}



public interface IDbDataModificationManager
{
    IDbSchemaScope ParentScope { get; }

    //TODO: Async yaptık? !! Scope gerekiyor !!
    IDbTransactionScope CurrentTransaction { get; }
    bool IsTransactionAvailable { get { return this.CurrentTransaction != null; } }

    IEntity New(IDbEntityMetadata em);


    IQuery GetQuery(IDbEntityMetadata em);

    IQuery<T> GetQuery<T>() where T : IConcreteEntity;


    //[Obsolete("", true)]
    //IEntityLoadResult<IEntity> Load(IDbEntityMetadata em, IDbCriteria criteria = null);


    Task<IEntity> Find(IDbEntityMetadata em, long id);


    Task<IEntityLoadResult> Load(IQueryLoader loader);


    Task<ILoadResult<DataRow>> LoadRaw(IQueryLoader loader);

    void Save(IEntity entity);

    void Save(IEnumerable<IEntity> entities);

    void Add(IQueryUpdater updater);

    IDbTransactionScope Begin(string transactionName = null);

    void Delete(IEntity entity);

    /// <summary>
    /// Write changes to database
    /// If transaction is available, changes will be written before commit transaction
    /// </summary>
    Task<IEntityUpdateResult> CommitOrApplyChanges();
    Task Rollback();


    IIntSequence Sequence(string name);
}



public interface IDbStructureProvider
{
    string ConnectionString { get; }
    IDbProvider ParentDbProvider { get; }
    IDbSchemaScope ParentScope { get; }

    string CheckObjectName(string name);

    bool IsDatabaseAvailable(string dbName);
    bool IsSchemaAvailable(string schemaName);

    bool IsExists(string schemaName, string entityName); //IDbSchemaStructureModificationManager yapılsa mı?
    bool IsExists(string schemaName, string entityName, IDbFieldMetadata cm);

    void CreateDatabase(string dbName);
    void DestroyDatabase(string dbName);

    void SwitchDatabase(string dbName);

    void CreateOrUpdateSchema(string schemaName);
    void DestroySchema(string schemaName);

    void ApplyEntityStructure(IDbEntityMetadata entityMetadata, bool applyScopedData = false);

    /// <summary>
    /// Drop table or collection on database
    /// </summary>
    /// <param name="entityName"></param>
    void DropEntity(IDbEntityMetadata entityMetadata);
    void ApplyAllStructure();


    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="minValue">-1 ise mevcut sequence'in min value'su değiştirilmez. Yeniden oluşturulacak ise min val: 1 olarak ayarlanır</param>
    /// <param name="startValue">-1 verilir ise mevcut sequence'nin currentValue'su değiştirilmez. Yeniden oluşturulacak ise currentVal: 1 olarak ayarlanır</param>
    void CreateSequenceIfNotExists(string name, int minValue = -1, int startValue = -1);

    //Prosedür, fonksiyon vs. ne olacak? Native?
}



public interface IDbSchemaScope : IDbDataModificationManager
{
    IDbScope ParentDbScope { get; }
    IDbProvider DbProvider { get; }

    //Cache?

    string SchemaName { get; }


    IDbStructureProvider Structure { get; }
    IBlobRepository Blobs { get; }
    IDbDataModificationManager Data { get; }

    EntityMapper Mapper { get; }

    IDbTransactionScope IDbDataModificationManager.Begin(string transactionName = null)
    {
        return this.Data.Begin(transactionName);
    }

    async Task<IEntityUpdateResult> IDbDataModificationManager.CommitOrApplyChanges()
    {
        return await this.Data.CommitOrApplyChanges();
    }

    async Task IDbDataModificationManager.Rollback()
    {
        await this.Data.Rollback();
    }

    IDbSchemaScope IDbDataModificationManager.ParentScope => this;

    IDbTransactionScope IDbDataModificationManager.CurrentTransaction => this.Data.CurrentTransaction;


    IEntity IDbDataModificationManager.New(IDbEntityMetadata em)
    {
        return this.Data.New(em);
    }

    IQuery IDbDataModificationManager.GetQuery(IDbEntityMetadata em)
    {
        return this.Data.GetQuery(em);
    }

    IQuery<T> IDbDataModificationManager.GetQuery<T>()
    {
        return this.Data.GetQuery<T>();
    }


    async Task<IEntityLoadResult> IDbDataModificationManager.Load(IQueryLoader loader)
    {
        return await this.Data.Load(loader);
    }

    Task<ILoadResult<DataRow>> IDbDataModificationManager.LoadRaw(IQueryLoader loader)
    {
        return this.Data.LoadRaw(loader);
    }


    async Task<IEntity> IDbDataModificationManager.Find(IDbEntityMetadata em, long id)
    {
        return await this.Data.Find(em, id);
    }

    void IDbDataModificationManager.Save(IEntity entity)
    {
        this.Data.Save(entity);
    }

    void IDbDataModificationManager.Save(IEnumerable<IEntity> entities)
    {
        this.Data.Save(entities);
    }

    void IDbDataModificationManager.Add(IQueryUpdater updater)
    {
        this.Data.Add(updater);
    }

    void IDbDataModificationManager.Delete(IEntity entity)
    {
        this.Data.Delete(entity);
    }


    void Setup();
}


public interface IDbScope : IDbSchemaScope
{
    //Hangi database + connections vs?

    string ConnectionString { get; }
    /// <summary>
    /// Database + TenantName
    /// </summary>

    /// <summary>
    /// scopeID (TenantId)
    /// </summary>
    long Id { get; }
    string Name { get; }
    string DatabaseName { get; }
    string DefaultSchemaName { get; }

    string[] Schemas { get; }



    IDbSchemaScope Base { get; }

    IDbMetadataContainer Metadata { get; }

    IDbSchemaScope AddSchemaIfNotExists(string schemaName = null, long id = DatabaseConstants.DEFAULT_EMPTY_ID);

    IDbSchemaScope Schema(string schemaName = null);

}


public interface IDbScopeManager
{
    void EnableMultiDb();

    IDbScope AddMainDbIfNotExists();

    IDbScope AddDbIfNotExists(long id, string name); //, string connectionStringOrConnectionNameInConfig = null

    IDbScope Db(string dbName = null, bool throwErrorIfNotExists = true);
}

public interface IDbThreadScopeManager : IDbSchemaScope
{
    IDbSchemaScope CurrentScope { get; }

    //!!! ThreadLocal
    //[ThreadStatic]


    void Enter(IDbSchemaScope scope);

    void Exit();
}


public interface IEntity
{
    [JsonIgnore]
    internal IDbEntityMetadata _Metadata { get; set; }

    string _TypeName { get; internal set; }
    string _DbName { get; internal set; }
    string _SchemaName { get; internal set; }
    bool _IsNew { get; set; }

    [JsonIgnore]
    IDbSchemaScope _Scope { get; internal set; }


    object this[string columnName] { get; set; }

    //IEntityRelationCollection Details { get; }

    int DbVersion { get; set; }
    string ExternalId { get; set; }



    object GetId();
    void SetId(object id);

    T GetValue<T>(string fieldName); //TODO: Extension
    void SetValue<T>(string fieldName, T value); //TODO: Extension

    object GetValue(string fieldName);
    void SetValue(string fieldName, object value);

    ObjDictionary GetAllValues();
}

/// <summary>
/// Kısmi veri taşıyan entity'ler için kullanılır
/// </summary>
public interface IPartialEntity : IEntity
{
    bool IsDeleted { get; set; }
}

public interface IEntity<IdType> : IEntity
{
    IdType Id { get; set; }



}


public interface IIntEntity : IEntity<long>
{


}

public interface IConcreteEntity : IIntEntity, IDbDefinition, IJsonOnDeserialized
{
}




public class PagingInfo : IPaging
{
    public long? PageSize { get; set; }
    public long? StartIndex { get; set; }
    public long? PageIndex { get; set; }
    public long? PageCount { get; set; }
    public bool IncludeTotalItemCount { get; set; }

    public PagingInfo()
    {

    }

    public PagingInfo(long pageSize, long pageIndex)
    {
        PageSize = pageSize;
        PageIndex = pageIndex;
        StartIndex = pageSize * pageIndex;
    }
}


public interface IEntityReadonlyCollection<TEntity> : IReadOnlyList<TEntity>, IEmptyCheckObject where TEntity : IEntity
{

}

public interface IEntityLoadResult<TEntity> : ILoadResult<TEntity>, IEntityReadonlyCollection<TEntity> where TEntity : IEntity
{

}

public interface IEntityLoadResult : IEntityLoadResult<IEntity>
{

}


public interface IDataType : ICloneable, ISerializationDataProvider
{
    IEntity Parent { get; set; }
    IDbFieldMetadata FieldMetadata { get; set; }

    /// <summary>
    /// True ise veritabanından yükler iken bu alan sorgulanmaz
    /// </summary>
    bool? SkipDirectLoad { get; }

    /// <summary>
    /// True ise veritabanından yükler iken bu alan sorgulanmaz
    /// </summary>
    bool? SkipDirectSet { get; }



    /// <summary>
    /// True ise bu alanın değişikliği veritabanı kaydında sürüm arttırmaya neden olmaz
    /// </summary>
    bool? SkipDbVersioning { get; }

    string TypeName { get; }
    Type BaseType { get; }


    /// <summary>
    /// Alt seviyeden (veritabanına yazılacak) değerini döndürür. Örn; bir reference için id değeri
    /// </summary>
    object GetValueLower();

    /// <summary>
    /// Üst seviyedendeğerini döndürür. Örn; bir reference için entity'nin kendisi
    /// Basit bir değerse ValueGetterLower ile aynı olabilir
    /// </summary>
    object GetValueUpper(IEntity source, string fieldName);


    void SetValue(IEntity source, string fieldName, object value, bool applyToEntity);

    ///// <summary>
    ///// Verilen json nesnesinden değeri alır ve entity'ye set eder
    ///// Ancak bazı türler (Örn relation vs.) için nested entity'ler söz konusu olabilir
    ///// Bu veriye göre güncellenen nested entity'ler döndürülür
    ///// 
    ///// Dikkat; dönülen nested entity'ler tam veri dönmüyor olabilir (IPartialEntity) !!!
    ///// </summary>
    ///// <param name="source"></param>
    ///// <param name="fieldName"></param>
    ///// <param name="dict"></param>
    ///// <returns></returns>
    //IPartialEntity[] SetValue(IEntity source, string fieldName, ObjDictionary dict);

    void SetValuePremature(object value);

    void SetupInstance(IEntity parentEntity, IDbFieldMetadata fm);


    /// <summary>
    /// Modifiye etmesi için column metadata kendisine verilir.
    /// See: Currency ..
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    IDbFieldMetadata SetupMetadata(IDbMetadataContainer container, IDbFieldMetadata self, ObjDictionary values);

    IValidationResult Validate();
}


public interface IDataType<T> : IDataType
{

}



public interface IEntityReleatedMessageArguments : ISignalArguments
{
    IEntity Entity { get; }

}
