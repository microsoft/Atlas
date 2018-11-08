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
    public class ForeachOperationTests : ServiceContextTestsBase<RequestOperationTests.ServiceContext>
    {
        [TestMethod]
        public void ForeachValuesAreIterated()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - foreach:
                            values:
                              x: ['a', 'b', 'c']
                          message: (['x is ', x])
            ");

            InitializeServices(stubBlueprints);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("x is a", "x is b", "x is c");
        }

        [TestMethod]
        public void SeveralArraysCanBeIterated()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - foreach:
                            values:
                              x: ['a', 'b', 'c']
                              y: 
                                z: ['d', 'e', 'f']
                          message: (['x is ', x, ' y.z is ', y.z])
            ");

            InitializeServices(stubBlueprints);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder(
                "x is a",
                "y.z is d",
                "x is b",
                "y.z is e",
                "x is c",
                "y.z is f");
        }


        [TestMethod]
        public void ForeachHashObject()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      values.yaml: |
                          hash:
                            a: b
                            c: d
                            e: f
                      workflow.yaml: |
                        operations:
                        - foreach:
                            values: ( map(&{ key:[0], value:[1] }, items(hash)) )
                          message: ""(['1: ', key, ' is ', value])""
                        - foreach:
                            values:
                              key: ( keys(hash) )
                              value: ( values(hash) )
                          message: ""(['2: ', key, ' is ', value])""
                        - foreach:
                            values:
                              kv: ( items(hash) )
                          message: ""(['3: ', kv[0], ' is ', kv[1]])""
            ");

            InitializeServices(stubBlueprints);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder(
                "1: a is b",
                "1: c is d",
                "1: e is f",
                "2: a is b",
                "2: c is d",
                "2: e is f",
                "3: a is b",
                "3: c is d",
                "3: e is f");
        }

        [TestMethod]
        public void ForeachOutputIsConcatinated()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  the-test:
                    Files:
                      workflow.yaml: |
                        operations:
                        - foreach:
                            values:
                              x: ['a', 'b', 'c']
                            output:
                              y: ([x])
                        - message: (join('+', y))
            ");

            InitializeServices(stubBlueprints);

            var result = Services.App.Execute("deploy", "the-test");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("a+b+c");
        }
        public class ServiceContext : ServiceContextBase
        {
            public CommandLineApplicationServices App { get; set; }
        }
    }
}
