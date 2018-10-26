using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Atlas.CommandLine.Swagger.Models;

namespace Microsoft.Atlas.CommandLine.Swagger
{
    public class RequestGenerator : IRequestGenerator
    {
        private readonly IYamlSerializers _yamlSerializers;

        public RequestGenerator(IYamlSerializers yamlSerializers)
        {
            _yamlSerializers = yamlSerializers;
        }

        public void GenerateSingleRequestDefinition(GenerateSingleRequestDefinitionContext context)
        {
            var generatedPath = NormalizePath(context.TargetPrefix);
            generatedPath += NormalizePath(context.Swagger.info.title);
            var operationId = context.Operation.Value.operationId;
            foreach (var tag in context.Operation.Value.tags ?? Enumerable.Empty<string>())
            {
                generatedPath += NormalizePath(tag);

                if (operationId.StartsWith(tag + "_", StringComparison.Ordinal))
                {
                    operationId = operationId.Substring(tag.Length + 1);
                }
            }

            generatedPath += NormalizePath(operationId + ".yaml");

            context.GeneratedPath = generatedPath.TrimStart('/');


            var parameters = GetParameterList(context);

            using (var writer = new StringWriter())
            {
                writer.WriteLine($"method: {context.Operation.Key.ToUpperInvariant()}");

                var hasHttps = context.Swagger.schemes?.Contains("https", StringComparer.OrdinalIgnoreCase) ?? false;
                var hasHttp = context.Swagger.schemes?.Contains("http", StringComparer.OrdinalIgnoreCase) ?? false;
                var scheme = (hasHttp && !hasHttps) ? "http" : "https";

                var host = context.Swagger.host ?? "{{ request.host }}";

                var basePath = context.Swagger.basePath ?? string.Empty;

                var path = context.Path.Key;

                foreach (var parameter in parameters.Where(parameter => parameter.@in == "path"))
                {
                    path = path.Replace($"{{{parameter.name}}}", GetParameterExpression(parameter, context));
                }

                writer.WriteLine($"url: {scheme}://{host}{basePath}{path}");

                if (parameters.Where(parameter => parameter.@in == "query").Any())
                {
                    writer.WriteLine("query:");
                    foreach (var parameter in parameters.Where(parameter => parameter.@in == "query"))
                    {
                        writer.WriteLine($"  {parameter.name}: {GetParameterExpression(parameter, context)}");
                    }
                }

                if (context.BlueprintInfo.extra != null)
                {
                    writer.WriteLine(_yamlSerializers.YamlSerializer.Serialize(context.BlueprintInfo.extra));
                }

                context.GeneratedContent = writer.GetStringBuilder().ToString();
            }
        }

        private string GetParameterExpression(Parameter parameter, GenerateSingleRequestDefinitionContext context)
        {
            var parameterDefault = parameter.@default;
            if (parameterDefault == null && string.Equals(parameter.name, "api-version", StringComparison.Ordinal))
            {
                parameterDefault = context?.Swagger?.info?.version;
            }

            if (parameterDefault != null)
            {
                return $"{{{{# if parameters.{parameter.name} }}}}{{{{ parameters.{parameter.name} }}}}{{{{ else }}}}{parameterDefault}{{{{/ if }}}}";
            }
            else
            {
                return $"{{{{ parameters.{parameter.name} }}}}";
            }
        }

        private List<Models.Parameter> GetParameterList(GenerateSingleRequestDefinitionContext context)
        {
            var refParameters = context.Swagger.parameters.ToDictionary(kv => $"#/parameters/{kv.Key}", kv => kv.Value);

            Models.Parameter LookupParameterRef(Models.Parameter parameter)
            {
                if (parameter.@ref == null)
                {
                    return parameter;
                }
                else
                {
                    return refParameters[parameter.@ref];
                }
            }

            var operationParamters = context.Operation.Value.parameters.Select(LookupParameterRef);

            bool IsNotOperationParameter(Models.Parameter pathParameter)
            {
                return !operationParamters.Any(operationParameter =>
                {
                    return string.Equals(pathParameter.name, operationParameter.name, StringComparison.Ordinal) && string.Equals(pathParameter.name, operationParameter.name, StringComparison.Ordinal);
                });
            }

            var pathParameters = context.Path.Value.parameters.Select(LookupParameterRef).Where(IsNotOperationParameter);

            return operationParamters.Concat(pathParameters).ToList();
        }

        private string NormalizePath(string path)
        {
            return '/' + path.Trim('/');
        }

        private string ToPathString(object defaultValue)
        {
            return defaultValue?.ToString() ?? string.Empty;
        }
    }
}
