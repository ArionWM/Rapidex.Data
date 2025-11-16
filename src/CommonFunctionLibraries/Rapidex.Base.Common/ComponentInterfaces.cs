using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rapidex;



public interface IComponent
{
    [YamlMember(Order = -9999)]
    [JsonPropertyOrder(-9999)]
    string Name { get; }

    [YamlMember(Order = -9998)]
    [JsonPropertyOrder(-9998)]
    string NavigationName { get; }
}

public interface IOrderedComponent : IComponent
{
    int Index { get; }
}

public interface IOneTimeRunnableComponent 
{
    RunnableComponentStatus Status { get;  set; }
    string StatusMessage { get; set; }
    double ProgressPercentage { get; set; }

    event Action OnEnd;
}

/// <summary>
/// Setup zamanında çalışmayan, start ile çalıştırılan servislerdir.
/// </summary>
public interface IManager //TODO: Reengineering for IComponentXXX + IManager
{
    //TODO async
    void Setup(IServiceCollection services);

    //TODO async
    void Start(IServiceProvider serviceProvider);
}

/// <summary>
/// Rapidex library mark. 
/// IRapidexModule contained libraries are scanned and loaded by Rapidex framework. 
/// Other libraries are ignored.
/// </summary>
public interface IRapidexAssemblyDefinition : IOrderedComponent
{
    string TablePrefix { get; }
    void SetupServices(IServiceCollection services);

    void Initialize(IServiceProvider serviceProvider); 
    void Start(IServiceProvider serviceProvider); 
}


