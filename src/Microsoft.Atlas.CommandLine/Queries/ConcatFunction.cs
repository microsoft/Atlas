// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Text;
using DevLab.JmesPath.Functions;
using DevLab.JmesPath.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Atlas.CommandLine.Queries
{
    public class ConcatFunction : JmesPathFunction
    {
        public ConcatFunction()
            : base("concat", 0, true)
        {
        }

        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();

            var types = new[] { "string" };
            foreach (var arg in args)
            {
                var item = arg.Token;
                if (!types.Contains(item.GetTokenType()))
                {
                    var valid = FormatAllowedDataTypes(types);
                    throw new Exception($"Error: invalid-type, function {Name} expects an array of {valid}.");
                }
            }
        }

        private static string FormatAllowedDataTypes(string[] types)
        {
            if (types.Length == 1)
            {
                return types[0];
            }

            var valid = new StringBuilder();
            valid.AppendFormat("{0} ", types[0]);
            for (var index = 1; index < types.Length - 1; index++)
            {
                valid.AppendFormat(", {0} ", types[index]);
            }

            valid.AppendFormat("or {0}", types[types.Length - 1]);

            return valid.ToString();
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var strings = args.Select(arg => arg.Token.Value<string>());

            var result = string.Concat(strings);

            return new JValue(result);
        }
    }
}
