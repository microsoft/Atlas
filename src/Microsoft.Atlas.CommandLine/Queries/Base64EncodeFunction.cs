// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Atlas.CommandLine.Queries
{
    public class Base64EncodeFunction : JmesPathFunction
    {
        public Base64EncodeFunction()
            : base("base64_encode", 1)
        {
        }

        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();
            EnsureArrayOrString(args[0]);
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var text = args[0].Token.Value<string>();
            var decoded = Encoding.UTF8.GetBytes(text);
            var encoded = Convert.ToBase64String(decoded);
            return new JValue(encoded);
        }
    }
}
