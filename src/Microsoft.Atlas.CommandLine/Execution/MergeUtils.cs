// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public static class MergeUtils
    {
        public static object Merge(object overlayValues, object underlayValues)
        {
            var overlayProperties = overlayValues as IDictionary<object, object>;
            var underlayProperties = underlayValues as IDictionary<object, object>;

            if (overlayProperties != null && underlayProperties != null)
            {
                var result = new Dictionary<object, object>();
                foreach (var key in underlayProperties.Keys.Concat(overlayProperties.Keys.Except(underlayProperties.Keys)))
                {
                    if (overlayProperties.TryGetValue(key, out var childValue1))
                    {
                        if (underlayProperties.TryGetValue(key, out var childValue2))
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
                        if (underlayProperties.TryGetValue(key, out var childValue2))
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

            return overlayValues;
        }
    }
}
