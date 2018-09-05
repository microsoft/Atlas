// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Commands;
using Microsoft.Atlas.CommandLine.Tests.Stubs;
using Microsoft.Atlas.CommandLine.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests
{
    [TestClass]
    public class WorkflowCommandsTests : ServiceContextTestsBase<WorkflowCommandsTests.ServiceContext>
    {
#pragma warning disable SA1201 // Elements must appear in the correct order
        public class ServiceContext : ServiceContextBase
        {
            public WorkflowCommands WorkflowCommands { get; set; }

            public CommandLineApplicationServices App { get; set; }
        }

        [TestMethod]
        public async Task HelpReturnsAboutText()
        {
            var result = Services.App.Execute("deploy", "--help");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task ExecuteRunsWorkflow()
        {
            var stubs = Yaml<StubBlueprintManager>(@"
Blueprints:
 the-test:
  Files:
   workflow.yaml: |
    operations:
    - message: Hello World
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task MissingBlueprintWillFail()
        {
            var stubs = Yaml<StubBlueprintManager>(@"
Blueprints:
 the-test:
  Files:
   workflow.yaml: |
    operations:
    - message: Hello World
");

            InitializeServices(stubs);

            var ex = Assert.ThrowsException<AggregateException>(() =>
            {
                Services.App.Execute("deploy", "bad-file-name");
            });

            Assert.IsTrue(ex.Message.Contains("bad-file-name"));
        }

        [TestMethod]
        public async Task MissingValuesWillFail()
        {
            var stubs = Yaml<StubBlueprintManager>(@"
Blueprints:
 the-test:
  Files:
   workflow.yaml: |
    operations:
    - message: Hello World
");

            InitializeServices(stubs);

            var ex = Assert.ThrowsException<AggregateException>(() =>
            {
                var result = Services.App.Execute("deploy", "-f", "missing-values.yaml", "the-test");
            });

            Assert.IsTrue(ex.Message.Contains("missing-values.yaml"));
        }

        [TestMethod]
        public async Task MessageCanBeJmesPath()
        {
            var stubs = Yaml<StubBlueprintManager>(@"
Blueprints:
 the-test:
  Files:
   workflow.yaml: |
    operations:
    - message: (length('hello world'))
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task OutputFlowsBackMultipleLevelsWhenNotInterrupted()
        {
            var stubs = Yaml<StubBlueprintManager>(@"
Blueprints:
 the-test:
  Files:
   workflow.yaml: |
    operations:
    - message: (['data is ', @])
    - message: has nested operations
      operations:
      - message: (['data is ', @])
      - message: returning 42
        output: {x: 42}
      - message: (['data is ', @])
    - message: (['data is ', @])
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task DeclaredValuesDoNotBecomeOutput()
        {
            var stubs = Yaml<StubBlueprintManager>(@"
Blueprints:
 the-test:
  Files:
   workflow.yaml: |
    values: {x1: 1}
    operations:
    - message: (['data is ', @])
    - values: {x2: 2}
      operations:
      - message: (['data is ', @])
      - values: {x3: 3}
      - message: (['data is ', @])
    - message: (['data is ', @])
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task ValuesCanBeUsedForOutput()
        {
            var stubs = Yaml<StubBlueprintManager>(@"
Blueprints:
 the-test:
  Files:
   workflow.yaml: |
    operations:
    - message: This has values and output
      values: {x1: 1}
      output: {x2: (x1)}
    - message: This has values, output, and nested operations
      values: {x3: 3}
      operations:
      - message: This is nested
      output: {x4: (x1), x5: (x2), x6: (x3)}
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task JsonResponseCanBeUsedForOutput()
        {
            var stubBlueprintManager = Yaml<StubBlueprintManager>(@"
Blueprints:
 the-test:
  Files:
   workflow.yaml: |
    operations:
    - message: Calling request and producing output
      request: request.yaml
      output: 
       alpha: (beta.gamma)
   request.yaml: |
    method: GET
    url: https://localhost/testing
");

            var stubJsonHttpClientFactory = Yaml<StubJsonHttpClientFactory>(@"
Responses:
 https://localhost/testing:
  GET:
   status: 200
   body:
    beta:
     gamma: delta
");

            InitializeServices(stubBlueprintManager, stubJsonHttpClientFactory);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task JsonResponseIsNotOutputByDefault()
        {
            var stubBlueprintManager = Yaml<StubBlueprintManager>(@"
Blueprints:
 the-test:
  Files:
   workflow.yaml: |
    operations:
    - message: Calling request and not producing output
      request: request.yaml
   request.yaml: |
    method: GET
    url: https://localhost/
");

            var stubJsonHttpClientFactory = Yaml<StubJsonHttpClientFactory>(@"
Responses:
 ""https://localhost/"":
  GET:
   status: 200
   body:
    beta:
     gamma: delta
");

            InitializeServices(stubBlueprintManager, stubJsonHttpClientFactory);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);
        }
    }
}
