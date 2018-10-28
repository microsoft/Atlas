// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.OAuth2;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Atlas.CommandLine.Swagger.Models;

namespace Microsoft.Atlas.CommandLine.Swagger
{
    public class SwaggarDocumentLoader : ISwaggarDocumentLoader
    {
        private readonly IYamlSerializers _yamlSerializers;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, SwaggerDocumentEntry> _loaded = new Dictionary<string, SwaggerDocumentEntry>();

        public SwaggarDocumentLoader(IYamlSerializers yamlSerializers, IHttpClientFactory httpClientFactory)
        {
            _yamlSerializers = yamlSerializers;
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.Create(null);
        }

        public async Task<SwaggerDocument> LoadDocument(string basePath, string relativePath)
        {
            return (await LoadEntry(basePath, relativePath)).SwaggerDocument;
        }

        public TObject GetResolved<TObject>(TObject refObject)
            where TObject : Reference
        {
            foreach (var loaded in _loaded)
            {
                if (loaded.Value.References.TryGetValue(refObject, out var value))
                {
                    return (TObject)value;
                }
            }

            return refObject;
        }

        private async Task<SwaggerDocumentEntry> LoadEntry(string basePath, string relativePath)
        {
            var effectivePath = CombinePaths(basePath, relativePath);
            if (_loaded.TryGetValue(effectivePath, out var value))
            {
                return value;
            }

            var swaggerText = await _httpClient.GetStringAsync(effectivePath);
            var swaggerDocument = _yamlSerializers.YamlDeserializer.Deserialize<SwaggerDocument>(swaggerText);
            var entry = new SwaggerDocumentEntry
            {
                SwaggerDocument = swaggerDocument
            };

            foreach (var definition in swaggerDocument.definitions)
            {
                entry.Targets.Add($"#/definitions/{definition.Key}", definition.Value);
            }

            foreach (var parameter in swaggerDocument.parameters)
            {
                entry.Targets.Add($"#/parameters/{parameter.Key}", parameter.Value);
            }

            _loaded.Add(effectivePath, entry);

            var p1 = swaggerDocument.parameters.Select(kv => kv.Value);
            var p2 = swaggerDocument.paths.Values.SelectMany(x => x.parameters);
            var p3 = swaggerDocument.paths.Values.SelectMany(x => x.operations).SelectMany(x => x.Value.parameters);
            var parameters = p1.Concat(p2).Concat(p3);

            foreach (var parameter in parameters.Where(x => x.@ref != null))
            {
                var refString = parameter.@ref;
                var hashIndex = refString.IndexOf('#');
                var refRelative = refString.Substring(0, hashIndex);
                var fragment = refString.Substring(hashIndex);

                var targetEntry = await LoadEntry(effectivePath, refRelative);
                entry.References[parameter] = targetEntry.Targets[fragment];
            }

            // TODO: this probablyl isn't good enough - will need to recurse schema.properties to any depth looking for refs?
            var s1 = swaggerDocument.definitions.Where(kv => kv.Value.properties != null).SelectMany(kv => kv.Value.properties.Values);
            var s2 = parameters.Where(x => x.schema != null).Select(x => x.schema);
            var schemas = s1.Concat(s2);

            foreach (var schema in schemas.Where(x => x.@ref != null))
            {
                var refString = schema.@ref;
                var hashIndex = refString.IndexOf('#');
                var refRelative = refString.Substring(0, hashIndex);
                var fragment = refString.Substring(hashIndex);

                var targetEntry = await LoadEntry(effectivePath, refRelative);
                entry.References[schema] = targetEntry.Targets[fragment];
            }

            return entry;
        }

        private string CombinePaths(string basePath, string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return basePath;
            }

            for (; ; )
            {
                if (relativePath.StartsWith("../"))
                {
                    basePath = RemoveSlash(basePath);
                    relativePath = relativePath.Substring("../".Length);
                }
                else if (relativePath.StartsWith("./"))
                {
                    relativePath = relativePath.Substring("./".Length);
                }
                else
                {
                    break;
                }
            }

            return RemoveSlash(basePath) + "/" + relativePath;
        }

        private string RemoveSlash(string path)
        {
            var lastSlash = path.LastIndexOf('/');
            return path.Substring(0, lastSlash);
        }

        public class SwaggerDocumentEntry
        {
            public SwaggerDocument SwaggerDocument { get; set; }

            public Dictionary<string, Reference> Targets { get; } = new Dictionary<string, Reference>();

            public Dictionary<Reference, Reference> References { get; } = new Dictionary<Reference, Reference>();
        }
    }
}
