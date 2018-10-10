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
                            condition: error.message == 'boom'
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
        public void CatchByStatusCode()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - message: Catching 404
                          request: notfound.yaml
                          catch:
                            condition: result.status == `404`
                        - message: Not Catching 503
                          request: servererror.yaml
                          catch:
                            condition: result.status == `404`
                        - message: Still Running
                      notfound.yaml: |
                        method: GET
                        url: https://localhost/notfound
                      servererror.yaml: |
                        method: GET
                        url: https://localhost/servererror
            ");

            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>(@"
                Responses:
                  https://localhost/notfound:
                    GET:
                      status: 404
                      body: Page error
                  https://localhost/servererror:
                    GET:
                      status: 503
                      body: Page error
            ");

            InitializeServices(stubBlueprints, stubHttpClients);

            var ex = Assert.ThrowsException<RequestException>(() => Services.App.Execute("deploy", "the-test"));

            Assert.IsTrue(ex.Message.Contains("503"));

            Assert.AreEqual(503, ex.Response.status);

            Console.AssertContainsInOrder("Catching 404", "Not Catching 503");

            Console.AssertNotContains("Still Running");
        }

        [TestMethod]
        public void ExceptionAndResultDetailsCanBeSavedInCatch()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - message: Catching 400
                          request: badrequest.yaml
                          catch:
                            output:
                              the-result: (result)
                              the-error: (error)
                        - message: Still Running
                      badrequest.yaml: |
                        method: GET
                        url: https://localhost/badrequest
            ");

            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>(@"
                Responses:
                  https://localhost/badrequest:
                    GET:
                      status: 400
                      headers: { Content-Type: ['application/json'] }
                      body: 
                        oops:
                          code: 1234
                          summary: This is a bad request
            ");

            InitializeServices(stubBlueprints, stubHttpClients);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder(
                "Catching 400",
                "Still Running",
                "the-result:",
                "status: 400",
                "body:",
                "oops:",
                "code:",
                "1234",
                "summary: This is a bad request",
                "the-error:",
                "message:",
                "400");
        }

        [TestMethod]
        public void ThrowDetailsCanBeSavedInCatch()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - message: Catching Exception
                          operations:
                          - message: Throwing Exception
                            throw:
                              message: one
                              details:
                                x: two
                                y: three
                          catch:
                            output:
                              the-details: (error.details)
                              the-message: (error.message)
                        - message: Still Running
            ");

            InitializeServices(stubBlueprints);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder(
                "Catching Exception",
                "Throwing Exception",
                "Still Running",
                "the-details:",
                "x: two",
                "y: three",
                "the-message: one");
        }

        public class ServiceContext : ServiceContextBase
        {
            public CommandLineApplicationServices App { get; set; }
        }
    }
}
