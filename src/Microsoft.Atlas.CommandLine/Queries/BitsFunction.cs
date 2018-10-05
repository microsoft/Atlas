// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Atlas.CommandLine.Queries
{
    public class BitsFunction : JmesPathFunction
    {
        public BitsFunction()
            : base("bits", 1, true)
        {
        }

        public override void Validate(params JmesPathFunctionArgument[] args)
        {
            base.Validate();
            EnsureNumbers(args);
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            ulong bits = 0;
            foreach (var argument in args)
            {
                bits |= argument.Token.Value<ulong>();
            }

            var result = new JArray();
            for (var bit = 0; bit < 64; ++bit)
            {
                var mask = 1ul << bit;
                if ((bits & mask) == mask)
                {
                    result.Add(new JValue(mask));
                }
            }

            return result;
        }
    }
}
