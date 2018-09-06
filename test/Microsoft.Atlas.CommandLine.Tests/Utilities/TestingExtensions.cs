// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Atlas.CommandLine.Tests
{
    public static class TestingExtensions
    {
        public static IServiceCollection AddStubs(this IServiceCollection services, object stubs)
        {
            foreach (var itf in stubs.GetType().GetTypeInfo().GetInterfaces())
            {
                services.AddSingleton(itf, stubs);
            }

            return services;
        }
    }
}
