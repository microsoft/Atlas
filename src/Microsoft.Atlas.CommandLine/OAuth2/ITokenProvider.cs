// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Atlas.CommandLine.OAuth2
{
    public interface ITokenProvider
    {
        Task<AuthenticationHeaderValue> AcquireTokenAsync(HttpAuthentication auth);
    }
}
