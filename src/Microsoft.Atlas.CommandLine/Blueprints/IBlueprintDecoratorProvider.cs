// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public interface IBlueprintDecoratorProvider
    {
        Task<IBlueprintPackage> CreateDecorator<TInfo>(TInfo info, IBlueprintPackage package);
    }
}
