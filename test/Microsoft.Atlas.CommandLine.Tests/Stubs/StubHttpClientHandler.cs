// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{
    public class StubHttpClientHandler : HttpMessageHandler
    {
        private readonly StubHttpClientHandlerFactory _factory;

        public StubHttpClientHandler(StubHttpClientHandlerFactory factory)
        {
            _factory = factory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _factory.Requests.Add(request);

            if (_factory.AllResponses.TryGetValue(StubFileSystem.Normalize(request.RequestUri.ToString()), out var address))
            {
                // Find method, or fall back to get if method is head
                if (address.TryGetValue(request.Method, out var response) ||
                    (request.Method == HttpMethod.Head && address.TryGetValue(HttpMethod.Get, out response)))
                {
                    var responseMessage = new HttpResponseMessage()
                    {
                        RequestMessage = request,
                        StatusCode = (HttpStatusCode)response.status,
                    };

                    if (request.Method != HttpMethod.Head)
                    {
                        if (response.body is byte[] responseBytes)
                        {
                            responseMessage.Content = new ByteArrayContent(responseBytes);
                        }
                        else if (response.body is string responseString)
                        {
                            responseMessage.Content = new StringContent(responseString);
                        }
                        else if (response.body != null)
                        {
                            responseMessage.Content = new StringContent(JsonConvert.SerializeObject(response.body));
                        }
                    }

                    foreach (var header in response.headers ?? Enumerable.Empty<KeyValuePair<object, object>>())
                    {
                        var name = Convert.ToString(header.Key);
                        foreach (var value in ((List<object>)header.Value).Select(Convert.ToString))
                        {
                            if (!responseMessage.Headers.TryAddWithoutValidation(name, value))
                            {
                                Assert.IsTrue(responseMessage.Content.Headers.TryAddWithoutValidation(name, value));
                            }
                        }
                    }

                    return responseMessage;
                }

                throw new ApplicationException($"Invalid request method {request.Method}");
            }

            // provide simple 404 response if the request is to an implied server
            if (_factory.AllResponses.Any(kv => kv.Key.StartsWith($"{request.RequestUri.Scheme}://{request.RequestUri.Host}")))
            {
                return new HttpResponseMessage
                {
                    RequestMessage = request,
                    StatusCode = HttpStatusCode.NotFound,
                };
            }

            throw new ApplicationException($"Invalid request url {request.RequestUri}");
        }
    }
}
