// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Accounts;
using Microsoft.Atlas.CommandLine.ConsoleOutput;
using Microsoft.Extensions.Logging;

namespace Microsoft.Atlas.CommandLine.OAuth2
{
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly ISettingsManager _settingsManager;

        private readonly ILogger<HttpClientFactory> _logger;
        private readonly IConsole _console;
        private readonly IHttpClientHandlerFactory _httpClientHandlerFactory;

        public HttpClientFactory(
            ISettingsManager settingsManager,
            ILogger<HttpClientFactory> logger,
            IConsole console,
            IHttpClientHandlerFactory httpClientHandlerFactory)
        {
            _settingsManager = settingsManager;
            _logger = logger;
            _console = console;
            _httpClientHandlerFactory = httpClientHandlerFactory;
        }

        public HttpClient Create(HttpAuthentication auth)
        {
            HttpMessageHandler handler;

            handler = _httpClientHandlerFactory.Create();

            var settings = _settingsManager.ReadSettings();
            var tokenCache = _settingsManager.GetTokenCache();
            var tokenProvider = new TokenProvider(settings, tokenCache, _console);

            if (auth != null)
            {
                handler = new AuthenticationTokenMessageHandler(() => tokenProvider.AcquireTokenAsync(auth))
                {
                    InnerHandler = handler
                };
            }

            handler = new LambdaDelegatingHandler(handler, SharedKeyAuthentication);

            handler = new LambdaDelegatingHandler(handler, async (request, cancellationToken, next) =>
            {
                var sw = new Stopwatch();
                try
                {
                    // _logger.LogInformation("{Method} {Url}", request.Method, request.RequestUri);
                    _console.WriteLine($"{request.Method.ToString().Color(ConsoleColor.DarkGreen)} {request.RequestUri}");
                    sw.Start();
                    var response = await next(request, cancellationToken);
                    sw.Stop();
                    _console.WriteLine($"{response.StatusCode.ToString().Color((int)response.StatusCode >= 400 ? ConsoleColor.Red : ConsoleColor.Green)} {request.RequestUri} {sw.ElapsedMilliseconds}ms");

                    return response;
                }
                catch
                {
                    Console.WriteLine($"{"FAIL".Color(ConsoleColor.DarkRed)} {request.RequestUri} {sw.ElapsedMilliseconds}ms");
                    throw;
                }
            });

            return new HttpClient(handler);
        }

        private static async Task<HttpResponseMessage> SharedKeyAuthentication(HttpRequestMessage request, CancellationToken cancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> next)
        {
            var authorization = request.Headers.Authorization;
            if (authorization != null && string.Equals(authorization.Scheme, "SharedKey"))
            {
                var parts = authorization.Parameter.Split(":", 2);
                var account = parts[0];
                var key = Convert.FromBase64String(parts[1]);

                var canonicalizedHeaders = request.Headers
                    .Where(header => header.Key.StartsWith("x-ms-"))
                    .Select(header => Tuple.Create(header.Key.ToLowerInvariant(), string.Join(",", header.Value)))
                    .OrderBy(header => header.Item1, StringComparer.InvariantCulture)
                    .Select(header => $"{header.Item1}:{header.Item2}\n")
                    .Aggregate(string.Empty, (a, b) => a + b);

                var canonicalizedResource = $"/{account}{request.RequestUri.AbsolutePath}";

                var signatureString =
                    $"{request.Method}\n" +
                    $"{request.Content.Headers.ContentEncoding}\n" +
                    $"{request.Content.Headers.ContentLanguage}\n" +
                    $"{request.Content.Headers.ContentLength}\n" +
                    $"{request.Content.Headers.ContentMD5}\n" +
                    $"{request.Content.Headers.ContentType}\n" +
                    $"{request.Headers.Date}\n" +
                    $"{request.Headers.IfModifiedSince}\n" +
                    $"{request.Headers.IfMatch}\n" +
                    $"{request.Headers.IfNoneMatch}\n" +
                    $"{request.Headers.IfUnmodifiedSince}\n" +
                    $"{request.Headers.Range}\n" +
                    $"{canonicalizedHeaders}" +
                    $"{canonicalizedResource}";

                var hmac = new HMACSHA256(key);
                var signature = hmac.ComputeHash(Encoding.ASCII.GetBytes(signatureString));

                request.Headers.Authorization = new AuthenticationHeaderValue("SharedKey", $"{account}:{Convert.ToBase64String(signature)}");
            }

            return await next(request, cancellationToken);
        }

        public class AuthenticationTokenMessageHandler : DelegatingHandler
        {
            private readonly Func<Task<AuthenticationHeaderValue>> _authHeaderProvider;

            public AuthenticationTokenMessageHandler(Func<Task<AuthenticationHeaderValue>> tokenProvider)
                : base()
            {
                _authHeaderProvider = tokenProvider;
            }

            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var authHeader = await _authHeaderProvider();
                request.Headers.Authorization = authHeader;
                return await base.SendAsync(request, cancellationToken);
            }
        }

        public class LambdaDelegatingHandler : DelegatingHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>> _sendAsync;

            public LambdaDelegatingHandler(HttpMessageHandler innerHandler, Func<HttpRequestMessage, CancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>, Task<HttpResponseMessage>> sendAsync)
                : base(innerHandler)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _sendAsync.Invoke(request, cancellationToken, base.SendAsync);
        }
    }
}
