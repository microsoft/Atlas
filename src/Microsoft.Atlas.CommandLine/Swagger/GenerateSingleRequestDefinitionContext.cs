// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Atlas.CommandLine.Blueprints.Models;
using Microsoft.Atlas.CommandLine.Swagger.Models;

namespace Microsoft.Atlas.CommandLine.Swagger
{
    public class GenerateSingleRequestDefinitionContext
    {
        public ISwaggarDocumentLoader SwaggarDocumentLoader { get; set; }

        public SwaggerReference SwaggerReference { get; set; }

        public SwaggerDocument SwaggerDocument { get; set; }

        public KeyValuePair<string, PathItem> Path { get; set; }

        public KeyValuePair<string, Operation> Operation { get; set; }

        public string GeneratedPath { get; set; }

        public string GeneratedContent { get; set; }
    }
}
