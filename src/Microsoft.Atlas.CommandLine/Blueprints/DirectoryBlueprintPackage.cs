// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public class DirectoryBlueprintPackage : IBlueprintPackage
    {
        private readonly string _directoryPath;

        public DirectoryBlueprintPackage(string directoryPath)
        {
            _directoryPath = directoryPath;
        }

        public bool Exists(string path) => File.Exists(ActualPath(path));

        public TextReader OpenText(string path) => File.OpenText(ActualPath(path));

        private string ActualPath(string path) => Path.Combine(_directoryPath, path);
    }
}
