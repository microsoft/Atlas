// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.Tests.Stubs;
using Microsoft.Atlas.CommandLine.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests.Swagger
{
  [TestClass]
    public class SwaggerWorkflowTests : ServiceContextTestsBase<SwaggerWorkflowTests.ServiceContext>
    {
        [TestMethod]
        public void WorkflowCanRunEntirelyFromStubHttpResponses()
        {
            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>(@"
Responses:
  https://example.net/the-test/readme.md:
    GET:
      status: 200
      body: |
        ``` yaml
        info:
            title: TheTest
        ```
  https://example.net/the-test/workflow.yaml:
    GET:
      status: 200
      body:
        operations:
        - message: This is a test
          output: { x: green }
            ");

            InitializeServices(stubHttpClients);

            var result = Services.App.Execute("deploy", "https://example.net/the-test");

            System.Console.Error.WriteLine(Console.ErrorStringWriter.ToString());
            System.Console.Out.WriteLine(Console.OutStringWriter.ToString());

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("This is a test", "x: green");
        }

        [TestMethod]
        [DataRow("api/tests", "ExampleClient", new[] { "Examples" }, "Examples_CreateOrUpdate", "api/tests/ExampleClient/Examples/CreateOrUpdate.yaml")]
        [DataRow("api/tests", "ExampleClient", null, "Examples_CreateOrUpdate", "api/tests/ExampleClient/Examples_CreateOrUpdate.yaml")]
        [DataRow("api/tests", "ExampleClient", new[] { "One", "Two" }, "Examples_CreateOrUpdate", "api/tests/ExampleClient/One/Two/Examples_CreateOrUpdate.yaml")]
        [DataRow("api/tests", "ExampleClient", new[] { "Examples" }, "Delete", "api/tests/ExampleClient/Examples/Delete.yaml")]
        public async Task ResultingFilePathContainsPrefixTitleTagsAndOperationId(string targetPrefix, string title, string[] operationTags, string operationId, string expectedPath)
        {
            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>($@"
Responses:
  https://example.net/the-test/readme.md:
    GET:
      status: 200
      body: |
        ``` yaml
        info:
          title: TheTest
        swagger:
          foo:
            target: {targetPrefix}
            source: https://example.org/specs/
            inputs:
            - testing/swagger.json
        ```
  https://example.net/the-test/workflow.yaml:
    GET:
      status: 200
      body:
        operations:
        - message: This is a test
          output: 
            x: green
  https://example.org/specs/testing/swagger.json:
    GET:
      status: 200
      body:
        swagger: 2.0
        info:
          title: {title}
        paths:
          /me:
            get:
              tags: [{string.Join(", ", operationTags ?? new string[0])}]
              operationId: {operationId}
            ");

            InitializeServices(stubHttpClients);

            var blueprint = await Services.BlueprintManager.GetBlueprintPackage("https://example.net/the-test");

            var generated = blueprint.GetGeneratedPaths().ToList();

            System.Console.Error.WriteLine(Console.ErrorStringWriter.ToString());
            System.Console.Out.WriteLine(Console.OutStringWriter.ToString());

            Assert.AreEqual(1, generated.Count());
            Assert.AreEqual(expectedPath, generated[0]);
        }

        [TestMethod]
        public async Task SimpleGetOperationBeingExecuted()
        {
            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>($@"
Responses:
  https://example.net/the-test/readme.md:
    GET:
      status: 200
      body: |
        ``` yaml
        info:
          title: TheTest
        swagger:
          foo:
            target: apis/path
            source: https://example.org/specs/
            inputs:
            - testing/swagger.json
        ```
  https://example.net/the-test/workflow.yaml:
    GET:
      status: 200
      body:
        operations:
        - request: apis/path/TestingClient/Me_Get.yaml
          output: ( result )
  https://example.org/specs/testing/swagger.json:
    GET:
      status: 200
      body:
        swagger: 2.0
        info:
          title: TestingClient
        host: example.com
        paths:
          /me:
            get:
              operationId: Me_Get
  https://example.com/me:
    GET:
      status: 200
      body:
        name: one
        id: 42
            ");

            InitializeServices(stubHttpClients);

            var result = Services.App.Execute("deploy", "https://example.net/the-test");

            System.Console.Error.WriteLine(Console.ErrorStringWriter.ToString());
            System.Console.Out.WriteLine(Console.OutStringWriter.ToString());

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("name: one", "id: 42");
        }

        [TestMethod]
        public async Task ParametersAppearInPathQueryAndBody()
        {
            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>($@"
Responses:
  https://example.net/the-test/readme.md:
    GET:
      status: 200
      body: |
        ``` yaml
        info:
          title: TheTest
        swagger:
          foo:
            target: apis/path
            source: https://example.org/specs/
            inputs:
            - testing/swagger.json
        ```
  https://example.net/the-test/workflow.yaml:
    GET:
      status: 200
      body:
        operations:
        - request: apis/path/TestingClient/CreateSomething.yaml
          values:
            request:
              parameters:
                one: uno
                two: dos
              body:
                four: quatro
                five: 5
          output: ( result )
  https://example.org/specs/testing/swagger.json:
    GET:
      status: 200
      body:
        swagger: 2.0
        info:
          title: TestingClient
        host: example.com
        paths:
          /somethings/{{one}}:
            put:
              operationId: CreateSomething
              parameters:
              - name: one
                in: path
              - name: two
                in: query
              - name: three
                in: body
                schema:
                  type: object
                  properties:
                    four:
                      type: string
                    five:
                      type: integer
  https://example.com/somethings/uno?two=dos:
    PUT:
      status: 200
      body:
      - request has arrived
            ");

            InitializeServices(stubHttpClients);

            var result = Services.App.Execute("deploy", "https://example.net/the-test");

            System.Console.Error.WriteLine(Console.ErrorStringWriter.ToString());
            System.Console.Out.WriteLine(Console.OutStringWriter.ToString());

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("request has arrived");

            var put = stubHttpClients.AssertRequest("PUT", "https://example.com/somethings/uno?two=dos");
            var body = await put.Content.ReadAsStringAsync();

            var expectedFourQuatroText = "\"four\":\"quatro\"";
            Assert.IsTrue(body.Contains(expectedFourQuatroText), $"Unable to find expected text '{expectedFourQuatroText}' in " + body);
            var expectedFive5Text = "\"five\":5";
            Assert.IsTrue(body.Contains(expectedFive5Text), $"Unable to find expected text '{expectedFive5Text}' in " + body);
        }

        [TestMethod]
        public async Task ApiVersionComesFromInfoVersionByDefault()
        {
            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>($@"
Responses:
  https://example.net/the-test/readme.md:
    GET:
      status: 200
      body: |
        ``` yaml
        info:
          title: TheTest
        swagger:
          foo:
            target: apis/path
            source: https://example.org/specs/
            inputs:
            - testing/swagger.json
        ```
  https://example.net/the-test/workflow.yaml:
    GET:
      status: 200
      body:
        operations:
        - request: apis/path/TestingClient/MethodOne.yaml
          output:
            one: ( result )
        - request: apis/path/TestingClient/MethodTwo.yaml
          output:
            two: ( result )
  https://example.org/specs/testing/swagger.json:
    GET:
      status: 200
      body:
        swagger: 2.0
        info:
          title: TestingClient
          version: 5.0-preview.2
        host: example.com
        paths:
          /method/one:
            get:
              operationId: MethodOne
              parameters:
              - name: api-version
                in: query
                required: true
          /v{{api-version}}/method/two:
            get:
              operationId: MethodTwo
              parameters:
              - name: api-version
                in: path
                required: true
  https://example.com/method/one?api-version=5.0-preview.2:
    GET:
      status: 200
      body:
      - request has arrived
  https://example.com/v5.0-preview.2/method/two:
    GET:
      status: 200
      body:
      - request has arrived
            ");

            InitializeServices(stubHttpClients);

            var result = Services.App.Execute("deploy", "https://example.net/the-test");

            System.Console.Error.WriteLine(Console.ErrorStringWriter.ToString());
            System.Console.Out.WriteLine(Console.OutStringWriter.ToString());

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("request has arrived", "request has arrived");
        }

        [TestMethod]
        public async Task SpacesAreRemoved()
        {
            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>($@"
Responses:
  https://example.net/the-test/readme.md:
    GET:
      status: 200
      body: |
        ``` yaml
        info:
          title: TheTest
        swagger:
          foo:
            target: apis/tests
            source: https://example.org/specs/
            inputs:
            - testing/swagger.json
        ```
  https://example.net/the-test/workflow.yaml:
    GET:
      status: 200
      body:
        operations:
        - request: apis/tests/TestingClient/TagOne/TagTwo/MethodOne.yaml
  https://example.org/specs/testing/swagger.json:
    GET:
      status: 200
      body:
        swagger: 2.0
        info:
          title: Testing Client
          version: 5.0-preview.2
        host: example.com
        paths:
          /path:
            get:
              tags:
              - Tag One
              - Tag Two
              operationId: Method One
  https://example.com/path:
    GET:
      status: 200
      body:
      - request has arrived
");

            InitializeServices(stubHttpClients);

            var result = Services.App.Execute("deploy", "https://example.net/the-test");

            System.Console.Error.WriteLine(Console.ErrorStringWriter.ToString());
            System.Console.Out.WriteLine(Console.OutStringWriter.ToString());

            Assert.AreEqual(0, result);

            stubHttpClients.AssertRequest("GET", "https://example.com/path");
        }

        public class ServiceContext : ServiceContextBase
        {
            public CommandLineApplicationServices App { get; set; }

            public IBlueprintManager BlueprintManager { get; set; }
        }
    }
}
