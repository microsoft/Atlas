// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public interface IBlueprintPackage
    {
        bool Exists(string path);

        TextReader OpenText(string path);
    }
}
