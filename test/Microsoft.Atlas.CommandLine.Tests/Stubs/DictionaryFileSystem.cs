// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;
using HandlebarsDotNet;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{
    public class DictionaryFileSystem : ViewEngineFileSystem, IDictionary<string, string>
    {
        public IDictionary<string, string> Files { get; set; } = new Dictionary<string, string>();

        public ICollection<string> Keys => Files.Keys;

        public ICollection<string> Values => Files.Values;

        public int Count => Files.Count;

        public bool IsReadOnly => Files.IsReadOnly;

        public string this[string key] { get => Files[key]; set => Files[key] = value; }

        public void Add(string key, string value)
        {
            Files.Add(key, value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            Files.Add(item);
        }

        public void Clear()
        {
            Files.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return Files.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return Files.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            Files.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Files.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return Files.Remove(key);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return Files.Remove(item);
        }

        public bool TryGetValue(string key, out string value)
        {
            return Files.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Files.GetEnumerator();
        }

        public override bool FileExists(string filePath)
        {
            return Files.ContainsKey(filePath);
        }

        public override string GetFileContent(string filename)
        {
            return Files.TryGetValue(filename, out var value) ? value : null;
        }

        protected override string CombinePath(string dir, string otherFileName)
        {
            return System.IO.Path.Combine(dir, otherFileName);
        }
    }
}
