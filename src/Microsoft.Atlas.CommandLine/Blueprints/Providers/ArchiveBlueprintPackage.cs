// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Atlas.CommandLine.Blueprints.Providers
{
    public class ArchiveBlueprintPackage : IBlueprintPackage
    {
        private readonly string _archivePath;

        public ArchiveBlueprintPackage(string archivePath)
        {
            _archivePath = archivePath;
        }

        public bool Exists(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetGeneratedPaths()
        {
            throw new NotImplementedException();
        }

        public TextReader OpenText(string path)
        {
            throw new NotImplementedException();
        }
    }
}
