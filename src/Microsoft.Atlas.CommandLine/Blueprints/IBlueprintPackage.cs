// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public interface IBlueprintPackage
    {
        string Location { get; }

        bool Exists(string path);

        TextReader OpenText(string path);

        IEnumerable<string> GetGeneratedPaths();
    }
}
