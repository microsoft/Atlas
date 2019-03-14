// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Blueprints.Models;
using Microsoft.Atlas.CommandLine.Factories;

namespace Microsoft.Atlas.CommandLine.Blueprints.Dependencies
{
    public class DependencyBlueprintDecoratorProvider : IBlueprintDecoratorProvider
    {
        private readonly IFactory<DependencyBlueprintDecorator> _factory;

        public DependencyBlueprintDecoratorProvider(IFactory<DependencyBlueprintDecorator> factory)
        {
            _factory = factory;
        }

        public async Task<IBlueprintPackage> CreateDecorator<TInfo>(TInfo info, IBlueprintPackage package)
        {
            if (info is DependencyReference dependencyInfo)
            {
                return await _factory.Create((dependencyInfo, package));
            }

            return package;
        }
    }
}
