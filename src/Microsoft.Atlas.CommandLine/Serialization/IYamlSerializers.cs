// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Serialization
{
    public interface IYamlSerializers
    {
        Deserializer YamlDeserializer { get; }

        Serializer YamlSerializer { get; }

        Serializer JsonSerializer { get; }

        IValueSerializer ValueSerialier { get; }

        Func<object, JToken> JTokenTranserializer { get; }
    }
}
