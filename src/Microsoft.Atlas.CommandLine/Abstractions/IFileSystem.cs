// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.Abstractions
{
    public interface IFileSystem
    {
        string PathCombine(params string[] paths);

        bool DirectoryExists(string path);

        bool FileExists(string path);

        TextReader OpenText(string path);
    }
}
