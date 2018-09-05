// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Atlas.CommandLine.Tests.Utilities
{
    public class ServiceContextBase : IDisposable, IServiceProvider
    {
        public IServiceProvider ServiceProvider { get; set; }

        public static TServiceContext Create<TServiceContext>(params object[] stubServices)
            where TServiceContext : ServiceContextBase, new()
        {
            var services = Program.AddServices(new ServiceCollection());
            foreach (var stubService in stubServices)
            {
                services.AddStubs(stubService);
            }

            var serviceProvider = services.BuildServiceProvider();

            Program.ConfigureApplicationCommands(serviceProvider.GetRequiredService<CommandLineApplicationServices>());

            var serviceContext = new TServiceContext();
            foreach (var property in serviceContext.GetType().GetTypeInfo().DeclaredProperties)
            {
                if (property.CanWrite)
                {
                    var service = serviceProvider.GetRequiredService(property.PropertyType);
                    property.SetValue(serviceContext, service);
                }
            }

            return serviceContext;
        }

        public virtual void Dispose()
        {
            (ServiceProvider as IDisposable)?.Dispose();
        }

        public virtual object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }
    }
}
