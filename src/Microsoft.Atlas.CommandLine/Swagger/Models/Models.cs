// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using YamlDotNet.Serialization;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable SA1516 // Elements must be separated by blank line
#pragma warning disable SA1649 // File name must match first type name
#pragma warning disable SA1402 // File may only contain a single class

namespace Microsoft.Atlas.CommandLine.Swagger.Models
{
    /// <summary>
    /// https://github.com/OAI/OpenAPI-Specification/blob/master/versions/2.0.md
    /// </summary>
    public class SwaggerDocument : VendorExtensions
    {
        public string swagger { get; set; }
        public Info info { get; set; }
        public string host { get; set; }
        public string basePath { get; set; }
        public List<string> schemes { get; set; }
        public List<string> consumes { get; set; }
        public List<string> produces { get; set; }
        public Paths paths { get; set; }
        public Dictionary<string, Schema> definitions { get; set; } = new Dictionary<string, Schema>();
        public Dictionary<string, Parameter> parameters { get; set; } = new Dictionary<string, Parameter>();
        public Responses responses { get; set; }
        public SecurityDefinitions securityDefinitions { get; set; }
        public List<SecurityRequirement> security { get; set; }
        public List<Tag> tags { get; set; }
        public ExternalDocumentation externalDocs { get; set; }
    }

    public abstract class VendorExtensions
    {
        [YamlAnyMembers]
        public Dictionary<string, object> vendorExtensions { get; set; } = new Dictionary<string, object>();
    }

    public class Reference : VendorExtensions
    {
        [YamlMember(Alias = "$ref")]
        public string @ref { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(@ref))
            {
                return @ref;
            }

            return base.ToString();
        }
    }

    /// <summary>
    /// The object provides metadata about the API. The metadata can be used by the clients if needed, and can be presented in the Swagger-UI for convenience.
    /// </summary>
    public class Info : VendorExtensions
    {
        /// <summary>
        /// Required. The title of the application.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// A short description of the application.GFM syntax can be used for rich text representation.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// The Terms of Service for the API.
        /// </summary>
        public string termsOfService { get; set; }

        /// <summary>
        /// The contact information for the exposed API.
        /// </summary>
        public Contact contact { get; set; }

        /// <summary>
        /// The license information for the exposed API.
        /// </summary>
        public License license { get; set; }

        /// <summary>
        /// Required. Provides the version of the application API (not to be confused with the specification version).
        /// </summary>
        public string version { get; set; }
    }

    /// <summary>
    /// Contact information for the exposed API.
    /// </summary>
    public class Contact : VendorExtensions
    {
        /// <summary>
        /// The identifying name of the contact person/organization.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The URL pointing to the contact information. MUST be in the format of a URL.
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// The email address of the contact person/organization.MUST be in the format of an email address.
        /// </summary>
        public string email { get; set; }
    }

    /// <summary>
    /// License information for the exposed API.
    /// </summary>
    public class License : VendorExtensions
    {
        /// <summary>
        /// Required. The license name used for the API.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// A URL to the license used for the API. MUST be in the format of a URL.
        /// </summary>
        public string url { get; set; }
    }

    /// <summary>
    /// Holds the relative paths to the individual endpoints. The path is appended to the basePath in order to construct the full URL. The Paths may be empty, due to ACL constraints.
    ///
    /// A relative path to an individual endpoint. The field name MUST begin with a slash. The path is appended to the basePath in order to construct the full URL. Path templating is allowed.
    /// </summary>
    public class Paths : Dictionary<string, PathItem>
    {
    }

    /// <summary>
    /// Describes the operations available on a single path. A Path Item may be empty, due to ACL constraints.
    /// The path itself is still exposed to the documentation viewer but they will not know which operations and parameters are available.
    /// </summary>
    public class PathItem : VendorExtensions
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

    public class Operation : VendorExtensions
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
    }

    public class Parameter : Reference
    {
        public string name { get; set; }
        public string @in { get; set; }
        public string description { get; set; }
        public bool required { get; set; }

        public Schema schema { get; set; }

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
    }

    public class Schema : Reference
    {
        public object format { get; set; }
        public object title { get; set; }
        public object description { get; set; }
        public object @default { get; set; }
        public object multipleOf { get; set; }
        public object maximum { get; set; }
        public object exclusiveMaximum { get; set; }
        public object minimum { get; set; }
        public object exclusiveMinimum { get; set; }
        public object maxLength { get; set; }
        public object minLength { get; set; }
        public object pattern { get; set; }
        public object maxItems { get; set; }
        public object minItems { get; set; }
        public object uniqueItems { get; set; }
        public object maxProperties { get; set; }
        public object minProperties { get; set; }
        public List<string> required { get; set; }
        public object @enum { get; set; }
        public string type { get; set; }

        public object items { get; set; }
        public List<Schema> allOf { get; set; }
        public Dictionary<string, Schema> properties { get; set; }
        public object additionalProperties { get; set; }

        public string discriminator { get; set; }
        public bool readOnly { get; set; }
        public object xml { get; set; }
        public ExternalDocumentation externalDocs { get; set; }
        public object example { get; set; }
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

    public class ExternalDocumentation : VendorExtensions
    {
        public string description { get; set; }
        public string url { get; set; }
    }
}
