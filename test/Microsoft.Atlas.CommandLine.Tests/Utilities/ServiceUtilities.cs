// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Atlas.CommandLine.Tests.Utilities
{
    public class ServiceUtilities
    {
        public static IServiceProvider CreateServiceProvider(params object[] stubServices)
        {
            var services = Program.AddServices(new ServiceCollection());
            foreach (var stubService in stubServices)
            {
                services.AddStubs(stubService);
            }

            return services.BuildServiceProvider();
        }
    }
}
