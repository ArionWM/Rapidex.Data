global using System;
global using System.Linq;
global using System.Collections;
global using System.Collections.Generic;
global using System.Reflection;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using Rapidex;
global using Rapidex.Base;



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
    public const string VERSION = "0.2.10.0";
}