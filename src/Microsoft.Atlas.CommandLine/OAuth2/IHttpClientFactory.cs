// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net.Http;

namespace Microsoft.Atlas.CommandLine.OAuth2
{
    public interface IHttpClientFactory
    {
        HttpClient Create(HttpAuthentication auth);
    }
}
