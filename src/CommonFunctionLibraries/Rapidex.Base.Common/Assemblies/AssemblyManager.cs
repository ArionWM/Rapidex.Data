using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Rapidex.Base.Common.Assemblies
{
    public class AssemblyManager : IManager
    {
        //Bkz: ProCore / AssemblyManagerV2
        //Bu basitleştirilmişi...

        protected static ConcurrentDictionary<string, Type> Types = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        protected HashSet<AssemblyInfo> assemblyDefinitions = new HashSet<AssemblyInfo>();

        public IReadOnlyCollection<AssemblyInfo> AssemblyDefinitions { get { return assemblyDefinitions.ToArray().AsReadOnly(); } } //Performance?
        public SortedComponentList<IRapidexAssemblyDefinition> AssemblyInstances { get; private set; } = new SortedComponentList<IRapidexAssemblyDefinition>(); //TODO: Readonly


        /// <summary>
        /// Yüklenerek incelenecek assembly path (.dll) ve adlarını (assemblyName) döndürür
        /// </summary>
        /// <returns></returns>
        protected string[] FindAssemblyPathsForCheck()
        {
            HashSet<string> paths = new HashSet<string>();
            paths.Add(Directory.GetFiles(Rapidex.Common.BinaryFolder, "*.dll"));

            return paths.ToArray();
        }

        public AssemblyInfo[] Add(Assembly assembly)
        {
            List<AssemblyInfo> infos = new List<AssemblyInfo>();

            Log.Info($"Checking assembly: {assembly.FullName}");

            Type[] moduleTypes = this.FindDerivedClassTypes(assembly, typeof(IRapidexAssemblyDefinition));
            if (moduleTypes.IsNOTNullOrEmpty())
            {
                foreach (Type type in moduleTypes)
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    IRapidexAssemblyDefinition adefInstance = TypeHelper.CreateInstance<IRapidexAssemblyDefinition>(type);

                    //adefInstance.Name;// assembly.GetName().Name;

                    if (!infos.Find(adefInstance.Name).Any() && !infos.Find(adefInstance.NavigationName).Any())
                        infos.Add(new AssemblyInfo(null, adefInstance.Name, adefInstance.NavigationName, null, assembly, type));
                }
            }
            else
            {
                AssemblyInfo ainfo = new();
                ainfo.Name = assembly.GetName().Name;
                ainfo.Assembly = assembly;
                infos.Add(ainfo);
            }

            this.assemblyDefinitions.Add(infos);

            return infos.ToArray();
        }

        protected AssemblyInfo[] CheckAssemblyInformation(string nameOrPath)
        {
            List<AssemblyInfo> infos = new List<AssemblyInfo>();

            Assembly assembly = null;

            try
            {
                if (File.Exists(nameOrPath))
                    assembly = Assembly.LoadFrom(nameOrPath);
                else
                    assembly = Assembly.Load(nameOrPath);
            }
            catch (Exception ex)
            {
                ex.Log(); //do nothing
                return new AssemblyInfo[0];
            }

            if (assembly.IsDynamic)
                return new AssemblyInfo[0];

            Log.Info($"Checking assembly: {nameOrPath}");
            return this.Add(assembly);
        }


        protected void SearchAssemblies()
        {
            List<AssemblyInfo> proCoreAssemblies = new List<AssemblyInfo>();

            string[] namesAndPaths = this.FindAssemblyPathsForCheck();

            foreach (string nameOrPath in namesAndPaths)
            {
                try
                {
                    AssemblyInfo[] infos = this.CheckAssemblyInformation(nameOrPath);
                    if (infos.IsNOTNullOrEmpty())
                    {
                        proCoreAssemblies.AddRange(infos);
                    }
                }
                catch (Exception ex)
                {
                    ex.Log(); //do nothing
                }
            }

            foreach (var assemblyInfo in this.AssemblyDefinitions)
            {
                if (assemblyInfo.InitializatorType == null)
                    continue; //Assembly don't have a IRapidexAssemblyDefinition derived definition class

                IRapidexAssemblyDefinition proxAssembly = TypeHelper.CreateInstance<IRapidexAssemblyDefinition>(assemblyInfo.InitializatorType);
                this.AssemblyInstances.Add(proxAssembly.Index, proxAssembly);
                assemblyInfo.DatabaseEntityPrefix = proxAssembly.TablePrefix;
                assemblyInfo.Name = proxAssembly.Name;
            }

            //this.Assemblies = proCoreAssemblies.ToList();
        }

        public Type FindType(string typeName, bool deepSearch = false)
        {
            if (typeName.IsNullOrEmpty())
                throw new BaseArgumentNullException("typeName");

            if (Types.ContainsKey(typeName))
                return Types[typeName];

            var assemblies = this.AssemblyDefinitions;

            foreach (var assembly in assemblies)
            {
                Type type = assembly.Assembly.GetType(typeName, false, true);

                if (type != null)
                {
                    Types.Set(typeName, type);
                    return type;
                }
            }

            if (deepSearch)
            {
                foreach (var assembly in assemblies)
                {
                    Type[] _types = assembly.Assembly.GetTypes();
                    foreach (var type in _types)
                    {
                        if (string.Compare(type.Name, typeName, true) == 0)
                        {
                            Types.Set(typeName, type);
                            return type;
                        }
                    }
                }
            }

            return null;
        }

        public Type[] FindDerivedClassTypes(Assembly assembly, Type baseTypeOrInterface)
        {
            try
            {
                List<Type> typeList = new List<Type>();
                Type[] types = assembly.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (baseTypeOrInterface.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        typeList.Add(type);
                }
                return typeList.ToArray();
            }
            catch (ReflectionTypeLoadException rtl)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("--------------------------------------");
                builder.AppendLine($"ReflectionTypeLoadException for: {assembly.FullName} / {assembly.Location}");
                builder.AppendLine("--------------------------------------");
                builder.AppendLine("--------------------------------------");
                builder.AppendLine(rtl.ToString());
                Log.Error(rtl);
                builder.AppendLine("--------------------------------------");
                builder.AppendLine("--------------------------------------");

                foreach (Exception ex in rtl.LoaderExceptions)
                {
                    builder.AppendLine(ex.ToString());
                    Log.Error(ex);

                    builder.AppendLine("--------------------------------------");

                }
                throw new Exception(builder.ToString());
            }
        }

        public Type[] FindDerivedClassTypes(Type baseTypeOrInterface)
        {
            //TODO: Cache. Ancak CheckAssemblyInformation kullanıldığında (yeni eklendiğinde vs.) cache sıfırlanmalı

            HashSet<Type> typeList = new HashSet<Type>();
            foreach (AssemblyInfo ai in this.AssemblyDefinitions)
            {
                Type[] types = FindDerivedClassTypes(ai.Assembly, baseTypeOrInterface);
                typeList.Add(types);
            }

            if (typeList.Contains(baseTypeOrInterface))
                typeList.Remove(baseTypeOrInterface);

            return typeList.ToArray();
        }

        public Type[] FindDerivedClassTypes<T>()
        {
            return FindDerivedClassTypes(typeof(T));
        }


        public Type[] FindDerivedClassTypesWithAssemblyInfo(Type baseTypeOrInterface, AssemblyInfo ainfo)
        {
            //TODO: Cache. Ancak CheckAssemblyInformation kullanıldığında (yeni eklendiğinde vs.) cache sıfırlanmalı
            List<Type> typeList = new();
            Type[] types = FindDerivedClassTypes(ainfo.Assembly, baseTypeOrInterface);
            foreach (var type in types)
            {
                typeList.Add(type);
            }

            return typeList.ToArray();
        }

        public (Type type, AssemblyInfo assembly)[] FindDerivedClassTypesWithAssemblyInfo(Type baseTypeOrInterface)
        {
            //TODO: Cache. Ancak CheckAssemblyInformation kullanıldığında (yeni eklendiğinde vs.) cache sıfırlanmalı

            List<(Type type, AssemblyInfo assembly)> typeList = new List<(Type type, AssemblyInfo assembly)>();
            foreach (AssemblyInfo ai in this.AssemblyDefinitions)
            {
                Type[] types = FindDerivedClassTypes(ai.Assembly, baseTypeOrInterface);
                foreach (var type in types)
                {
                    typeList.Add((type, ai));
                }
            }

            return typeList.ToArray();
        }

        public (Type type, AssemblyInfo assembly)[] FindDerivedClassTypesWithAssemblyInfo<T>()
        {
            return FindDerivedClassTypesWithAssemblyInfo(typeof(T));
        }

        public Type[] FindTypesHasAttribute(Type attributeType, bool inherit)
        {
            var assemblies = this.AssemblyDefinitions;

            List<Type> types = new List<Type>();

            foreach (var assembly in assemblies)
            {
                Type[] atypes = assembly.Assembly.GetTypes();

                foreach (var type in atypes)
                {
                    var attributes = type.GetCustomAttributes(attributeType, inherit);

                    if (attributes.Any())
                    {
                        types.Add(type);
                    }
                }
            }

            return types.ToArray();
        }

        public Type[] FindTypesHasAttribute<T>(bool inherit) where T : Attribute
        {
            return this.FindTypesHasAttribute(typeof(T), inherit);
        }

        public AssemblyInfo FindAssemblyInfo(Assembly assembly)
        {
            return this.AssemblyDefinitions.FirstOrDefault(f => f.Assembly == assembly);
        }

        public AssemblyInfo FindAssemblyInfo(Type type)
        {
            return this.AssemblyDefinitions.FirstOrDefault(f => f.Assembly == type.Assembly);
        }

        public void Setup(IServiceCollection services)
        {
            this.SearchAssemblies();
        }

        public void Start(IServiceProvider serviceProvider)
        {

        }


        public void IterateAsemblies(Action<IRapidexAssemblyDefinition> action)
        {
            foreach (var proxAssembly in this.AssemblyInstances.List)
            {
                action(proxAssembly);
            }
        }

        public void SetupAssemblyServices(IServiceCollection services)
        {
            this.IterateAsemblies(assembly =>
            {
                assembly.SetupServices(services);
            });
        }

        //public void SetupAssemblyMetadata(IServiceCollection services)
        //{
        //    this.IterateAsemblies(assembly =>
        //    {
        //        assembly.SetupMetadata(services);
        //    });
        //}

        public void StartAssemblies(IServiceProvider serviceProvider)
        {
            this.IterateAsemblies(assembly =>
            {
                assembly.Start(serviceProvider);
            });
        }

    }

    public static class AssemblyManagementExtender
    {
        public static AssemblyInfo[] Find(this IEnumerable<AssemblyInfo> infos, string nameOrAssembly)
        {

            return infos.Where(inf => inf.Code == nameOrAssembly || inf.Name == nameOrAssembly || inf.NavigationName == nameOrAssembly || inf.Assembly.FullName == nameOrAssembly || inf.Assembly.GetName().Name == nameOrAssembly).ToArray();
        }
    }
}
