using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace DingleTheBotReboot.Helpers
{
    public class BotHostBuilder
    {
        private readonly IConfiguration _config;
        private readonly IServiceCollection _services;

        public BotHostBuilder(IConfiguration config, IServiceCollection services)
        {
            _config = config;
            _services = services;
        }

        public BotHostBuilder UseStartup<T>()
        {
            Type type = typeof(T);
            System.Reflection.MethodInfo configServices = type.GetMethod("ConfigureServices");
            System.Reflection.ConstructorInfo constructor = type.GetConstructor(new[] { typeof(IConfiguration) });
            object instance = constructor is null
                ? Activator.CreateInstance(type)
                : Activator.CreateInstance(type, _config);

            Debug.Assert(
                configServices != null,
                $"{type} must contain: public void ConfigureServices(IServiceCollection services)"
            );
            configServices.Invoke(instance, new object[] { _services });
            return this;
        }
    }
}
