// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HandlebarsDotNet;

namespace Microsoft.Atlas.CommandLine.Templates.FileSystems
{
    public class BuiltInTemplatesFileSystem : ViewEngineFileSystem
    {
        private readonly ViewEngineFileSystem _inner;
        private readonly IDictionary<string, string> _files;

        public BuiltInTemplatesFileSystem(ViewEngineFileSystem inner, IDictionary<string, string> files)
        {
            _inner = inner;
            _files = files;
        }

        public override bool FileExists(string filePath) => _files.ContainsKey(filePath) || _inner.FileExists(filePath);

        public override string GetFileContent(string filePath) => _files.TryGetValue(filePath, out var value) ? value : _inner.GetFileContent(filePath);

        protected override string CombinePath(string filePath, string relativeFileName) => Path.Combine(filePath, relativeFileName);
    }
}
