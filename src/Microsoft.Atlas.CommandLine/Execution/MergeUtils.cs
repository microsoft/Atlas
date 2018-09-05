// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public static class MergeUtils
    {
        public static object Merge(object values1, object values2)
        {
            var values1Properties = values1 as IDictionary<object, object>;
            var values2Properties = values2 as IDictionary<object, object>;

            if (values1Properties != null && values2Properties != null)
            {
                var result = new Dictionary<object, object>();
                foreach (var key in values2Properties.Keys.Concat(values1Properties.Keys.Except(values2Properties.Keys)))
                {
                    if (values1Properties.TryGetValue(key, out var childValue1))
                    {
                        if (values2Properties.TryGetValue(key, out var childValue2))
                        {
                            result.Add(key, Merge(childValue1, childValue2));
                        }
                        else
                        {
                            result.Add(key, childValue1);
                        }
                    }
                    else
                    {
                        if (values2Properties.TryGetValue(key, out var childValue2))
                        {
                            result.Add(key, childValue2);
                        }
                        else
                        {
                            // NOTE: hitting this line is technically impossible...
                            result.Add(key, null);
                        }
                    }
                }

                return result;
            }

            return values1;
        }
    }
}
