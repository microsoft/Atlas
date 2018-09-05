// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.Accounts
{
    public class SettingsFile : ISettingsFile
    {
        private string _filePath;

        public SettingsFile(string filePath)
        {
            _filePath = filePath;
        }

        public void Delete()
        {
            File.Delete(_filePath);
        }

        public byte[] ReadAllBytes()
        {
            if (File.Exists(_filePath))
            {
                return File.ReadAllBytes(_filePath);
            }

            return null;
        }

        public string ReadAllText()
        {
            if (File.Exists(_filePath))
            {
                return File.ReadAllText(_filePath);
            }

            return null;
        }

        public void WriteAllBytes(byte[] bytes)
        {
            File.WriteAllBytes(_filePath, bytes);
        }

        public void WriteAllText(string contents)
        {
            File.WriteAllText(_filePath, contents);
        }
    }
}
