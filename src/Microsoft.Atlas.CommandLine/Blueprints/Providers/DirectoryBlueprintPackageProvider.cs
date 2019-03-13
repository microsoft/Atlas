// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using Microsoft.Atlas.CommandLine.Abstractions;

namespace Microsoft.Atlas.CommandLine.Blueprints.Providers
{
    public class DirectoryBlueprintPackageProvider : IBlueprintPackageProvider
    {
        private readonly IFileSystem _fileSystem;

        public DirectoryBlueprintPackageProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IBlueprintPackage TryGetBlueprintPackage(string blueprint)
        {
            if (_fileSystem.DirectoryExists(blueprint))
            {
                return new DirectoryBlueprintPackage(_fileSystem, blueprint);
            }

            return null;
        }
    }
}
