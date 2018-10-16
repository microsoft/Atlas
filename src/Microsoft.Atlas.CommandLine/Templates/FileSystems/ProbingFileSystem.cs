// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HandlebarsDotNet;

namespace Microsoft.Atlas.CommandLine.Templates.FileSystems
{
    public class ProbingFileSystem : ViewEngineFileSystem
    {
        private readonly ViewEngineFileSystem _inner;

        public ProbingFileSystem(ViewEngineFileSystem inner)
        {
            _inner = inner;
        }

        public override bool FileExists(string filePath) => _inner.FileExists(filePath);

        public override string GetFileContent(string filePath) => _inner.GetFileContent(filePath);

        protected override string CombinePath(string filePath, string relativeFilePame)
        {
            foreach (var probePath in new[] { relativeFilePame }.SelectMany(WithoutPartialsFolder).SelectMany(WithoutExtension))
            {
                var combinedPath = _inner.Closest(Path.Combine(filePath, "_"), probePath);
                if (combinedPath != null)
                {
                    return combinedPath;
                }
            }

            return Path.Combine(filePath, relativeFilePame);
        }

        private IEnumerable<string> WithoutPartialsFolder(string filePath)
        {
            const string partialsFolder = "partials/";
            if (filePath.StartsWith(partialsFolder, StringComparison.Ordinal))
            {
                yield return filePath.Substring(partialsFolder.Length);
            }

            yield return filePath;
        }

        private IEnumerable<string> WithoutExtension(string filePath)
        {
            const string hbsExtension = "hbs";
            if (filePath.EndsWith(hbsExtension))
            {
                yield return filePath.Substring(0, filePath.Length - hbsExtension.Length);
            }

            yield return filePath;
        }
    }
}
