// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public class DirectoryBlueprintPackageProvider : IBlueprintPackageProvider
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
