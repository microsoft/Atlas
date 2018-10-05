// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.Atlas.CommandLine.Secrets;
using Microsoft.Atlas.CommandLine.Tests.Stubs;
using Microsoft.Atlas.CommandLine.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests
{
    [TestClass]
    public class RequestOperationTests : ServiceContextTestsBase<RequestOperationTests.ServiceContext>
    {
        [TestMethod]
        public void ResponseBodyMayBeQueried()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - request: request.yaml
                          output:
                            x: (result.body.color)
                            y: (result.body.id)
                      request.yaml: |
                        method: GET
                        url: https://localhost/
            ");

            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>(@"
                Responses:
                  https://localhost/:
                    GET:
                      status: 200
                      body:
                        id: 4
                        color: green
            ");

            InitializeServices(stubBlueprints, stubHttpClients);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("x: green", "y:", "4");
        }

        [TestMethod]
        public void ResponseMayBeMergedWithWorkingMemory()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - output:
                            b: [three,four]
                        - values:
                            a: [one,two]
                          request: request.yaml
                          output:
                            d: ([a,b,result.body,c][])
                      request.yaml: |
                        method: GET
                        url: https://localhost/
            ");

            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>(@"
                Responses:
                  https://localhost/:
                    GET:
                      status: 200
                      body:
                        c: [five,six]
            ");

            InitializeServices(stubBlueprints, stubHttpClients);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("d:", "one", "two", "three", "four", "five", "six");
        }

        [TestMethod]
        public void StatusAndHeadersAreAvailable()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - request: request.yaml
                          output:
                            x: (result.status)
                            y: (result.headers.""Content-Type""[0])
                            z: (result.headers.""Content-Length""[0])
                      request.yaml: |
                        method: GET
                        url: https://localhost/
            ");

            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>(@"
                Responses:
                  https://localhost/:
                    GET:
                      status: 200
                      headers:
                        Content-Type: [text/plain]
                        Content-Length: [12]
                      body: Hello world!
            ");

            InitializeServices(stubBlueprints, stubHttpClients);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("x:", "200", "y:", "text/plain", "z:", "12");
        }

        public class ServiceContext : ServiceContextBase
        {
            public CommandLineApplicationServices App { get; set; }
        }
    }
}
