using Rapidex.Data.Sample.App1.Services;

namespace Rapidex.Data.Sample.App1;

public static class ServicesHelper
{
    public static void AddApplicationServices(this IServiceCollection sc)
    {
        sc.AddScoped<SampleServiceA>();
    }

}
