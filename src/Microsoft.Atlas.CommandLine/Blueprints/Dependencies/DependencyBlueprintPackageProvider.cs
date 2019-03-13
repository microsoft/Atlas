// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using Microsoft.Atlas.CommandLine.Abstractions;

namespace Microsoft.Atlas.CommandLine.Blueprints.Dependencies
{
    public class DependencyBlueprintPackageProvider : IDependencyBlueprintPackageProvider
    {
        private readonly IFileSystem _fileSystem;

        public DependencyBlueprintPackageProvider(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IBlueprintPackage TryGetBlueprintPackage(IBlueprintPackage parent, string blueprint)
        {
            if (parent.Exists(_fileSystem.PathCombine(blueprint, "workflow.yaml")))
            {
                return new DependencyBlueprintPackage(_fileSystem, parent, blueprint);
            }

            return null;
        }
    }
}
