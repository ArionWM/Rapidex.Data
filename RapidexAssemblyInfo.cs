global using System;
global using System.Linq;
global using System.Collections;
global using System.Collections.Generic;

global using Rapidex;
global using Rapidex.Base;

global using Microsoft.Extensions.DependencyInjection;



using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#pragma warning disable CS0436

[assembly: AssemblyCompanyAttribute("ProE")]
[assembly: AssemblyProductAttribute("Rapidex Application Framework")]
[assembly: AssemblyVersion(RapidexAssembly.VERSION)]
#pragma warning disable CS0436 // Type conflicts with imported type
[assembly: AssemblyFileVersion(RapidexAssembly.VERSION)]
[assembly: AssemblyInformationalVersion(RapidexAssembly.VERSION)]
#pragma warning restore CS0436 // Type conflicts with imported type

[assembly: AssemblyCopyright("Copyright © ProE. 2024")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif


#pragma warning disable CS8600
public static class RapidexAssembly
{
    //WARN: Yapısında değişiklik yapmayın. TeamCity sürüm numarasını bu satırdan alıyor:
    //^(?<start>\s* public const string VERSION \= \")(?<ver>(([0-9\*])+\.){3})(?<build>([0-9\*])+)(?<end>\"\;)
    //${start}${ver}\%build.number%${end}
    public const string VERSION = "0.0.1.0001";
}