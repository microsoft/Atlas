// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.Blueprints.Decorators;
using Microsoft.Atlas.CommandLine.Blueprints.Models;
using Microsoft.Atlas.CommandLine.Blueprints.Providers;
using Microsoft.Atlas.CommandLine.OAuth2;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Atlas.CommandLine.Swagger.Models;

namespace Microsoft.Atlas.CommandLine.Swagger
{
    public class SwaggerBlueprintDecorator : GeneratedFileBlueprintDecorator
    {
        private readonly IRequestGenerator _requestGenerator;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IYamlSerializers _yamlSerializers;

        public SwaggerBlueprintDecorator(
            IBlueprintPackage package,
            IRequestGenerator requestGenerator,
            IHttpClientFactory httpClientFactory,
            IYamlSerializers yamlSerializers)
            : base(package)
        {
            _requestGenerator = requestGenerator;
            _httpClientFactory = httpClientFactory;
            _yamlSerializers = yamlSerializers;
        }

        public async Task Initialize(SwaggerReference info)
        {
            var swaggerDocumentLoader = new SwaggarDocumentLoader(_yamlSerializers, _httpClientFactory);

            var swaggerDocuments = new List<SwaggerDocument>();
            var httpClient = _httpClientFactory.Create(null);

            foreach (var input in info.inputs)
            {
                if (string.IsNullOrEmpty(info.source))
                {
                    // TODO : re-abstract this case into docmgr
                    using (var yamlReader = InnerPackage.OpenText(input))
                    {
                        var swaggerDocument = _yamlSerializers.YamlDeserializer.Deserialize<SwaggerDocument>(yamlReader);
                        swaggerDocuments.Add(swaggerDocument);
                    }
                }
                else
                {
                    var uriParts = UriParts.Parse(info.source);
                    uriParts.RewriteGitHubUris();

                    var swaggerDocument = await swaggerDocumentLoader.LoadDocument(uriParts.ToString(), input);
                    swaggerDocuments.Add(swaggerDocument);
                }
            }

            foreach (var swaggerDocument in swaggerDocuments)
            {
                foreach (var pathEntry in swaggerDocument.paths)
                {
                    foreach (var operationEntry in pathEntry.Value.operations)
                    {
                        var context = new GenerateSingleRequestDefinitionContext
                        {
                            SwaggerReference = info,
                            SwaggerDocument = swaggerDocument,
                            SwaggarDocumentLoader = swaggerDocumentLoader,
                            Path = pathEntry,
                            Operation = operationEntry,
                        };

                        _requestGenerator.GenerateSingleRequestDefinition(context);

                        GeneratedFiles.Add(
                            context.GeneratedPath,
                            context.GeneratedContent);
                    }
                }
            }
        }
    }
}
