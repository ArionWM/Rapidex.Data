﻿

using Mapster;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

[assembly: InternalsVisibleTo("Rapidex.Base.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]

namespace Rapidex;

internal class Library : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "Common Library";
    public override string TablePrefix => "core";
    public override int Index => 1;



    public override void SetupServices(IServiceCollection services)
    {
        //services.AddSingleton<IServiceScopeFactory, RapidexServiceScopeFactory>();

        services.Configure<JsonSerializerOptions>(opt => opt.SetDefaultOptions());

        services.AddSingletonForProd<ITimeProvider, DefaultTimeProvider>();

        services.AddSingletonForProd<TypeConverter>(Common.Converter);
        services.AddSingletonForProd<ExceptionManager, ExceptionManager>();
        services.AddSingletonForProd<IExceptionManager, ExceptionManager>();
        services.AddSingletonForProd<IExceptionTranslator, CommonExceptionTranslator>();
    }

    public override void Start(IServiceProvider serviceProvider)
    {

    }
}
