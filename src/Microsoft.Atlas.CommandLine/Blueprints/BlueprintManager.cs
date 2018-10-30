// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Syntax;
using Microsoft.Atlas.CommandLine.Blueprints.Models;
using Microsoft.Atlas.CommandLine.Serialization;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public class BlueprintManager : IBlueprintManager
    {
        private readonly IEnumerable<IBlueprintPackageProvider> _packageProviders;
        private readonly IEnumerable<IBlueprintDecoratorProvider> _decoratorProviders;
        private readonly IYamlSerializers _yamlSerializers;

        public BlueprintManager(
            IEnumerable<IBlueprintPackageProvider> packageProviders,
            IEnumerable<IBlueprintDecoratorProvider> fileGeneratorProviders,
            IYamlSerializers yamlSerializers)
        {
            _packageProviders = packageProviders;
            _decoratorProviders = fileGeneratorProviders;
            _yamlSerializers = yamlSerializers;
        }

        public async Task<IBlueprintPackage> GetBlueprintPackage(string blueprint)
        {
            var blueprintPackageCore = GetBlueprintPackageCore(blueprint);
            if (blueprintPackageCore == null)
            {
                return null;
            }

            var blueprintInfo = new WorkflowInfoDocument();

            if (blueprintPackageCore.Exists("readme.md"))
            {
                var readmeText = blueprintPackageCore.OpenText("readme.md").ReadToEnd();
                var readmeDoc = Markdig.Markdown.Parse(readmeText);

                var codeBlocks = readmeDoc.OfType<FencedCodeBlock>();
                var yamlBlocks = codeBlocks.Where(cb => string.Equals(cb.Info, "yaml", StringComparison.Ordinal));

                if (yamlBlocks.Any())
                {
                    var yamlLines = yamlBlocks.SelectMany(yaml => yaml.Lines.Lines);
                    var yamlText = yamlLines.Aggregate(string.Empty, (a, b) => $"{a}{b}{Environment.NewLine}");

                    blueprintInfo = _yamlSerializers.YamlDeserializer.Deserialize<WorkflowInfoDocument>(yamlText);
                }
            }

            var blueprintPackage = blueprintPackageCore;

            foreach (var swaggerInfo in blueprintInfo.swagger.Values)
            {
                foreach (var decoratorProvider in _decoratorProviders)
                {
                    blueprintPackage = await decoratorProvider.CreateDecorator(swaggerInfo, blueprintPackage);
                }
            }

            return blueprintPackage;
        }

        private IBlueprintPackage GetBlueprintPackageCore(string blueprint)
        {
            foreach (var provider in _packageProviders)
            {
                var blueprintPackage = provider.TryGetBlueprintPackage(blueprint);
                if (blueprintPackage != null)
                {
                    return blueprintPackage;
                }
            }

            return null;
        }
    }
}
