using System.Collections.Generic;
using Microsoft.Atlas.CommandLine.Blueprints.Models;
using Microsoft.Atlas.CommandLine.Swagger.Models;

namespace Microsoft.Atlas.CommandLine.Swagger
{
    public class GenerateSingleRequestDefinitionContext
    {
        public SwaggerBlueprintInfo BlueprintInfo { get; set; }

        public string TargetPrefix => BlueprintInfo.target;

        public SwaggerDocument SwaggerDocument { get; set; }

        public KeyValuePair<string, PathItem> Path { get; set; }

        public KeyValuePair<string, Operation> Operation { get; set; }

        public string GeneratedPath { get; set; }

        public string GeneratedContent { get; set; }
        public SwaggarDocumentManager SwaggerManager { get; internal set; }
    }
}
