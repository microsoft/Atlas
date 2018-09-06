// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Atlas.CommandLine.Accounts
{
    public interface ISettingsFile
    {
        void Delete();

        string ReadAllText();

        byte[] ReadAllBytes();

        void WriteAllText(string contents);

        void WriteAllBytes(byte[] bytes);
    }
}
