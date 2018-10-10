// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Atlas.CommandLine.OAuth2;

namespace Microsoft.Atlas.CommandLine.Blueprints.Providers
{
    public class HttpsFilesBlueprintPackageProvider : IBlueprintPackageProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpsFilesBlueprintPackageProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IBlueprintPackage TryGetBlueprintPackage(string blueprint)
        {
            if (!blueprint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var uriParts = UriParts.Parse(blueprint);
            uriParts.RewriteGitHubUris();
            uriParts.RemoveWorkflowYaml();
            return new HttpsFilesBlueprintPackage(_httpClientFactory, uriParts);
        }
    }
}
