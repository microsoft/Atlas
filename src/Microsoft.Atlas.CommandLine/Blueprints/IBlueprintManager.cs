// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public interface IBlueprintManager
    {
        Task<IBlueprintPackage> GetBlueprintPackage(string blueprint);
        Task<IBlueprintPackage> GetBlueprintPackageDependency(IBlueprintPackage package, string blueprint);
    }
}
