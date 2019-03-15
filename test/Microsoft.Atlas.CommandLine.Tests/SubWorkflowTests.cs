// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Commands;
using Microsoft.Atlas.CommandLine.Tests.Stubs;
using Microsoft.Atlas.CommandLine.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests
{
    [TestClass]
    public class SubWorkflowTests : ServiceContextTestsBase<WorkflowCommandsTests.ServiceContext>
    {
#pragma warning disable SA1201 // Elements must appear in the correct order
        public class ServiceContext : ServiceContextBase
        {
            public WorkflowCommands WorkflowCommands { get; set; }

            public CommandLineApplicationServices App { get; set; }
        }

        [TestMethod]
        public async Task FileSystemSubWorkflowCanBeCalled()
        {
            var stubs = Yaml<StubFileSystem>(@"
  Files:
   the-test/workflow.yaml: |
    operations:
    - message: Before
    - workflow: lib/step1
    - message: After
   the-test/lib/step1/workflow.yaml: |
    operations:
    - message: During
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("Before", "During", "After");
        }

        [TestMethod]
        public async Task WebServerSubWorkflowCanBeCalled()
        {
            var stubRequests = Yaml<StubHttpClientHandlerFactory>(@"
Files:
  https://localhost/the-test/workflow.yaml: |
    operations:
    - message: Before
    - workflow: lib/step1
    - message: After
  https://localhost/the-test/lib/step1/workflow.yaml: |
    operations:
    - message: During
");

            InitializeServices(stubRequests, new StubFileSystem());

            var result = Services.App.Execute("deploy", "https://localhost/the-test/workflow.yaml");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("Before", "During", "After");
        }

        [TestMethod]
        public async Task FileSystemRelativeReferenceImportsFiles()
        {
            var stubs = Yaml<StubFileSystem>(@"
Files:
  the-test/readme.md: |
    ``` yaml
    info:
        title: TheTest
    workflows:
      local:
        inputs:
        - step1
    ```
  the-test/workflow.yaml: |
    operations:
    - message: Before
    - workflow: workflows/step1
    - message: After
  step1/workflow.yaml: |
    operations:
    - message: During
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("Before", "During", "After");
        }

        [TestMethod]
        public async Task WebServerRelativeReferenceImportsFiles()
        {
            var stubs = Yaml<StubHttpClientHandlerFactory>(@"
Files:
  https://localhost/the-test/readme.md: |
    ``` yaml
    info:
        title: TheTest
    workflows:
      local:
        inputs:
        - step1
    ```
  https://localhost/the-test/workflow.yaml: |
    operations:
    - message: Before
    - workflow: workflows/step1
    - message: After
  https://localhost/step1/workflow.yaml: |
    operations:
    - message: During
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "https://localhost/the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("Before", "During", "After");
        }

        [TestMethod]
        public async Task OnlyWorkflowOperationValuesPassThrough()
        {
            var stubs = Yaml<StubHttpClientHandlerFactory>(@"
Files:
  https://localhost/the-test/workflow.yaml: |
    operations:
    - workflow: ../step1
      values: { x: ( xValue ) }
  https://localhost/step1/workflow.yaml: |
    operations:
    - message: (['x=<', x, '>'])
    - message: (['y=<', y, '>'])
    - message: (['xValue=<', xValue, '>'])
    - message: (['yValue=<', yValue, '>'])
");

            InitializeServices(stubs);

            var result = Services.App.Execute(
                "deploy",
                "https://localhost/the-test",
                "--set",
                "xValue=alpha",
                "--set",
                "yValue=beta",
                "--set",
                "x=gamma",
                "--set",
                "y=delta");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("x=<alpha>", "y=<>", "xValue=<>", "yValue=<>");
        }

        [TestMethod]
        public async Task OnlyWorkflowOperationOutputPassBack()
        {
            var stubs = Yaml<StubHttpClientHandlerFactory>(@"
Files:
  https://localhost/the-test/workflow.yaml: |
    operations:
    - workflow: ../step1
      output: { xOut: ( result.x ) }
    - message: ([ 'everything is ', to_string(@) ])
  https://localhost/step1/workflow.yaml: |
    operations:
    - output:
        x: alpha
        y: beta
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "https://localhost/the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder(@"everything is {""xOut"":""alpha""}");
        }

        [TestMethod]
        public async Task WorkflowOutputWorksSameAsOperationOutput()
        {
            var stubs = Yaml<StubHttpClientHandlerFactory>(@"
Files:
  https://localhost/the-test/workflow.yaml: |
    operations:
    - workflow: ../step1
      output: { xOut: ( result.x ) }
    - message: ([ 'everything is ', to_string(@) ])
  https://localhost/step1/workflow.yaml: |
    operations:
    - output:
        xOperation: alpha
        yOperation: beta
    output:
      x: ( xOperation )
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "https://localhost/the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder(@"everything is {""xOut"":""alpha""}");
        }

        [TestMethod]
        public async Task WorkflowValuesYamlFileIsInEffect()
        {
            var stubs = Yaml<StubHttpClientHandlerFactory>(@"
Files:
  https://localhost/the-test/workflow.yaml: |
    operations:
    - workflow: step1
      output: (result)
    - condition: (x != 'one')
      throw: 
        message: (['Expected x == one, actual <', x||'null', '>'])
  https://localhost/the-test/step1/values.yaml: |
    xValue: one
  https://localhost/the-test/step1/workflow.yaml: |
    operations:
    - output: {x: (xValue)}
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "https://localhost/the-test");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task WorkflowValuesPropertyIsInEffect()
        {
            var stubs = Yaml<StubHttpClientHandlerFactory>(@"
Files:
  https://localhost/the-test/workflow.yaml: |
    operations:
    - workflow: step1
      output: (result)
    - condition: (y != 'two')
      throw: 
        message: (['Expected y == two, actual <', y||'null', '>'])
  https://localhost/the-test/step1/workflow.yaml: |
    values:
      yValue: two
    operations:
    - output: {y: (yValue)}
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "https://localhost/the-test");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task WorkflowModelsYamlIsInEffect()
        {
            var stubs = Yaml<StubHttpClientHandlerFactory>(@"
Files:
  https://localhost/the-test/workflow.yaml: |
    operations:
    - workflow: step1
      values:
        zValueInput: three
      output: (result)
    - condition: (z != 'three')
      throw: 
        message: (['Expected z == three, actual <', z||'null', '>'])
  https://localhost/the-test/step1/model.yaml: |
    zValue: {{ zValueInput }}
  https://localhost/the-test/step1/workflow.yaml: |
    operations:
    - output: {z: (zValue)}
");

            InitializeServices(stubs);

            var result = Services.App.Execute("deploy", "https://localhost/the-test");

            Assert.AreEqual(0, result);
        }
    }
}
