// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Atlas.CommandLine.Tests.Stubs;
using Microsoft.Atlas.CommandLine.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Tests
{
    public class ServiceContextTestsBase<TServiceContext>
        where TServiceContext : ServiceContextBase, new()
    {
        private TServiceContext _services;

        public TServiceContext Services
        {
            get => _services ?? InitializeServices();
            set => _services = value;
        }

        public StubConsole Console { get; set; } = new StubConsole();

        public TServiceContext InitializeServices(params object[] stubServices)
        {
            Services = ServiceContextBase.Create<TServiceContext>(new[] { Console }.Concat(stubServices).ToArray());
            return Services;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _services?.Dispose();
        }

        protected T Yaml<T>(string input) => new DeserializerBuilder()
            .WithTypeConverter(new HttpMethodConverter())
            .Build().Deserialize<T>(input);

        private class HttpMethodConverter : IYamlTypeConverter
        {
            private bool _jsonCompatible = false;

            public bool Accepts(Type type) => type == typeof(HttpMethod);

            public object ReadYaml(IParser parser, Type type)
            {
                var value = ((Scalar)parser.Current).Value;
                parser.MoveNext();
                return new HttpMethod(value);
            }

            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                var method = (HttpMethod)value;
                emitter.Emit(new Scalar(null, null, method.ToString(), _jsonCompatible ? ScalarStyle.DoubleQuoted : ScalarStyle.Any, true, false));
            }
        }
    }
}
