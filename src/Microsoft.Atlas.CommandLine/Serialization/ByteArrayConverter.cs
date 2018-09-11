// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Serialization
{
    public class ByteArrayConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(byte[]);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            var scalar = (YamlDotNet.Core.Events.Scalar)parser.Current;
            var bytes = Convert.FromBase64String(scalar.Value);
            parser.MoveNext();
            return bytes;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var bytes = (byte[])value;
            emitter.Emit(new YamlDotNet.Core.Events.Scalar(
                null,
                "tag:yaml.org,2002:binary",
                Convert.ToBase64String(bytes),
                ScalarStyle.Plain,
                false,
                false));
        }
    }
}
