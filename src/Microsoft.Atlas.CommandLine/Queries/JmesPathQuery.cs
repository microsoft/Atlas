// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
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
                .Register<ZipFunction>()
                .Register<DistinctByFunction>()
                .Register<DistinctFunction>()
                .Register<Base64DecodeFunction>()
                .Register<Base64EncodeFunction>()
                .Register<BitsFunction>()
                .Register<ConcatFunction>()
                .Register("search", new SearchFunction(this));
            _serializers = serializers;
        }

        public object Search(string expression, object json)
        {
            try
            {
                var jsonInput = _serializers.JsonSerializer.Serialize(json);
                var jsonOutput = _jmespath.Transform(jsonInput, expression);
                var result = _serializers.YamlDeserializer.Deserialize<object>(jsonOutput);
                return result;
            }
            catch (Exception ex)
            {
                throw new QueryException(ex.Message + Environment.NewLine + expression, ex) { Expression = expression };
            }
        }

        public string SearchJsonText(string expression, string jsonText)
        {
            try
            {
                return _jmespath.Transform(jsonText, expression);
            }
            catch (Exception ex)
            {
                throw new QueryException(ex.Message + Environment.NewLine + expression) { Expression = expression };
            }
        }
    }
}
