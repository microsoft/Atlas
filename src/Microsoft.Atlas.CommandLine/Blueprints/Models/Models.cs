// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable SA1516 // Elements must be separated by blank line
#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class

namespace Microsoft.Atlas.CommandLine.Blueprints.Models
{
    public class WorkflowInfoDocument
    {
        public Swagger.Models.Info info { get; set; }

        public Dictionary<string, SwaggerReference> swagger { get; set; } = new Dictionary<string, SwaggerReference>();

        public Dictionary<string, DependencyReference> workflows { get; set; } = new Dictionary<string, DependencyReference>();
    }

    public class SwaggerReference
    {
        public string target { get; set; }
        public string source { get; set; }
        public List<string> inputs { get; set; } = new List<string>();
        public object extra { get; set; }
    }

    public class DependencyReference
    {
        public string target { get; set; }
        public string source { get; set; }
        public List<string> inputs { get; set; } = new List<string>();
    }
}
