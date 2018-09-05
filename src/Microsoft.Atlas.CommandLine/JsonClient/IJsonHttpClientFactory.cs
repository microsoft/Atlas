// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Atlas.CommandLine.OAuth2;

namespace Microsoft.Atlas.CommandLine.JsonClient
{
    public interface IJsonHttpClientFactory
    {
        IJsonHttpClient Create(HttpAuthentication auth);
    }
}
