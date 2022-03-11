using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ObririUssd
{
    public static class ServiceContext
    {
        static IServiceProvider _provider;
        public static object GetService(Type type)
        {
            return _provider.GetService(type);
        }

        public static IHost SetServiceContext(this IHost host)
        {
            _provider = host.Services;
            return host;
        }

        public static IServiceScope ServiceProvider() => _provider.CreateScope();
        
    }
}
