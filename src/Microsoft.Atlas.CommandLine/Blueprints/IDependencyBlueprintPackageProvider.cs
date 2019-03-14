// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public interface IDependencyBlueprintPackageProvider
    {
        IBlueprintPackage TryGetBlueprintPackage(IBlueprintPackage package, string blueprint);
    }
}
