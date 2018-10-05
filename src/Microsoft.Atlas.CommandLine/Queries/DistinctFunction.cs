// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Atlas.CommandLine.Queries
{
    public class DistinctFunction : JmesPathFunction
    {
        public DistinctFunction()
            : base("distinct", 1)
        {
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var array = (JArray)args[0].Token;

            var distinct = array.Children().Distinct(JToken.EqualityComparer);

            return new JArray(distinct.Cast<object>().ToArray());
        }
    }
}
