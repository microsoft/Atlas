// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Serialization
{
    public class NonStringScalarTypeResolver : INodeTypeResolver
    {
        bool INodeTypeResolver.Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            // TODO: should use the correct boolean parser (which accepts yes/no) instead of bool.tryparse
            if (currentType == typeof(object) && nodeEvent is Scalar)
            {
                var scalar = nodeEvent as Scalar;
                if (scalar.IsPlainImplicit)
                {
                    if (bool.TryParse(scalar.Value, out var boolValue))
                    {
                        currentType = typeof(bool);
                        return true;
                    }

                    if (int.TryParse(scalar.Value, out var intValue))
                    {
                        currentType = typeof(int);
                        return true;
                    }

                    if (double.TryParse(scalar.Value, out var doubleValue))
                    {
                        currentType = typeof(double);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
