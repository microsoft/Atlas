using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Swagger.Models
{
    public class Swagger
    {
        public string swagger { get; set; }
        public Info info { get; set; }
        public string host { get; set; }
        public string basePath { get; set; }
        public List<string> schemes { get; set; }
        public List<string> consumes { get; set; }
        public List<string> produces { get; set; }
        public Paths paths { get; set; }
        public Definitions definitions { get; set; }
        public Dictionary<string, Parameter> parameters { get; set; }
        public Responses responses { get; set; }
        public SecurityDefinitions securityDefinitions { get; set; }
        public List<SecurityRequirement> security { get; set; }
        public List<Tag> tags { get; set; }
        public ExternalDocumentation externalDocs { get; set; }
    }

    public class Info : Dictionary<object, object>
    {
    }

    public class Paths : Dictionary<string, PathItem>
    {
    }

    public class PathItem
    {
        public Operation get { get; set; }
        public Operation put { get; set; }
        public Operation post { get; set; }
        public Operation delete { get; set; }
        public Operation options { get; set; }
        public Operation head { get; set; }
        public Operation patch { get; set; }
        public List<Parameter> parameters { get; set; } = new List<Parameter>();

        public IEnumerable<KeyValuePair<string, Operation>> operations
        {
            get
            {
                if (get != null)
                {
                    yield return KeyValuePair.Create("get", get);
                }
                if (put != null)
                {
                    yield return KeyValuePair.Create("put", put);
                }
                if (post != null)
                {
                    yield return KeyValuePair.Create("post", post);
                }
                if (delete != null)
                {
                    yield return KeyValuePair.Create("delete", delete);
                }
                if (options != null)
                {
                    yield return KeyValuePair.Create("options", options);
                }
                if (head != null)
                {
                    yield return KeyValuePair.Create("head", head);
                }
                if (patch != null)
                {
                    yield return KeyValuePair.Create("patch", patch);
                }
            }
        }
    }

    public class Operation
    {
        public List<string> tags { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public ExternalDocumentation externalDocs { get; set; }
        public string operationId { get; set; }
        public List<string> consumes { get; set; }
        public List<string> produces { get; set; }
        public List<Parameter> parameters { get; set; } = new List<Parameter>();
        public Responses responses { get; set; }
        public List<string> schemes { get; set; }
        public bool deprecated { get; set; }
        public List<SecurityRequirement> security { get; set; }

        [YamlAnyMembers]
        public Dictionary<string, object> vendorExtensions { get; set; } = new Dictionary<string, object>();
    }

    public class Parameter : Reference
    {
        public string name { get; set; }
        public string @in { get; set; }
        public string description { get; set; }
        public bool required { get; set; }

        public Dictionary<object, object> schema { get; set; }

        public object type { get; set; }
        public object format { get; set; }
        public object allowEmptyValue { get; set; }
        public object items { get; set; }
        public object collectionFormat { get; set; }
        public object @default { get; set; }
        public double maximum { get; set; }
        public bool exclusiveMaximum { get; set; }
        public double minimum { get; set; }
        public bool exclusiveMinimum { get; set; }
        public int maxLength { get; set; }
        public object minLength { get; set; }
        public object pattern { get; set; }
        public object maxItems { get; set; }
        public object minItems { get; set; }
        public object uniqueItems { get; set; }
        public object @enum { get; set; }
        public object multipleOf { get; set; }

        [YamlAnyMembers]
        public Dictionary<string, object> vendorExtensions { get; set; } = new Dictionary<string, object>();

    }

    public class Reference
    {
        [YamlMember(Alias = "$ref")]
        public string @ref { get; set; }
    }

    public class Definitions : Dictionary<object, object>
    {
    }

    public class Parameters : Dictionary<object, object>
    {
    }

    public class Responses : Dictionary<object, object>
    {
    }

    public class SecurityDefinitions : Dictionary<object, object>
    {
    }

    public class SecurityRequirement : Dictionary<object, object>
    {
    }

    public class Tag : Dictionary<object, object>
    {
    }

    public class ExternalDocumentation : Dictionary<object, object>
    {
    }

}
