// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using DevLab.JmesPath.Functions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Atlas.CommandLine.Queries
{
    public class DistinctByFunction : ByFunction
    {
        public DistinctByFunction()
            : base("distinct_by")
        {
        }

        public override JToken Execute(params JmesPathFunctionArgument[] args)
        {
            var array = (JArray)args[0].Token;
            var expression = args[1].Expression;

            var comparer = new Comparer(u => expression.Transform(u).AsJToken());

            var distinct = array.Children().Distinct(comparer);

            return new JArray(distinct.Cast<object>().ToArray());
        }

        private class Comparer : IEqualityComparer<JToken>
        {
            private readonly Func<JToken, JToken> _keySelector;

            public Comparer(Func<JToken, JToken> keySelector)
            {
                _keySelector = keySelector;
            }

            public bool Equals(JToken x, JToken y)
            {
                return JToken.EqualityComparer.Equals(_keySelector(x), _keySelector(y));
            }

            public int GetHashCode(JToken obj)
            {
                return JToken.EqualityComparer.GetHashCode(_keySelector(obj));
            }
        }
    }
}
