// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public interface IBlueprintPackageProvider
    {
        IBlueprintPackage TryGetBlueprintPackage(string blueprint);
    }
}
