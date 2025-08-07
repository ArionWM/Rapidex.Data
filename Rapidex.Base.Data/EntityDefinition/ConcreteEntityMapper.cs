using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Rapidex.Data
{
    //TODO: Buna gerek yok?

    [Obsolete("Use DbScope.Mapper", true)]
    public class ConcreteEntityMapper
    {
        public class TypeParameterList
        {
            public Type SourceType { get; set; }
            public List<PropertyInfo> Properties { get; set; }

            public TypeParameterList(Type sourceType)
            {
                SourceType = sourceType;
                Properties = new List<PropertyInfo>();

                this.CreateMap();
            }

            protected void CreateMap()
            {
                var properties = this.SourceType.GetProperties(BindingFlags.Public);
                foreach (var prop in properties)
                {

                    if (!prop.IsUseful())
                        continue;

                    this.Properties.Add(prop);
                }
            }
        }

        Dictionary<Type, TypeParameterList> _mappings = new Dictionary<Type, TypeParameterList>();
        ReaderWriterLockSlim _mappingsLock = new ReaderWriterLockSlim();

        protected TypeParameterList GetMapping(Type type)
        {
            _mappingsLock.EnterUpgradeableReadLock();
            try
            {
                if (_mappings.ContainsKey(type))
                {
                    return _mappings[type];
                }
                else
                {
                    _mappingsLock.EnterWriteLock();
                    try
                    {
                        TypeParameterList tpl = new TypeParameterList(type);
                        _mappings.Add(type, tpl);
                        return tpl;
                    }
                    finally
                    {
                        _mappingsLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _mappingsLock.ExitUpgradeableReadLock();
            }
        }

        public T Map<T>(IEntity source) where T : IConcreteEntity
        {
            Type conctType = typeof(T);

            T concEntity = (T)Database.EntityFactory.Create<T>(source._Scope, false);

            concEntity._IsNew = source._IsNew;
            concEntity.Id = (long)source.GetId();
            concEntity.ExternalId = source.ExternalId;
            concEntity.DbVersion = source.DbVersion;

            TypeParameterList typeParameterList = this.GetMapping(conctType);

            foreach (var prop in typeParameterList.Properties)
            {
                var value = source.GetValue(prop.Name);

                concEntity.SetValue(prop.Name, value);
            }

            concEntity.EnsureDataTypeInitialization();

            return concEntity;
        }

        public T[] Map<T>(IEnumerable<IEntity> source) where T : IConcreteEntity
        {
            List<T> concEntities = new List<T>();
            foreach (var item in source)
            {
                concEntities.Add(this.Map<T>(item));
            }
            return concEntities.ToArray();
        }

    }
}
