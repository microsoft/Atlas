// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.JsonClient;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{
    internal class StubJsonHttpClient : IJsonHttpClient
    {
        private StubJsonHttpClientFactory _factory;

        public StubJsonHttpClient(StubJsonHttpClientFactory factory)
        {
            _factory = factory;
        }

        async Task<JsonResponse> IJsonHttpClient.SendAsync(JsonRequest request)
        {
            if (_factory.Responses.TryGetValue(request.url,  out var address))
            {
                if (address.TryGetValue(request.method, out var response))
                {
                    return response;
                }

                throw new ApplicationException($"Invalid request method {request.method}");
            }

            throw new ApplicationException($"Invalid request url {request.url}");
        }
    }
}
