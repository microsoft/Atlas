// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Atlas.CommandLine.Blueprints.Providers
{
    public class DependencyBlueprintPackage : IBlueprintPackage
    {
        private readonly string _directoryPath;

        public DependencyBlueprintPackage(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        public bool Exists(string path) => File.Exists(ActualPath(path));

        public IEnumerable<string> GetGeneratedPaths() => Enumerable.Empty<string>();

        public TextReader OpenText(string path) => File.OpenText(ActualPath(path));

        private string ActualPath(string path) => Path.Combine(_directoryPath, path);
    }
}
