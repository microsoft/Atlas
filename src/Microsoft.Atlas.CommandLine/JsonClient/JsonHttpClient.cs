// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Queries;
using Microsoft.Atlas.CommandLine.Secrets;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.JsonClient
{
    public class JsonHttpClient : IJsonHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly Serializer _serializer;
        private readonly Deserializer _deserializer;
        private readonly IJmesPathQuery _jmesPath;
        private readonly ISecretTracker _secretTracker;

        public JsonHttpClient(
            HttpClient httpClient,
            Serializer serializer,
            Deserializer deserializer,
            IJmesPathQuery jmesPath,
            ISecretTracker secretTracker)
        {
            _httpClient = httpClient;
            _serializer = serializer;
            _deserializer = deserializer;
            _jmesPath = jmesPath;
            _secretTracker = secretTracker;
        }

        public Task<JsonResponse> SendAsync(JsonRequest request) => SendAsync<object>(request);

        public async Task<JsonResponse> SendAsync<TResponse>(JsonRequest jsonRequest)
        {
            var request = new HttpRequestMessage(jsonRequest.method, jsonRequest.url);
            if (jsonRequest.body != null)
            {
                if (jsonRequest.body is string)
                {
                    request.Content = new StringContent((string)jsonRequest.body);
                }
                else if (jsonRequest.body is byte[])
                {
                    request.Content = new ByteArrayContent((byte[])jsonRequest.body);
                }
                else
                {
                    var memoryStream = new MemoryStream();
                    using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true))
                    {
                        _serializer.Serialize(writer, jsonRequest.body);
                    }

                    memoryStream.Position = 0;
                    request.Content = new StreamContent(memoryStream)
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/json")
                        }
                    };
                }
            }

            if (jsonRequest.headers != null)
            {
                foreach (var kv in jsonRequest.headers)
                {
                    var name = kv.Key.ToString();
                    IEnumerable<string> values;
                    if (kv.Value is IList<object>)
                    {
                        values = ((IList<object>)kv.Value).Select(value => value.ToString());
                    }
                    else
                    {
                        values = new[] { kv.Value.ToString() };
                    }

                    foreach (var value in values)
                    {
                        if (!request.Headers.TryAddWithoutValidation(name, values))
                        {
                            request?.Content.Headers.TryAddWithoutValidation(name, values);
                        }
                    }
                }
            }

            var response = await _httpClient.SendAsync(request);

            var jsonResponse = new JsonResponse
            {
                status = response.StatusCode
            };

            // var responseBody = await response.Content.ReadAsStringAsync();
            // using (var reader = new StringReader(responseBody))
            using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            {
                jsonResponse.body = _deserializer.Deserialize<TResponse>(reader);
            }

            // Information is added to secret tracker as soon as possible
            if (!string.IsNullOrEmpty(jsonRequest.secret) && jsonResponse.body != null)
            {
                var searchResult = _jmesPath.Search(jsonRequest.secret, jsonResponse.body);

                var secrets = searchResult as IEnumerable<object>;
                if (secrets != null)
                {
                    foreach (var secret in secrets)
                    {
                        if (secret != null)
                        {
                            _secretTracker.AddSecret(secret.ToString());
                        }
                    }
                }
                else
                {
                    if (searchResult != null)
                    {
                        _secretTracker.AddSecret(searchResult.ToString());
                    }
                }
            }

            return jsonResponse;
        }
    }
}
