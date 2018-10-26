using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Atlas.CommandLine.Swagger;
using Microsoft.Atlas.CommandLine.Swagger.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests.Swagger
{
    [TestClass]
    public class RequestGeneratorTests
    {
        [TestMethod]
        [DataRow("api/tests", "ExampleClient", new[] { "Examples" }, "Examples_CreateOrUpdate", "/api/tests/ExampleClient/Examples/CreateOrUpdate.yaml")]
        [DataRow("api/tests", "ExampleClient", null, "Examples_CreateOrUpdate", "/api/tests/ExampleClient/Examples_CreateOrUpdate.yaml")]
        [DataRow("api/tests", "ExampleClient", new[] { "One", "Two" }, "Examples_CreateOrUpdate", "/api/tests/ExampleClient/One/Two/Examples_CreateOrUpdate.yaml")]
        [DataRow("api/tests", "ExampleClient", new[] { "Examples" }, "Delete", "/api/tests/ExampleClient/Examples/Delete.yaml")]
        public void ResultingFilePathContainsPrefixTitleAndOperationId(string targetPrefix, string title, string[] operationTags, string operationId, string expectedPath)
        {
            var requestGenerator = new RequestGenerator();
            var context = CreateContext(targetPrefix: targetPrefix, infoTitle: title, operationTags: operationTags, operationId: operationId);

            requestGenerator.GenerateSingleRequestDefinition(context);

            Assert.AreEqual(expectedPath, context.GeneratedPath);
        }

        [TestMethod]
        [DataRow("put", "method: PUT")]
        [DataRow("GET", "method: GET")]
        public void HttpMethodComesFromOperationKey(string operationKey, string expectedMethod)
        {
            var requestGenerator = new RequestGenerator();
            var context = CreateContext(operationKey: operationKey);

            requestGenerator.GenerateSingleRequestDefinition(context);

            Assert.IsTrue(context.GeneratedContent.Contains(expectedMethod, StringComparison.Ordinal));
        }

        [TestMethod]
        [DataRow(new[] { "https" }, "url: https://")]
        [DataRow(new[] { "http" }, "url: http://")]
        [DataRow(new[] { "https", "http" }, "url: https://")]
        [DataRow(new[] { "http", "https" }, "url: https://")]
        [DataRow(null, "url: https://")]
        [DataRow(new[] { "ws", "http", "wss" }, "url: http://")]
        public void HttpsIsUsedWhenPossibleHttpIsUsedOtherwise(string[] schemes, string expectedUrl)
        {
            var requestGenerator = new RequestGenerator();
            var context = CreateContext(schemes: schemes != null ? string.Join(' ', schemes) : null);

            requestGenerator.GenerateSingleRequestDefinition(context);

            Assert.IsTrue(context.GeneratedContent.Contains(expectedUrl, StringComparison.Ordinal), $"'{expectedUrl}' not found in\r\n{context.GeneratedContent}");
        }

        [TestMethod]
        [DataRow("the-host", "url: https://the-host")]
        [DataRow("the-host:5001", "url: https://the-host:5001")]
        [DataRow(null, "url: https://{{ request.host }}")]
        public void HostAppearsInUrl(string host, string expectedUrl)
        {
            var requestGenerator = new RequestGenerator();
            var context = CreateContext(host: host);

            requestGenerator.GenerateSingleRequestDefinition(context);

            Assert.IsTrue(context.GeneratedContent.Contains(expectedUrl, StringComparison.Ordinal), $"'{expectedUrl}' not found in\r\n{context.GeneratedContent}");
        }

        [TestMethod]
        [DataRow("/pets", "url: https://example.local/pets")]
        [DataRow("/pets/again", "url: https://example.local/pets/again")]
        [DataRow("/", "url: https://example.local/")]
        public void PathAppearsInUrl(string pathKey, string expectedUrl)
        {
            var requestGenerator = new RequestGenerator();
            var context = CreateContext(pathKey: pathKey);

            requestGenerator.GenerateSingleRequestDefinition(context);

            Assert.IsTrue(context.GeneratedContent.Contains(expectedUrl, StringComparison.Ordinal), $"'{expectedUrl}' not found in\r\n{context.GeneratedContent}");
        }

        [TestMethod]
        [DataRow(null, "url: https://example.local/pets")]
        [DataRow("/v1", "url: https://example.local/v1/pets")]
        [DataRow("/another/prefix", "url: https://example.local/another/prefix/pets")]
        public void BasePathAppearsInUrl(string basePath, string expectedUrl)
        {
            var requestGenerator = new RequestGenerator();
            var context = CreateContext(basePath: basePath);

            requestGenerator.GenerateSingleRequestDefinition(context);

            Assert.IsTrue(context.GeneratedContent.Contains(expectedUrl, StringComparison.Ordinal), $"'{expectedUrl}' not found in\r\n{context.GeneratedContent}");
        }

        [TestMethod]
        public void RequiredParameterInPathIsTemplatized()
        {
            var requestGenerator = new RequestGenerator();
            var context = CreateContext(pathKey: "/pets/{id}");
            context.Operation.Value.parameters.Add(new Parameter
            {
                name = "id",
                @in = "path",
                required = true,
            });

            requestGenerator.GenerateSingleRequestDefinition(context);

            Assert.IsTrue(context.GeneratedContent.Contains("/pets/{{ parameter request.id required=true }}", StringComparison.Ordinal), "Expected text not found in \r\n" + context.GeneratedContent);
        }

        [TestMethod]
        public void DefaultParameterInPathIsTemplatized()
        {
            var requestGenerator = new RequestGenerator();
            var context = CreateContext(pathKey: "/styles/{style}");
            context.Operation.Value.parameters.Add(new Parameter
            {
                name = "style",
                @in = "path",
                @default = "plain",
            });

            requestGenerator.GenerateSingleRequestDefinition(context);

            Assert.IsTrue(context.GeneratedContent.Contains("/styles/{{ parameter request.style default=\"plain\" }}", StringComparison.Ordinal), "Expected text not found in \r\n" + context.GeneratedContent);
        }

        private static GenerateSingleRequestDefinitionContext CreateContext(
            string targetPrefix = "api/tests",
            string infoTitle = "ExampleClient",
            string schemes = "https",
            string host = "example.local",
            string basePath = null,
            string pathKey = "/pets",
            string operationKey = "put",
            string[] operationTags = null,
            string operationId = "Examples_CreateOrUpdate")
        {
            return new GenerateSingleRequestDefinitionContext
            {
                TargetPrefix = targetPrefix,
                Swagger = new SwaggerDocument
                {
                    info = new Info { title = infoTitle },
                    schemes = schemes != null ? schemes.Split(' ').ToList() : null,
                    host = host,
                    basePath = basePath,
                },
                Path = KeyValuePair.Create(pathKey, new PathItem {}),
                Operation = KeyValuePair.Create(operationKey, new Operation { tags = operationTags?.ToList(), operationId = operationId }),
            };
        }
    }
}
