using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.ConsoleOutput;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.Atlas.CommandLine.Commands
{
    public class SwaggerCommands
    {
        private readonly IConsole _console;
        private readonly IYamlSerializers _serializers;

        public SwaggerCommands(
            IConsole console,
            IYamlSerializers serializers)
        {
            _console = console;
            _serializers = serializers;
        }

        public CommandArgument Specs { get; set; }

        public async Task<int> ExecutePreview()
        {
            var specsUrl = Specs.Required();
            var specsJson = await new HttpClient().GetStringAsync(specsUrl);
            var specs = _serializers.YamlDeserializer.Deserialize<Swagger.Models.SwaggerDocument>(specsJson);

            var refParameters = specs.parameters.ToDictionary(p => $"#/parameters/{p.Key}", p => p.Value);

            foreach (var pathEntry in specs.paths)
            {
                var path = pathEntry.Key;
                var pathItem = pathEntry.Value;

                for (var index = 0; index < pathItem.parameters.Count(); ++index)
                {
                    if (pathItem.parameters[index].@ref != null)
                    {
                        pathItem.parameters[index] = refParameters[pathItem.parameters[index].@ref];
                    }
                }

                foreach (var operationEntry in pathEntry.Value.operations)
                {
                    var context = new Swagger.GenerateSingleRequestDefinitionContext
                    {
                        Swagger = specs,
                        Path = pathEntry,
                        Operation = operationEntry,
                        BlueprintInfo = new Blueprints.Models.SwaggerBlueprintInfo
                        {
                            target = "api/azure",
                        },
                    };
                    var generator = new Swagger.RequestGenerator(new YamlSerializers());
                    generator.GenerateSingleRequestDefinition(context);
                    Console.WriteLine(context.GeneratedPath);
                    Console.WriteLine(context.GeneratedContent);
                }
            }

            return 0;
        }

        private string Stringify(object value)
        {
            if (value is IDictionary)
            {
                return _serializers.JsonSerializer.Serialize(value).TrimEnd('\r', '\n');
            }
            return Convert.ToString(value);
        }
    }
}
