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
            var specs = _serializers.YamlDeserializer.Deserialize<Swagger.Models.Swagger>(specsJson);

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
                    var operation = operationEntry.Key;
                    var operationItem = operationEntry.Value;

                    for (var index = 0; index < operationItem.parameters.Count(); ++index)
                    {
                        if (operationItem.parameters[index].@ref != null)
                        {
                            operationItem.parameters[index] = refParameters[operationItem.parameters[index].@ref];
                        }
                    }

                    foreach (var parameter in pathItem.parameters.Where(p1 => operationItem.parameters.Any(p2 => p1.@in == p2.@in && p1.name == p2.name)))
                    {
                        operationItem.parameters.Add(parameter);
                    }

                    Console.WriteLine($"{operationItem.operationId}{operationItem.vendorExtensions.Aggregate("", (a, b) => $"{a} {b.Key}={Stringify(b.Value)}")}");
                    Console.WriteLine($"  {operation} {path}");
                    foreach (var parameter in operationItem.parameters)
                    {
                        Console.WriteLine($"  {parameter.@in} {parameter.name}{(parameter.required ? "!" : "")}{parameter.vendorExtensions.Aggregate("", (a, b) => $"{a} {b.Key}={Stringify(b.Value)}")}");
                    }
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
