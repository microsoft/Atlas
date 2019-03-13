// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Linq;

namespace Microsoft.Atlas.CommandLine.Abstractions
{
    public class FileSystem : IFileSystem
    {
        public string PathCombine(params string[] paths)
        {
            var combined = paths.Aggregate(default(string), (acc, path) =>
            {
                if (acc == null)
                {
                    return path;
                }

                var slashIndex = path.IndexOf('/');
                var backslashIndex = path.IndexOf('\\');
                if (slashIndex == 0 || backslashIndex == 0)
                {
                    return path;
                }

                var schemeDelimiterIndex = path.IndexOf("://");
                if (schemeDelimiterIndex > 0 && schemeDelimiterIndex < slashIndex)
                {
                    return path;
                }

                return acc.TrimEnd('/', '\\') + '/' + path.TrimStart('/', '\\');
            });
            return combined;
        }

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public bool FileExists(string path) => File.Exists(path);

        public TextReader OpenText(string path) => File.OpenText(path);
    }
}
