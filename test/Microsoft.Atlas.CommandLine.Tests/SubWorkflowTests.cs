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
    }
}
