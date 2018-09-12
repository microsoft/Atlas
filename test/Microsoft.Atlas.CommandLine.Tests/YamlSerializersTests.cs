// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests
{
    [TestClass]
    public class YamlSerializersTests
    {
        public IYamlSerializers GetSerializers()
        {
            return Program
                .AddServices(new ServiceCollection())
                .BuildServiceProvider()
                .GetRequiredService<IYamlSerializers>();
        }

        public Dictionary<object, object> GetExpectedObject()
        {
            return new Dictionary<object, object>
            {
                { "alpha", "text" },
                { "beta", 42 },
                { "gamma", true },
                { "delta", null },
                {
                    "epsilon", new List<object>
                    {
                        "text", 42, true, null, new Dictionary<object, object>
                        {
                            { "alpha", "text" },
                            { "beta", 42 },
                            { "gamma", true },
                            { "delta", null },
                            { "epsilon", new List<object> { "text", 42, true, null } },
                        }
                    }
                },
                {
                    "zeta", new Dictionary<object, object>
                    {
                        { "alpha", "text" },
                        { "beta", 42 },
                        { "gamma", true },
                        { "delta", null },
                        {
                            "epsilon", new List<object> { "text", 42, true, null }
                        },
                    }
                },
                { "eta", new List<object> { "42", "true", "null" } },
            };
        }

        public string GetJsonRepresentation()
        {
            return @"
{
    ""alpha"": ""text"",
    ""beta"": 42,
    ""gamma"": true,
    ""delta"": null,
    ""epsilon"": [""text"", 42, true, null, {
        ""alpha"": ""text"",
        ""beta"": 42,
        ""gamma"": true,
        ""delta"": null,
        ""epsilon"": [""text"", 42, true, null]
    }],
    ""zeta"": {
        ""alpha"": ""text"",
        ""beta"": 42,
        ""gamma"": true,
        ""delta"": null,
        ""epsilon"": [""text"", 42, true, null]
    },
    ""eta"": [""42"", ""true"", ""null""]
}";
        }

        public string GetYamlRepresentation()
        {
            return @"alpha: text
beta: 42
gamma: true
delta: 
epsilon:
- text
- 42
- true
- 
- alpha: text
  beta: 42
  gamma: true
  delta: 
  epsilon:
  - text
  - 42
  - true
  - 
zeta:
  alpha: text
  beta: 42
  gamma: true
  delta: 
  epsilon:
  - text
  - 42
  - true
  - 
eta:
- ""42""
- ""true""
- ""null""
";
        }

        [TestMethod]
        public void JsonDeserializesToObjects()
        {
            var serializers = GetSerializers();

            var values = serializers.YamlDeserializer.Deserialize<object>(GetJsonRepresentation());

            var expected = GetExpectedObject();

            AssertAreEqual(expected, values);
        }

        [TestMethod]
        public void YamlDeserializesToObjects()
        {
            var serializers = GetSerializers();

            var values = serializers.YamlDeserializer.Deserialize<object>(GetYamlRepresentation());

            var expected = GetExpectedObject();

            AssertAreEqual(expected, values);
        }

        [TestMethod]
        public void ObjectSerializesToJson()
        {
            var services = Program.AddServices(new ServiceCollection()).BuildServiceProvider();

            var serializers = services.GetRequiredService<IYamlSerializers>();

            var data = GetExpectedObject();
            var json = serializers.JsonSerializer.Serialize(data);

            // normalizing out white-space
            var expected = GetJsonRepresentation().Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty);
            var actual = json.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ObjectSerializesToYaml()
        {
            var services = Program.AddServices(new ServiceCollection()).BuildServiceProvider();

            var serializers = services.GetRequiredService<IYamlSerializers>();

            var data = GetExpectedObject();
            var yaml = serializers.YamlSerializer.Serialize(data);

            // yaml serializer writes blank value for null
            var expected = GetYamlRepresentation().Replace("\r", string.Empty);
            var actual = yaml.Replace("\r", string.Empty);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ObjectTranserializesToJToken()
        {
            var services = Program.AddServices(new ServiceCollection()).BuildServiceProvider();

            var serializers = services.GetRequiredService<IYamlSerializers>();

            var data = GetExpectedObject();
            var jtoken = serializers.JTokenTranserializer(data);

            // yaml serializer writes blank value for null
            var expected = GetJsonRepresentation().Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty);
            var actual = jtoken.ToString().Replace("\r", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BinaryCanBeSerialized()
        {
            var services = Program.AddServices(new ServiceCollection()).BuildServiceProvider();

            var serializers = services.GetRequiredService<IYamlSerializers>();

            var data = new Dictionary<object, object>
            {
                { "data", new byte[] { 0, 1, 2, 3, 254, 255 } },
            };

            var yaml = serializers.YamlSerializer.Serialize(data);

            var json = serializers.JsonSerializer.Serialize(data);

            Assert.AreEqual("data: !!binary AAECA/7/", yaml.Replace("\r", string.Empty).Replace("\n", string.Empty));

            Assert.AreEqual(@"{""data"": !!binary AAECA/7/}", json.Replace("\r", string.Empty).Replace("\n", string.Empty));
        }

        private void AssertAreEqual(object expected, object actual)
        {
            Assert.AreSame(expected?.GetType(), actual?.GetType());

            if (expected is IDictionary<object, object>)
            {
                AssertAreEqual(expected as IDictionary<object, object>, actual as IDictionary<object, object>);
            }
            else if (expected is IList<object>)
            {
                AssertAreEqual(expected as IList<object>, actual as IList<object>);
            }
            else
            {
                Assert.AreEqual(expected, actual);
            }
        }

        private void AssertAreEqual(IDictionary<object, object> expected, IDictionary<object, object> actual)
        {
            Assert.AreEqual(expected.Count(), actual.Count(), $"Expected and actual dictionary have different size");
            foreach (var group in expected.Concat(actual).GroupBy(kv => kv.Key))
            {
                Assert.AreEqual(2, group.Count(), $"Expected and actual differ by key {group.Key}");

                AssertAreEqual(group.First().Value, group.Last().Value);
            }
        }

        private void AssertAreEqual(IList<object> expected, IList<object> actual)
        {
            Assert.AreEqual(expected.Count(), actual.Count(), $"Expected and actual list have different size");
            foreach (var pair in expected.Zip(actual, Tuple.Create))
            {
                AssertAreEqual(pair.Item1, pair.Item2);
            }
        }
    }
}
