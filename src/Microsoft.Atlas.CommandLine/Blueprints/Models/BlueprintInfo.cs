using System.Collections.Generic;

namespace Microsoft.Atlas.CommandLine.Blueprints.Models
{
    public class BlueprintInfo
    {
        public string license { get; set; }

        public List<SwaggerBlueprintInfo> swagger { get; set; } = new List<SwaggerBlueprintInfo>();
    }

    public class SwaggerBlueprintInfo
    {
        public string target { get; set; }
        public string source { get; set; }
        public List<string> inputs { get; set; } = new List<string>();
        public object extra { get; set; }
    }
}
