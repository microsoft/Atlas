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

        public async Task Initialize(SwaggerBlueprintInfo info)
        {
            var swaggerDocuments = new List<SwaggerDocument>();
            var httpClient = _httpClientFactory.Create(null);

            foreach (var input in info.inputs)
            {
                var inputPath = info.source + input;
                if (InnerPackage.Exists(input))
                {
                    using (var yamlReader = InnerPackage.OpenText(inputPath))
                    {
                        var swaggerDocument = _yamlSerializers.YamlDeserializer.Deserialize<SwaggerDocument>(yamlReader);
                        swaggerDocuments.Add(swaggerDocument);
                    }
                }
                else
                {
                    var uriParts = UriParts.Parse(inputPath);
                    uriParts.RewriteGitHubUris();

                    var yamlText = await httpClient.GetStringAsync(uriParts.ToString());
                    var swaggerDocument = _yamlSerializers.YamlDeserializer.Deserialize<SwaggerDocument>(yamlText);
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
                            BlueprintInfo = info,
                            Swagger = swaggerDocument,
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
