using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.Blueprints.Models;
using Microsoft.Atlas.CommandLine.OAuth2;
using Microsoft.Atlas.CommandLine.Serialization;

namespace Microsoft.Atlas.CommandLine.Swagger
{
    public class SwaggerBlueprintDecoratorProvider : IBlueprintDecoratorProvider
    {
        private readonly IRequestGenerator _requestGenerator;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IYamlSerializers _yamlSerializers;

        public SwaggerBlueprintDecoratorProvider(
            IRequestGenerator requestGenerator,
            IHttpClientFactory httpClientFactory,
            IYamlSerializers yamlSerializers)
        {
            _requestGenerator = requestGenerator;
            _httpClientFactory = httpClientFactory;
            _yamlSerializers = yamlSerializers;
        }

        public async Task<IBlueprintPackage> CreateDecorator<TInfo>(TInfo info, IBlueprintPackage package)
        {
            if (info is SwaggerBlueprintInfo swaggerBlueprintInfo)
            {
                var decorator = new SwaggerBlueprintDecorator(
                    package,
                    _requestGenerator,
                    _httpClientFactory,
                    _yamlSerializers);

                await decorator.Initialize(swaggerBlueprintInfo);

                return decorator;
            }

            return package;
        }
    }
}
