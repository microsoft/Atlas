// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Atlas.CommandLine.JsonClient;
using Microsoft.Atlas.CommandLine.OAuth2;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{
    public class StubJsonHttpClientFactory : IJsonHttpClientFactory
    {
        public IDictionary<string, IDictionary<HttpMethod, JsonResponse>> Responses { get; set; }

        public IJsonHttpClient Create(HttpAuthentication auth) => new StubJsonHttpClient(this);
    }
}
