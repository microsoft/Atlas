// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Atlas.CommandLine.Accounts
{
    public class ProtectedSettingsFile : ISettingsFile
    {
        private readonly ISettingsFile _file;

        public ProtectedSettingsFile(ISettingsFile file)
        {
            _file = file;
        }

        public void Delete()
        {
            _file.Delete();
        }

        public byte[] ReadAllBytes()
        {
            var bytes = _file.ReadAllBytes();
            if (bytes != null)
            {
                return ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            }
            else
            {
                return null;
            }
        }

        public string ReadAllText()
        {
            var bytes = ReadAllBytes();
            if (bytes != null)
            {
                return Encoding.UTF8.GetString(bytes);
            }
            else
            {
                return null;
            }
        }

        public void WriteAllBytes(byte[] bytes)
        {
            if (bytes != null)
            {
                _file.WriteAllBytes(ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser));
            }
            else
            {
                _file.WriteAllBytes(null);
            }
        }

        public void WriteAllText(string contents)
        {
            if (contents != null)
            {
                _file.WriteAllBytes(Encoding.UTF8.GetBytes(contents));
            }
            else
            {
                _file.WriteAllBytes(null);
            }
        }
    }
}
