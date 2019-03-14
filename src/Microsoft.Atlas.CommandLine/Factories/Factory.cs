// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Atlas.CommandLine.Factories
{
    public class Factory<TService> : IFactory<TService>
    {
        private readonly IServiceProvider _serviceProvider;

        public Factory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TService> Create<TArguments>(TArguments arguments)
        {
            var instance = _serviceProvider.GetService<TService>();
            await ((IFactoryInstance<TArguments>)instance).Initialize(arguments);
            return instance;
        }
    }
}
