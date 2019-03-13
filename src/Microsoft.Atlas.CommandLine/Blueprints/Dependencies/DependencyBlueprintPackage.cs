// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Atlas.CommandLine.Abstractions;

namespace Microsoft.Atlas.CommandLine.Blueprints.Dependencies
{
    public class DependencyBlueprintPackage : IBlueprintPackage
    {
        private readonly IFileSystem _fileSystem;
        private readonly IBlueprintPackage _parent;
        private readonly string _directoryPath;

        public DependencyBlueprintPackage(IFileSystem fileSystem, IBlueprintPackage parent, string directoryPath)
        {
            _fileSystem = fileSystem;
            _parent = parent;
            _directoryPath = directoryPath;
        }

        public string Location => _fileSystem.PathCombine(_parent.Location, _directoryPath);

        public bool Exists(string path) => _parent.Exists(ActualPath(path));

        public IEnumerable<string> GetGeneratedPaths() => Enumerable.Empty<string>();

        public TextReader OpenText(string path) => _parent.OpenText(ActualPath(path));

        private string ActualPath(string path) => _fileSystem.PathCombine(_directoryPath, path);
    }
}
