// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using HandlebarsDotNet;
using Microsoft.Atlas.CommandLine.Blueprints;

namespace Microsoft.Atlas.CommandLine.Templates.FileSystems
{
    public class BlueprintPackageFileSystem : ViewEngineFileSystem
    {
        private readonly IBlueprintPackage _blueprintPackage;

        public BlueprintPackageFileSystem(IBlueprintPackage blueprintPackage)
        {
            _blueprintPackage = blueprintPackage;
        }

        public override bool FileExists(string filePath)
        {
            return _blueprintPackage.Exists(filePath);
        }

        public override string GetFileContent(string filename)
        {
            using (var reader = _blueprintPackage.OpenText(filename))
            {
                return reader.ReadToEnd();
            }
        }

        protected override string CombinePath(string dir, string otherFileName)
        {
            return Path.Combine(dir, otherFileName);
        }
    }
}
