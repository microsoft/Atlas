// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using DevLab.JmesPath;
using DevLab.JmesPath.Functions;
using Microsoft.Atlas.CommandLine.Commands;
using Microsoft.Atlas.CommandLine.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Atlas.CommandLine.Queries
{
    public class JmesPathQuery : IJmesPathQuery
    {
        private readonly JmesPath _jmespath;
        private readonly IYamlSerializers _serializers;

        public JmesPathQuery(IYamlSerializers serializers)
        {
            _jmespath = new JmesPath();
            _jmespath.FunctionRepository
                .Register<ItemsFunction>()
                .Register<ToObjectFunction>()
                .Register<ZipFunction>();
            _serializers = serializers;
        }

        public object Search(string expression, object json)
        {
            var jtokenEmitter = new JTokenEmitter();
            _serializers.ValueSerialier.SerializeValue(jtokenEmitter, json, json?.GetType() ?? typeof(object));
            var transformOutput = _jmespath.Transform(jtokenEmitter.Root, expression);

            using (var stringWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(stringWriter) { CloseOutput = false })
                {
                    transformOutput.WriteTo(jsonWriter);
                }

                var jsonText = stringWriter.GetStringBuilder().ToString();
                return _serializers.YamlDeserializer.Deserialize<object>(jsonText);
            }
        }
    }
}
