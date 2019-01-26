// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Atlas.CommandLine.Queries
{
    public class Base64DecodeFunction : JmesPathFunction
    {
        public Base64DecodeFunction()
            : base("base64_decode", 1)
        {
        }

        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();
            EnsureArrayOrString(args[0]);
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var encoded = args[0].Token.Value<string>();
            var decoded = Convert.FromBase64String(encoded);
            var text = Encoding.UTF8.GetString(decoded);
            return new JValue(text);
        }
    }
}
