// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Atlas.CommandLine.Queries
{
    public class SearchFunction : JmesPathFunction
    {
        private readonly IJmesPathQuery _jmesPathQuery;

        public SearchFunction(IJmesPathQuery jmesPathQuery)
            : base("search", 2)
        {
            _jmesPathQuery = jmesPathQuery;
        }

        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();
            EnsureArrayOrString(args[0]);
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var expr = args[0].Token.Value<string>();
            var jsonInput = args[1].Token;

            var jsonTextInput = JsonConvert.SerializeObject(jsonInput);
            var jsonTextOutput = _jmesPathQuery.SearchJsonText(expr, jsonTextInput);
            var jsonOutput = JToken.Parse(jsonTextOutput);

            return jsonOutput;
        }
    }
}
