// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net.Http;

namespace Microsoft.Atlas.CommandLine.OAuth2
{
    public class HttpClientHandlerFactory : IHttpClientHandlerFactory
    {
        public HttpMessageHandler Create()
        {
            return new HttpClientHandler();
        }
    }
}
