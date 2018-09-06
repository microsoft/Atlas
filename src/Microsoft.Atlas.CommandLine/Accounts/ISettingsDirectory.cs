// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Atlas.CommandLine.Accounts
{
    public interface ISettingsDirectory
    {
        string GetBasePath();

        void SetBasePath(string basePath);

        ISettingsFile GetFile(string filePath, bool sensitive);
    }
}
