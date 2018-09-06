// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public class ArchiveBlueprintPackageProvider : IBlueprintPackageProvider
    {
        public IBlueprintPackage TryGetBlueprintPackage(string blueprint)
        {
            if (File.Exists(blueprint))
            {
                return new ArchiveBlueprintPackage(blueprint);
            }

            return null;
        }
    }
}
