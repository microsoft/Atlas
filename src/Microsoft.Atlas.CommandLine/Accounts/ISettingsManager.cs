// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Atlas.CommandLine.Accounts
{
    public interface ISettingsManager
    {
        TokenCache GetTokenCache();

        SettingsModel ReadSettings();

        void WriteSettings(SettingsModel settings);
    }
}
