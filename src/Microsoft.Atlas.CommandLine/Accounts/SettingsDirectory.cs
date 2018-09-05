// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Atlas.CommandLine.Accounts
{
    public class SettingsDirectory : ISettingsDirectory
    {
        private Lazy<string> _defaultBasePath = new Lazy<string>();
        private string _basePath;

        public SettingsDirectory()
        {
            _defaultBasePath = new Lazy<string>(
                () =>
                {
                    var atlasConfigDir = Environment.GetEnvironmentVariable("ATLAS_CONFIG_DIR");

                    if (string.IsNullOrEmpty(atlasConfigDir))
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            atlasConfigDir = "%USERPROFILE%\\.atlas";
                        }
                        else
                        {
                            atlasConfigDir = "~/.atlas";
                        }
                    }

                    var basePath = Environment.ExpandEnvironmentVariables(atlasConfigDir);
                    if (!Directory.Exists(basePath))
                    {
                        Directory.CreateDirectory(basePath);
                    }

                    return basePath;
                },
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public string GetBasePath()
        {
            return _basePath ?? _defaultBasePath.Value;
        }

        public void SetBasePath(string basePath)
        {
            _basePath = basePath;
        }

        public ISettingsFile GetFile(string filePath, bool sensitive)
        {
            var settingsFile = new SettingsFile(Path.Combine(GetBasePath(), filePath));

            if (sensitive && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new ProtectedSettingsFile(settingsFile);
            }

            return settingsFile;
        }
    }
}
