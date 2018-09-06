// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Atlas.CommandLine.OAuth2;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Atlas.CommandLine.Accounts
{
    public class SettingsManager : ISettingsManager
    {
        private readonly ISettingsDirectory _settingsDirectory;
        private readonly IYamlSerializers _yamlSerializers;

        public SettingsManager(ISettingsDirectory settingsDirectory, IYamlSerializers yamlSerializers)
        {
            _settingsDirectory = settingsDirectory;
            _yamlSerializers = yamlSerializers;
        }

        public TokenCache GetTokenCache()
        {
            return new SettingsFileTokenCache(_settingsDirectory.GetFile("TokenCache.dat", sensitive: true));
        }

        public SettingsModel ReadSettings()
        {
            var file = _settingsDirectory.GetFile(
                "settings.yaml",
                sensitive: false);

            var contents = file.ReadAllText();

            if (contents != null)
            {
                return _yamlSerializers.YamlDeserializer.Deserialize<SettingsModel>(contents);
            }
            else
            {
                return new SettingsModel();
            }
        }

        public void WriteSettings(SettingsModel settings)
        {
            var file = _settingsDirectory.GetFile(
                "settings.yaml",
                sensitive: false);

            var contents = _yamlSerializers.YamlSerializer.Serialize(settings);

            file.WriteAllText(contents);
        }
    }
}
