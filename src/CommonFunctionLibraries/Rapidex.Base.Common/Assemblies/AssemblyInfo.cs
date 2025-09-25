using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace Rapidex.Base.Common.Assemblies
{
    public class AssemblyInfo : IComparable<AssemblyInfo>, IEquatable<AssemblyInfo>
    {
        public string Code { get; internal set; }
        public string Name { get; internal set; }
        public string NavigationName { get; internal set; }
        public string DatabaseEntityPrefix { get; internal set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Assembly Assembly { get; internal set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Type InitializatorType { get; internal set; }

        //For log 
        public string AssemblyName => Assembly?.GetName().Name;

        //For log 
        public string InitializatorTypeName => InitializatorType?.FullName;

        public AssemblyInfo()
        {
            
        }

        public AssemblyInfo(string code, string name, string navigationName, string entityPrefix, Assembly assembly, Type initializatorType)
        {
            this.DatabaseEntityPrefix = entityPrefix;
            this.Code = code;
            this.Name = name;
            this.NavigationName = navigationName;
            this.Assembly = assembly.NotNull();
            this.InitializatorType = initializatorType.NotNull();
        }

        public override string ToString()
        {
            return this.Assembly.FullName;
        }

        public override int GetHashCode()
        {
            if(this.InitializatorType == null)
            {
                return base.GetHashCode();

            }
            return this.InitializatorType.GetHashCode();
        }

        public int CompareTo(AssemblyInfo? other)
        {
            if (object.ReferenceEquals(this, other) || this.InitializatorType == other.InitializatorType)
                return 0;

            return -1;
        }

        public bool Equals(AssemblyInfo? other)
        {
            return object.ReferenceEquals(this, other) || this.InitializatorType == other.InitializatorType;
        }
    }
}
