// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Atlas.CommandLine.Execution;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Atlas.CommandLine.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable SA1300 // Element must begin with upper-case letter

namespace Microsoft.Atlas.CommandLine.Tests
{
    [TestClass]
    public class ValuesEngineTests : ServiceContextTestsBase<ValuesEngineTests.ServiceContext>
    {
        [TestMethod]
        public void SearchExpressionsAreEvaluated()
        {
            var scenario = Yaml<ValuesScenario>(@"
source:
 x: This is literal
 y: ( z )
context:
 z: This is queried
expected:
 x: This is literal
 y: This is queried
");

            Evaluate(scenario);
        }

        [TestMethod]
        public void PropertyNamesMayBeEvaluated()
        {
            var scenario = Yaml<ValuesScenario>(@"
source:
 one: 1
 (propName): (propValue)
 three: 3
context:
 propName: two
 propValue: 2
expected:
 one: 1
 two: 2
 three: 3
");

            Evaluate(scenario);
        }

        [TestMethod]
        public void ForEachCanOutputSeveralPropertyNames()
        {
            var scenario = Yaml<ForEachOutScenario>(@"
source:
 one: 1
 (propName): (propValue)
 three: 3
contexts:
- propName: two.a
  propValue: 2a
- propName: two.b
  propValue: 2b
expected:
 one: 1
 two.a: 2a
 two.b: 2b
 three: 3
");

            Evaluate(scenario);
        }

        [TestMethod]
        public void ForEachOutputCanMerge()
        {
            var scenario = Yaml<ForEachOutScenario>(@"
source:
 one: 1
 (propName): {pv: (propValue), pa: (propArray), po: (propObject)}
 three: 3
contexts:
- propName: two.a
  propValue: 2a1v
  propArray: [2a1a]
  propObject: {2a1ok: 2a1ov, 2ak: 2a1v}
- propName: two.b
  propValue: 2b1v
  propArray: [2b1a]
  propObject: {2b1ok: 2b1ov}
- propName: two.a
  propValue: 2a2v
  propArray: [2a2a]
  propObject: {2a2ok: 2a2ov, 2ak: 2a2v}
expected:
 one: 1
 two.a:
  pv: 2a2v
  pa: [2a1a, 2a2a]
  po: {2a1ok: 2a1ov, 2ak: 2a2v, 2a2ok: 2a2ov}
 two.b:
  pv: 2b1v
  pa: [2b1a]
  po: {2b1ok: 2b1ov}
 three: 3
");

            Evaluate(scenario);
        }

        private void Evaluate(ValuesScenario data)
        {
            var result = Services.ValuesEngine.ProcessValues(data.source, data.context);

            AssertAreEqual(data.expected, result);
        }

        private void Evaluate(ForEachOutScenario data)
        {
            var result = Services.ValuesEngine.ProcessValuesForeachOut(data.source, data.contexts);

            AssertAreEqual(data.expected, result);
        }

        private void AssertAreEqual(object expected, object result)
        {
            var expectedText = Services.Serializers.JsonSerializer.Serialize(expected);
            var actualText = Services.Serializers.JsonSerializer.Serialize(result);
            Assert.AreEqual(expectedText, actualText);
        }

        public class ServiceContext : ServiceContextBase
        {
            public IValuesEngine ValuesEngine { get; set; }

            public IYamlSerializers Serializers { get; set; }
        }

        public class ValuesScenario
        {
            public object source { get; set; }

            public object context { get; set; }

            public object expected { get; set; }
        }

        public class ForEachOutScenario
        {
            public object source { get; set; }

            public object[] contexts { get; set; }

            public object expected { get; set; }
        }
    }
}
