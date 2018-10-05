// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Atlas.CommandLine.Queries;
using Microsoft.Atlas.CommandLine.Serialization;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Tests.Queries
{
    public abstract class BaseFunctionTests
    {
        public IJmesPathQuery Query { get; set; } = new JmesPathQuery(new YamlSerializers());

        public T Yaml<T>(string input) => new YamlSerializers().YamlDeserializer.Deserialize<T>(input);

        public IDictionary<string, object> Yaml(string input) => Yaml<IDictionary<string, object>>(input);
    }
}
