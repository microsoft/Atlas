// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using DevLab.JmesPath;
using DevLab.JmesPath.Functions;
using HandlebarsDotNet;
using HandlebarsDotNet.Compiler;
using Microsoft.Atlas.CommandLine.Templates.FileSystems;
using Microsoft.Atlas.CommandLine.Templates.TextWriters;

namespace Microsoft.Atlas.CommandLine.Templates
{
    public partial class TemplateEngine : ITemplateEngine
    {
        private readonly IHandlebars _compiler;

        public TemplateEngine(TemplateEngineServices services, TemplateEngineOptions options)
        {
            Services = services;
            Options = options;
            var config = new HandlebarsConfiguration()
            {
                FileSystem = new ProbingFileSystem(options.FileSystem),
                Helpers =
                {
                    { "query", QueryHelper },
                    { "json", JsonHelper },
                    { "guid", GuidHelper },
                    { "datetime", DateTimeHelper },
                    { "secret", SecretHelper },
                },
                BlockHelpers =
                {
                    { "query", QueryBlockHelper },
                    { "indent", IndentBlockHelper },
                },
                TextEncoder = new SimpleEncoder(),
            };
            foreach (var templateHelperProvider in services.TemplateHelperProviders)
            {
                foreach (var helper in templateHelperProvider.GetHelpers())
                {
                    config.Helpers[helper.Key] = helper.Value;
                }

                foreach (var helper in templateHelperProvider.GetBlockHelpers())
                {
                    config.BlockHelpers[helper.Key] = helper.Value;
                }
            }

            _compiler = Handlebars.Create(config);
        }

        public TemplateEngineServices Services { get; }

        public TemplateEngineOptions Options { get; }

        public Action<TextWriter, object> LoadTemplate(string templateFile)
        {
            var template = _compiler.CompileView(templateFile);
            return (writer, model) => writer.Write(template(model));
        }

        public object LoadValues(string fileName)
        {
            using (var reader = File.OpenText(fileName))
            {
                return Services.Serializers.YamlDeserializer.Deserialize(reader);
            }
        }

        public void Render(string templateFile, object values, TextWriter writer)
        {
            var template = LoadTemplate(templateFile);
            template.Invoke(writer, values);
        }

        public TModel Render<TModel>(string templateFile, object values)
        {
            using (var writer = new StringWriter())
            {
                Render(templateFile, values, writer);
                using (var reader = new StringReader(writer.ToString()))
                {
                    return Services.Serializers.YamlDeserializer.Deserialize<TModel>(reader);
                }
            }
        }

        private void SecretHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1)
            {
                throw new HandlebarsException("{{secret}} helper must have exactly one argument");
            }

            var secret = Convert.ToString(arguments[0]);
            Services.SecretTracker.AddSecret(secret);
            output.Write(secret);
        }

        private void IndentBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            var indent = int.Parse(arguments.Single().ToString());
            using (var writer = new IndentingTextWriter(output, new string(' ', indent)))
            {
                options.Template(writer, context);
            }
        }

        private void DateTimeHelper(TextWriter output, dynamic context, object[] arguments)
        {
            var dateTime = DateTimeOffset.UtcNow;
            var options = arguments.Last() as IDictionary<string, object>;
            string format = "o";
            if (options != null)
            {
                if (options.TryGetValue("add", out var add))
                {
                    dateTime = dateTime.Add(XmlConvert.ToTimeSpan(add?.ToString()));
                }

                if (options.TryGetValue("format", out var formatOption))
                {
                    format = formatOption?.ToString() ?? "o";
                }
            }

            output.Write(dateTime.ToString(format));
        }

        private void GuidHelper(TextWriter output, dynamic context, object[] arguments)
        {
            var options = arguments.Last() as IDictionary<string, object>;
            if (options != null)
            {
                if (options.TryGetValue("provider", out var provider) && string.Equals(provider?.ToString(), "RNGCryptoServiceProvider", StringComparison.OrdinalIgnoreCase))
                {
                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        var bytes = new byte[16];
                        rng.GetBytes(bytes);
                        bytes[8] = (byte)(bytes[8] & 0xBF | 0x80);
                        bytes[7] = (byte)(bytes[7] & 0x4F | 0x40);
                        output.Write(new Guid(bytes).ToString());
                        return;
                    }
                }

                throw new ApplicationException("Unknown options");
            }

            var guidHelperNamespaceId = Guid.Parse("bb28bf8b-ca8e-4a27-b55b-b17aaebeafc5");
            var name = arguments.Aggregate(string.Empty, (agg, arg) => $"{agg}{arg}");
            output.Write(Create(guidHelperNamespaceId, name).ToString());
        }

        private Guid Create(Guid namespaceId, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
            // ASSUME: UTF-8 encoding is always appropriate
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);

            // convert the namespace UUID to network order (step 3)
            byte[] namespaceBytes = namespaceId.ToByteArray();
            SwapByteOrder(namespaceBytes);

            // comput the hash of the name space ID concatenated with the name (step 4)
            byte[] hash;
            using (HashAlgorithm algorithm = SHA1.Create())
            {
                algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
                algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
                hash = algorithm.Hash;
            }

            // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
            byte[] newGuid = new byte[16];
            Array.Copy(hash, 0, newGuid, 0, 16);

            // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
            newGuid[6] = (byte)((newGuid[6] & 0x0F) | (5 << 4));

            // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
            newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

            // convert the resulting UUID to local byte order (step 13)
            SwapByteOrder(newGuid);
            return new Guid(newGuid);
        }

        private void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        private void SwapBytes(byte[] guid, int left, int right)
        {
            byte temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
        }

        private void JsonHelper(TextWriter output, object context, object[] arguments)
        {
            if (arguments.Length == 0)
            {
                output.Write(ToJsonString(context));
            }
            else
            {
                var json = arguments.FirstOrDefault(argument => argument != null && argument.GetType().Name != "UndefinedBindingResult");
                output.Write(ToJsonString(json));
            }
        }

        private void QueryHelper(TextWriter output, object context, object[] arguments)
        {
            var transformObject = QueryCore(context, arguments);
            JsonHelper(output, context, new object[1] { transformObject });
        }

        private void QueryBlockHelper(TextWriter output, HelperOptions options, object context, object[] arguments)
        {
            var transformObject = QueryCore(context, arguments);

            var items = transformObject as IEnumerable<object>;
            if (items != null)
            {
                if (items.Any())
                {
                    foreach (var item in items)
                    {
                        options.Template(output, item);
                    }
                }
                else
                {
                    options.Inverse(output, context);
                }
            }
            else
            {
                if (transformObject != null)
                {
                    options.Template(output, transformObject);
                }
                else
                {
                    options.Inverse(output, context);
                }
            }
        }

        private object QueryCore(dynamic context, object[] arguments)
        {
            var hashParameters = arguments.OfType<HashParameterDictionary>();

            var args = arguments.Except(hashParameters);

            var formatString = args.Take(1).SingleOrDefault()?.ToString() ?? "@";
            var formatArguments = args.Skip(1).Select(ToJsonString).ToArray();

            formatString = formatArguments.Select((_, index) => index).Aggregate(formatString.Replace("{", "{{").Replace("}", "}}"), (acc, index) => acc.Replace($"${index + 1}", $"{{{index}}}"));

            var query = string.Format(formatString, args: formatArguments.ToArray());
            var json = arguments.OfType<HashParameterDictionary>().SingleOrDefault() ?? (object)context;

            var jmespath = new JmesPath();
            jmespath.FunctionRepository
                .Register<ItemsFunction>()
                .Register<ToObjectFunction>()
                .Register<ZipFunction>();

            var token = Services.Serializers.JTokenTranserializer(json);
            var transformOutput = jmespath.Transform(token, query);
            var transformObject = Services.Serializers.YamlDeserializer.Deserialize<object>(transformOutput.ToString());

            return transformObject;
        }

        private string ToJsonString(object jsonObject)
        {
            if (jsonObject == null)
            {
                return "null";
            }

            if (jsonObject is int jsonInt)
            {
                return jsonInt.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (jsonObject is bool jsonBool)
            {
                return jsonBool ? "true" : "false";
            }

            return Services.Serializers.JsonSerializer.Serialize(jsonObject)?.TrimEnd('\r', '\n');
        }

        private class SimpleEncoder : ITextEncoder
        {
            public string Encode(string value)
            {
                return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
            }
        }
    }
}
