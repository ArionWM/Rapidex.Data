
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Rapidex.Data.Metadata;
using Rapidex.Data.Scopes;
using static System.Formats.Asn1.AsnWriter;

namespace Rapidex.Data;

public class DbEntityFactory
{
    public class TemplateInfo
    {
        public IEntity Entity { get; set; }
        public string PersistentSequence { get; set; }
        public string PrematureSequence { get; set; }
    }

    protected Dictionary<string, TemplateInfo> Templates { get; set; } = new Dictionary<string, TemplateInfo>();
    protected ReaderWriterLockSlim _templateLock = new ReaderWriterLockSlim();

    /// <summary>
    /// Çözümlenmiş concrete tipler. Entity başına assembly taraması ve tip doğrulaması yapılmaması için.
    /// </summary>
    readonly static ConcurrentDictionary<string, Type> ConcreteTypes = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);



    protected DbEntity Create(IDbEntityMetadata entityMetadata, IDbSchemaScope? scope)
    {
        DbEntity dbEntity = new DbEntity();
        dbEntity._Schema = scope;
        dbEntity._TypeName = entityMetadata.Name;
        dbEntity._DbName = scope?.ParentDbScope.Name;
        dbEntity._SchemaName = scope?.SchemaName;
        dbEntity.Id = DatabaseConstants.DEFAULT_EMPTY_ID;
        dbEntity.ExternalId = null;
        dbEntity.DbVersion = 0;
        return dbEntity;
    }

    protected PartialEntity CreatePartial(IDbEntityMetadata entityMetadata, IDbSchemaScope? scope)
    {
        PartialEntity dbEntity = new PartialEntity();
        dbEntity._Schema = scope;
        dbEntity._TypeName = entityMetadata.Name;
        dbEntity._DbName = scope?.ParentDbScope.Name;
        dbEntity._SchemaName = scope?.SchemaName;
        dbEntity.Id = DatabaseConstants.DEFAULT_EMPTY_ID;
        dbEntity.ExternalId = null;
        dbEntity.DbVersion = 0;
        return dbEntity;
    }


    /// <summary>
    /// Metadata'daki concrete tip adını çözer. Tip doğrulaması yalnızca ilk çözümlemede yapılır.
    /// </summary>
    /// <param name="concreteTypeName">Metadata'da tanımlı concrete tip adı</param>
    internal static Type ResolveConcreteType(string concreteTypeName)
    {
        if (ConcreteTypes.TryGetValue(concreteTypeName, out Type cached))
            return cached;

        Type concreteType = Common.Assembly.FindType(concreteTypeName);
        concreteType.ShouldSupportTo<IEntity>($"Concrete type '{concreteTypeName}' must implement IEntity interface.");
        ConcreteTypes[concreteTypeName] = concreteType;
        return concreteType;
    }

    protected IEntity CreateConcrete(IDbEntityMetadata entityMetadata, IDbSchemaScope? scope)
    {
        Type concreteType = ResolveConcreteType(entityMetadata.ConcreteTypeName);

        IEntity dbEntity = TypeHelper.CreateInstance<IEntity>(concreteType);

        dbEntity._Schema = scope;
        dbEntity._TypeName = entityMetadata.Name;
        dbEntity._DbName = scope?.ParentDbScope.Name;
        dbEntity._SchemaName = scope?.SchemaName;
        if (dbEntity is IIntEntity intEntity)
            intEntity.Id = 0L;
        else
            dbEntity.SetId(0L);
        dbEntity.ExternalId = null;
        dbEntity.DbVersion = 0;
        return dbEntity;
    }

    protected TemplateInfo CreateTemplate(IDbEntityMetadata entityMetadata, IDbSchemaScope scope)
    {
        DbEntity dbEntity = this.Create(entityMetadata, scope);

        TemplateInfo templateInfo = new TemplateInfo();
        templateInfo.Entity = dbEntity;

        templateInfo.PersistentSequence = "id_" + entityMetadata.Name;
        scope.Structure.CreateSequenceIfNotExists(templateInfo.PersistentSequence, -1, 10000);

        templateInfo.PrematureSequence = templateInfo.PersistentSequence + "_prm";
        scope.Structure.CreateSequenceIfNotExists(templateInfo.PrematureSequence, -1, 10000);

        return templateInfo;
    }

    protected TemplateInfo CreateConcreteTemplate(IDbEntityMetadata entityMetadata, IDbSchemaScope scope)
    {
        IEntity dbEntity = this.CreateConcrete(entityMetadata, scope);

        TemplateInfo templateInfo = new TemplateInfo();
        templateInfo.Entity = dbEntity;

        templateInfo.PersistentSequence = "id_" + entityMetadata.Name;
        scope.Structure.CreateSequenceIfNotExists(templateInfo.PersistentSequence, -1, 10000);

        templateInfo.PrematureSequence = templateInfo.PersistentSequence + "_prm";
        scope.Structure.CreateSequenceIfNotExists(templateInfo.PrematureSequence, -1, 10000);

        return templateInfo;
    }

    public TemplateInfo GetTemplate(IDbEntityMetadata em, IDbSchemaScope dbScope)
    {
        string key = dbScope.ParentDbScope.Name + "." + dbScope.SchemaName + "." + em.Name;

        _templateLock.EnterUpgradeableReadLock();
        try
        {
            if (!this.Templates.ContainsKey(key))
            {
                _templateLock.EnterWriteLock();
                try
                {
                    if (em.ConcreteTypeName.IsNullOrEmpty())
                        this.Templates.Add(key, this.CreateTemplate(em, dbScope));
                    else
                        this.Templates.Add(key, this.CreateConcreteTemplate(em, dbScope));
                }
                finally
                {
                    _templateLock.ExitWriteLock();
                }
            }
            return this.Templates[key];
        }

        finally
        {
            _templateLock.ExitUpgradeableReadLock();
        }
    }

    public IEntity Create(IDbEntityMetadata em, IDbSchemaScope dbScope, bool forNew)
    {
        return this.Create(em, dbScope, forNew, true);
    }

    /// <summary>
    /// Entity örneği oluşturur
    /// </summary>
    /// <param name="em">Entity metadata'sı</param>
    /// <param name="dbScope">Entity'nin bağlanacağı şema kapsamı</param>
    /// <param name="forNew">Yeni kayıt için mi oluşturuluyor (premature id alır)</param>
    /// <param name="ensureDataTypes">
    /// IDataType alanları burada ilklendirilsin mi. Yükleme yolunda mapper bu alanları
    /// zaten dolduracağı için false verilir; aksi halde her alan için gereksiz bir örnek üretilir.
    /// </param>
    public IEntity Create(IDbEntityMetadata em, IDbSchemaScope dbScope, bool forNew, bool ensureDataTypes)
    {
        em.NotNull();
        dbScope.NotNull();

        IEntity newEntity = null;

        if (em.ConcreteTypeName.IsNullOrEmpty())
        {
            newEntity = this.Create(em, dbScope);
        }
        else
        {
            newEntity = this.CreateConcrete(em, dbScope);
        }

        newEntity._Metadata = em;

        if (forNew)
        {
            TemplateInfo templateInfo = this.GetTemplate(em, dbScope);

            newEntity._IsNew = true;
            long id = dbScope.Data.Sequence(templateInfo.PrematureSequence).GetNext().Result; //TODO: Slow, much faster tech needed (with GetNextN)
            id = id * -1;
            newEntity.SetId(id);
        }
        else
        {
            newEntity._IsNew = false;
        }

        newEntity._TypeName = em.Name;

        if (ensureDataTypes)
            newEntity.EnsureDataTypeInitialization();

        return newEntity;
    }

    public IEntity Create<T>(IDbSchemaScope dbScope, bool forNew) where T : IConcreteEntity
    {
        IDbEntityMetadata em = dbScope.ParentDbScope.Metadata.Get<T>();
        return this.Create(em, dbScope, forNew);
    }


    //TODO: Test, pEntity.Values boş olmalı 
    public IPartialEntity CreatePartial(IDbEntityMetadata em, IDbSchemaScope dbScope, bool forNew, bool forDeleted)
    {
        em.NotNull();

        IPartialEntity newEntity = null;

        newEntity = this.CreatePartial(em, dbScope);

        newEntity._Metadata = em;

        if (forNew)
        {
            TemplateInfo templateInfo = this.GetTemplate(em, dbScope);

            newEntity._IsNew = true;
            long id = dbScope.Data.Sequence(templateInfo.PrematureSequence).GetNext().Result;
            id = id * -1;
            newEntity.SetId(id);
        }
        else
        {
            newEntity._IsNew = false;
        }

        if (forDeleted)
        {
            newEntity._IsDeleted = true;
        }

        newEntity._TypeName = em.Name;

        return newEntity;
    }


    public void Clear()
    {
        ConcreteTypes.Clear();

        _templateLock.EnterWriteLock();
        try
        {
            this.Templates.Clear();
        }
        finally
        {
            _templateLock.ExitWriteLock();
        }
    }
}
