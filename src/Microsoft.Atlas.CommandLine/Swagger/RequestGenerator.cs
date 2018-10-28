// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
            var generatedPath = NormalizePath(context.SwaggerReference.target);
            generatedPath += NormalizePath(context.SwaggerDocument.info.title);
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

                var hasHttps = context.SwaggerDocument.schemes?.Contains("https", StringComparer.OrdinalIgnoreCase) ?? false;
                var hasHttp = context.SwaggerDocument.schemes?.Contains("http", StringComparer.OrdinalIgnoreCase) ?? false;
                var scheme = (hasHttp && !hasHttps) ? "http" : "https";

                var host = context.SwaggerDocument.host ?? "{{ request.host }}";

                var basePath = context.SwaggerDocument.basePath ?? string.Empty;

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

                if (context.SwaggerReference.extra != null)
                {
                    writer.WriteLine(_yamlSerializers.YamlSerializer.Serialize(context.SwaggerReference.extra));
                }

                var bodyParameter = parameters.SingleOrDefault(parameter => parameter.@in == "body");

                if (bodyParameter != null)
                {
                    var bodySchema = Dereference(bodyParameter.schema, context);

                    writer.WriteLine("body:");
                    foreach (var property in bodySchema.properties)
                    {
                        var propertySchema = Dereference(property.Value, context);

                        writer.WriteLine($"{{{{# if request.body.{property.Key} }}}}");
                        writer.WriteLine($"  {property.Key}: {{{{{{ json request.body.{property.Key} }}}}}}");
                        if (propertySchema.@default != null)
                        {
                            writer.WriteLine("{{ else }}");
                            writer.WriteLine($"  {property.Key}: {_yamlSerializers.JsonSerializer.Serialize(propertySchema.@default)}");
                        }

                        writer.WriteLine($"{{{{/ if  }}}}");
                    }
                }

                context.GeneratedContent = writer.GetStringBuilder().ToString();
            }
        }

        private Schema Dereference(Schema schema, GenerateSingleRequestDefinitionContext context)
        {
            return context.SwaggarDocumentLoader.GetResolved(schema);
        }

        private string GetParameterExpression(Parameter parameter, GenerateSingleRequestDefinitionContext context)
        {
            var parameterDefault = parameter.@default;
            if (parameterDefault == null && string.Equals(parameter.name, "api-version", StringComparison.Ordinal))
            {
                parameterDefault = context?.SwaggerDocument?.info?.version;
            }

            if (parameterDefault != null)
            {
                return $"{{{{# if request.parameters.{parameter.name} }}}}{{{{ request.parameters.{parameter.name} }}}}{{{{ else }}}}{parameterDefault}{{{{/ if }}}}";
            }
            else
            {
                return $"{{{{ request.parameters.{parameter.name} }}}}";
            }
        }

        private List<Models.Parameter> GetParameterList(GenerateSingleRequestDefinitionContext context)
        {
            Parameter LookupParameterRef(Parameter parameter)
            {
                return context.SwaggarDocumentLoader.GetResolved(parameter);
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
