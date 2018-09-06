// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Atlas.CommandLine.OAuth2;
using Microsoft.Atlas.CommandLine.Queries;
using Microsoft.Atlas.CommandLine.Secrets;
using Microsoft.Atlas.CommandLine.Serialization;

namespace Microsoft.Atlas.CommandLine.JsonClient
{
    public class JsonHttpClientFactory : IJsonHttpClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IYamlSerializers _serializers;
        private readonly IJmesPathQuery _jmesPathQuery;
        private readonly ISecretTracker _secretTracker;

        public JsonHttpClientFactory(
            IHttpClientFactory httpClientFactory,
            IYamlSerializers serializers,
            IJmesPathQuery jmesPathQuery,
            ISecretTracker secretTracker)
        {
            _httpClientFactory = httpClientFactory;
            _serializers = serializers;
            _jmesPathQuery = jmesPathQuery;
            _secretTracker = secretTracker;
        }

        public IJsonHttpClient Create(HttpAuthentication auth)
        {
            return new JsonHttpClient(
                _httpClientFactory.Create(auth),
                _serializers.JsonSerializer,
                _serializers.YamlDeserializer,
                _jmesPathQuery,
                _secretTracker);
        }
    }
}
