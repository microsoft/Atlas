// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Accounts;
using Microsoft.Atlas.CommandLine.ConsoleOutput;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.Atlas.CommandLine.Commands
{
    public class AccountCommands
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IYamlSerializers _yamlSerializers;
        private readonly IConsole _console;

        public AccountCommands(
            ISettingsManager settingsManager,
            IYamlSerializers yamlSerializers,
            IConsole console)
        {
            _settingsManager = settingsManager;
            _yamlSerializers = yamlSerializers;
            _console = console;
        }

        public CommandOption Name { get; set; }

        public CommandOption Authority { get; set; }

        public CommandOption Tenant { get; set; }

        public CommandOption Resource { get; set; }

        public CommandOption Token { get; set; }

        public CommandOption Appid { get; set; }

        public CommandOption Username { get; set; }

        public CommandOption Password { get; set; }

        public CommandOption Pat { get; set; }

        public CommandOption Secret { get; set; }

        public async Task<int> ExecuteClear()
        {
            var settings = _settingsManager.ReadSettings();

            settings.accounts = new List<AccountEntry>();

            _settingsManager.WriteSettings(settings);

            return 0;
        }

        public async Task<int> ExecuteAdd()
        {
            var settings = _settingsManager.ReadSettings();

            var name = Name.Optional(null);
            var authority = Authority.Optional(null);
            var tenant = Tenant.Optional(null);
            var resource = Resource.Optional(null);

            var appid = Appid.Optional(null);
            var secret = Secret.Optional(null);

            var username = Username.Optional(null);
            var password = Password.Optional(null);

            var token = Token.Optional(null);

            if (tenant != null)
            {
                authority = $"https://login.windows.net/{tenant}";
            }

            if (name != null)
            {
                settings.accounts = settings.accounts
                    .Where(x => !string.Equals(x.name, name, StringComparison.Ordinal))
                    .ToList();
            }

            var entry = new AccountEntry
            {
                name = name,
                resource = resource,
                authority = authority,
                token = token,
                appid = appid,
                secret = secret,
            };
            settings.accounts.Add(entry);

            _settingsManager.WriteSettings(settings);

            return 0;
        }

        public async Task<int> ExecuteShow()
        {
            var settings = _settingsManager.ReadSettings();

            var nameFilter = Name.HasValue() ? Name.Values : null;

            var accounts = settings.accounts
                .Where(x => nameFilter == null || nameFilter.Contains(x.name, StringComparer.Ordinal))
                .Select(RedactSecrets)
                .ToList();

            _yamlSerializers.YamlSerializer.Serialize(_console.Out, accounts);

            return 0;
        }

        private AccountEntry RedactSecrets(AccountEntry entry)
        {
            return new AccountEntry
            {
                name = entry.name,
                authority = entry.authority,
                resource = entry.resource,
                appid = entry.appid,
                secret = string.IsNullOrEmpty(entry.secret) ? entry.secret : "***",
                token = string.IsNullOrEmpty(entry.token) ? entry.token : "***",
                username = entry.username,
                password = string.IsNullOrEmpty(entry.token) ? entry.password : "***",
            };
        }
    }
}
