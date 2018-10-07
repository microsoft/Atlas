// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.Atlas.CommandLine.Execution;
using Microsoft.Atlas.CommandLine.Secrets;
using Microsoft.Atlas.CommandLine.Tests.Stubs;
using Microsoft.Atlas.CommandLine.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests
{
    [TestClass]
    public class CatchOperationTests : ServiceContextTestsBase<RequestOperationTests.ServiceContext>
    {
        [TestMethod]
        public void ThrowOperationCanBeCaught()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - message: Catching operation
                          operations:
                          - message: Throwing operation
                            throw: { message: boom }
                          catch: {}
                        - message: Still Running
            ");

            InitializeServices(stubBlueprints);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("Catching operation", "Throwing operation", "Still Running");
        }

        [TestMethod]
        public void CatchesIfConditionIsTrue()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - message: Catching operation
                          operations:
                          - message: Throwing operation
                            throw: { message: boom }
                          catch:
                            condition: Message == 'boom'
                        - message: Still Running
            ");

            InitializeServices(stubBlueprints);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("Catching operation", "Throwing operation", "Still Running");
        }

        [TestMethod]
        public void NotCaughtIfConditionIsFalse()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - message: Catching operation
                          operations:
                          - message: Throwing operation
                            throw: { message: different }
                          catch:
                            condition: Message == 'boom'
                        - message: Still Running
            ");

            InitializeServices(stubBlueprints);

            var ex = Assert.ThrowsException<OperationException>(() => Services.App.Execute("deploy", "the-test"));

            Assert.AreEqual("different", ex.Message);

            Console.AssertContainsInOrder("Catching operation", "Throwing operation", "different");
        }

        [TestMethod]
        public void InvalidStatusCodeDetailsCanBeCaught()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - message: Catching operation
                          operations:
                          - message: Making Bad Request
                            request: request.yaml
                          catch:
                            condition: result.status == 404
                        - message: Still Running
                      request.yaml: |
                        method: GET
                        url: https://localhost/bad-request
            ");

            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>(@"
                Responses:
                  https://localhost/bad-request:
                    GET:
                      status: 404
                      body: No Such Page
            ");

            InitializeServices(stubBlueprints, stubHttpClients);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("d:", "one", "two", "three", "four", "five", "six");
        }

        public class ServiceContext : ServiceContextBase
        {
            public CommandLineApplicationServices App { get; set; }
        }
    }
}
