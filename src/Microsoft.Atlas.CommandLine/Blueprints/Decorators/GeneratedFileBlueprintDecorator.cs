// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Atlas.CommandLine.Blueprints.Decorators
{
    public abstract class GeneratedFileBlueprintDecorator : IBlueprintPackage
    {
        private readonly IBlueprintPackage _package;

        protected GeneratedFileBlueprintDecorator(IBlueprintPackage package)
        {
            _package = package;
        }

        public Dictionary<string, string> GeneratedFiles { get; } = new Dictionary<string, string>();

        public IBlueprintPackage InnerPackage => _package;

        public string Location => InnerPackage.Location;

        public virtual bool Exists(string path)
        {
            return GeneratedFiles.ContainsKey(path) || InnerPackage.Exists(path);
        }

        public IEnumerable<string> GetGeneratedPaths()
        {
            return InnerPackage
                .GetGeneratedPaths()
                .Concat(GeneratedFiles.Keys)
                .Distinct(GeneratedFiles.Comparer);
        }

        public virtual TextReader OpenText(string path)
        {
            if (GeneratedFiles.TryGetValue(path, out var value))
            {
                return new StringReader(value);
            }

            return InnerPackage.OpenText(path);
        }
    }
}
