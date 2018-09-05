// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Atlas.CommandLine.Accounts;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Atlas.CommandLine.OAuth2
{
    // This is a simple persistent cache implementation for a desktop application.
    // It uses DPAPI for storing tokens in a local file.
    public class SettingsFileTokenCache : TokenCache
    {
        private static readonly object FileLock = new object();
        private ISettingsFile _settingsFile;

        // Initializes the cache against a local file.
        // If the file is already present, it loads its content in the ADAL cache
        public SettingsFileTokenCache(ISettingsFile settingsFile)
        {
            _settingsFile = settingsFile;
            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;
            Deserialize(_settingsFile.ReadAllBytes());
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            _settingsFile.Delete();
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Deserialize(_settingsFile.ReadAllBytes());
        }

        // Triggered right after ADAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (HasStateChanged)
            {
                // reflect changes in the persistent store
                _settingsFile.WriteAllBytes(Serialize());

                // once the write operation took place, restore the HasStateChanged bit to false
                HasStateChanged = false;
            }
        }
    }
}
