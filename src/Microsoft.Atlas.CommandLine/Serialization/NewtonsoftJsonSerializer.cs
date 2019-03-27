// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Newtonsoft.Json;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Serialization
{
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private JsonSerializer _serializer = new JsonSerializer();

        public void Serialize(TextWriter writer, object graph)
        {
            _serializer.Serialize(writer, graph);
        }

        public string Serialize(object graph)
        {
            using (var writer = new StringWriter())
            {
                _serializer.Serialize(writer, graph);
                return writer.ToString();
            }
        }

        public void Serialize(TextWriter writer, object graph, Type type)
        {
            _serializer.Serialize(writer, graph, type);
        }

        public void Serialize(IEmitter emitter, object graph)
        {
            throw new NotImplementedException();
        }

        public void Serialize(IEmitter emitter, object graph, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
