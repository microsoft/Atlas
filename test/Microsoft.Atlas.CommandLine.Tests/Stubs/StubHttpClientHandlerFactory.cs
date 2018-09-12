// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Atlas.CommandLine.JsonClient;
using Microsoft.Atlas.CommandLine.OAuth2;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{
    public class StubHttpClientHandlerFactory : IHttpClientHandlerFactory
    {
        public IDictionary<string, IDictionary<HttpMethod, JsonResponse>> Responses { get; set; }

        public HttpMessageHandler Create()
        {
            return new StubHttpClientHandler(this);
        }
    }
}
