// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Accounts;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Atlas.CommandLine.OAuth2
{
    public class TokenProvider : ITokenProvider
    {
        private readonly SettingsModel _settings;
        private readonly TokenCache _tokenCache;

        public TokenProvider(SettingsModel settings, TokenCache tokenCache)
        {
            _settings = settings;
            _tokenCache = tokenCache;
        }

        public async Task<AuthenticationHeaderValue> AcquireTokenAsync(string tenant, string resourceId, string clientId)
        {
            // https://login.microsoftonline.com
            // https://login.windows.net
            var authority = $"https://login.windows.net/{tenant ?? "common"}";

            AccountEntry account = null;
            if (account == null)
            {
                account = _settings.accounts.Where(entry => string.Equals(entry.authority, authority, StringComparison.Ordinal) && string.Equals(entry.resource, resourceId, StringComparison.Ordinal)).LastOrDefault();
            }

            if (account == null)
            {
                account = _settings.accounts.Where(entry => string.IsNullOrEmpty(entry.authority) && string.Equals(entry.resource, resourceId, StringComparison.Ordinal)).LastOrDefault();
            }

            if (account == null)
            {
                account = _settings.accounts.Where(entry => string.Equals(entry.authority, authority, StringComparison.Ordinal) && string.IsNullOrEmpty(entry.resource)).LastOrDefault();
            }

            if (account == null)
            {
                account = _settings.accounts.Where(entry => string.IsNullOrEmpty(entry.authority) && string.IsNullOrEmpty(entry.resource)).LastOrDefault();
            }

            var ctx = new AuthenticationContext(authority, _tokenCache);
            AuthenticationResult result = null;

            if (account != null)
            {
                // TODO: split apart the idea of a bearer auth accesstoken and a basic auth PAT
                if (!string.IsNullOrEmpty(account.token))
                {
                    var basic = $":{account.token}";
                    return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(basic)));
                }
                else if (!string.IsNullOrEmpty(account.appid))
                {
                    result = await ctx.AcquireTokenAsync(resourceId, new ClientCredential(account.appid, account.secret));
                }
                else
                {
                    throw new InvalidOperationException("Account entry requires either token or appid and secret values.");
                }
            }
            else
            {
                // TODO: don't go down this path if --noninteractive is set?
                try
                {
                    result = await ctx.AcquireTokenSilentAsync(resourceId, clientId);
                }
                catch
                {
                    DeviceCodeResult codeResult = await ctx.AcquireDeviceCodeAsync(resourceId, clientId);
                    Console.Error.WriteLine(codeResult.Message.Color(ConsoleColor.Yellow));
                    result = await ctx.AcquireTokenByDeviceCodeAsync(codeResult);
                }
            }

            return new AuthenticationHeaderValue(result.AccessTokenType, result.AccessToken);
        }
    }
}
