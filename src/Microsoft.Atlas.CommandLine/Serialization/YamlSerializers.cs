// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Atlas.CommandLine.Commands;
using Microsoft.Atlas.CommandLine.Execution;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Serialization
{
    public class YamlSerializers : IYamlSerializers
    {
        public YamlSerializers()
        {
            YamlDeserializer = new DeserializerBuilder()
                .WithNodeTypeResolver(new NonStringScalarTypeResolver())
                .WithTagMapping("tag:yaml.org,2002:binary", typeof(byte[]))
                .WithTypeConverter(new ByteArrayConverter())
                .WithTypeInspector<AnyMembersTypeInspector>(AnyMembersTypeInspector.Create)
                .Build();

            YamlSerializer = new SerializerBuilder()
                .DisableAliases()
                .WithAttributeOverride<Exception>(e => e.TargetSite, new YamlIgnoreAttribute())
                .WithAttributeOverride<Exception>(e => e.Message, new YamlMemberAttribute { Alias = "message" })
                .WithEventEmitter(DoubleQuoteAmbiguousStringScalarEmitter.Factory)
                .WithTypeConverter(new ByteArrayConverter())
                .Build();

            JsonSerializer = new SerializerBuilder()
                .DisableAliases()
                .JsonCompatible()
                .WithTypeConverter(new ByteArrayConverter())
                .Build();

            ValueSerialier = new SerializerBuilder()
                .DisableAliases()
                .JsonCompatible()
                .WithTypeConverter(new ByteArrayConverter())
                .BuildValueSerializer();

            JTokenTranserializer = JTokenTranserializerImpl;
        }

        public IDeserializer YamlDeserializer { get; }

        public ISerializer YamlSerializer { get; }

        public ISerializer JsonSerializer { get; }

        public IValueSerializer ValueSerialier { get; }

        public Func<object, JToken> JTokenTranserializer { get; }

        public JToken JTokenTranserializerImpl(object source)
        {
            var jtokenEmitter = new JTokenEmitter();
            ValueSerialier.SerializeValue(jtokenEmitter, source, source?.GetType() ?? typeof(object));
            return jtokenEmitter.Root;
        }
    }
}
