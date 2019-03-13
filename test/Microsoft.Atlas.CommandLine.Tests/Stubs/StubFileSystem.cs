// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Atlas.CommandLine.Abstractions;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{

    public class StubFileSystem : IFileSystem
    {
        public static string Normalize(string path)
        {
            var parts = path.Replace("://", ":::").Replace('\\', '/').Split('/');
            var normalizedParts = parts.Aggregate(new Stack<string>(), (stack, part) =>
            {
                var last = stack.LastOrDefault();
                if (part == string.Empty)
                {
                    // consecutive delimiters with no path segment are collapsed
                }
                else if (part == ".." && last != null && last != "..")
                {
                    // back-path removes the previous part, unless it has gone past the root path
                    stack.Pop();
                }
                else
                {
                    stack.Push(part);
                }
                return stack;
            });

            if (parts.Last() == string.Empty)
            {
                // preserve trailing slashes if needed
                normalizedParts.Push(string.Empty);
            }

            var normalized = string.Join('/', normalizedParts.Reverse()).Replace(":::", "://");
            if (!string.Equals(path, normalized, System.StringComparison.Ordinal))
            {
                Console.WriteLine($"{path} -> {normalized}");
            }
            return normalized;
        }

        public IDictionary<string, string> Files { get; set; } = new Dictionary<string, string>();

        bool IFileSystem.DirectoryExists(string path)
        {
            var prefix = ((IFileSystem)this).PathCombine(Normalize(path), string.Empty);
            return Files.Keys.Any(key => key.StartsWith(prefix));
        }

        bool IFileSystem.FileExists(string path) => Files.ContainsKey(Normalize(path));

        TextReader IFileSystem.OpenText(string path) => Files.TryGetValue(Normalize(path), out var text) ? new StringReader(text) : null;

        string IFileSystem.PathCombine(params string[] paths) => paths.Aggregate(default(string), (acc, path) =>
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
    }
}
