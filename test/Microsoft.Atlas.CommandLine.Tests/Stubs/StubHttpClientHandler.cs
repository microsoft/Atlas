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
            if (_factory.Responses.TryGetValue(request.RequestUri.ToString(), out var address))
            {
                if (address.TryGetValue(request.Method, out var response))
                {
                    var responseMessage = new HttpResponseMessage()
                    {
                        RequestMessage = request,
                        StatusCode = (HttpStatusCode)response.status,
                    };

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

            throw new ApplicationException($"Invalid request url {request.RequestUri}");
        }
    }
}
