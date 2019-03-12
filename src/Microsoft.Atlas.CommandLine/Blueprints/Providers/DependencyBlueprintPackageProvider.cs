// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.Blueprints.Providers
{
    public class DependencyBlueprintPackageProvider : IDependencyBlueprintPackageProvider
    {
        public IBlueprintPackage TryGetBlueprintPackage(string blueprint)
        {
            if (Directory.Exists(blueprint))
            {
                return new DirectoryBlueprintPackage(blueprint);
            }

            return null;
        }
    }
}
