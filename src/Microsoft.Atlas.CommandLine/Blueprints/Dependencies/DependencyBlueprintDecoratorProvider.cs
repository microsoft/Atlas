// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.Atlas.CommandLine.Blueprints.Dependencies
{
    public class DependencyBlueprintDecoratorProvider : IBlueprintDecoratorProvider
    {
        public async Task<IBlueprintPackage> CreateDecorator<TInfo>(TInfo info, IBlueprintPackage package)
        {
            return package;
        }
    }
}
