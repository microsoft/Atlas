// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using Microsoft.Atlas.CommandLine.Templates;
using Microsoft.Atlas.CommandLine.Tests.Stubs;
using Microsoft.Atlas.CommandLine.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests
{
    [TestClass]
    public class TemplateEngineTests : ServiceContextTestsBase<TemplateEngineTests.ServiceContext>
    {
#pragma warning disable SA1201 // Elements must appear in the correct order
        public class ServiceContext : ServiceContextBase
        {
            public ITemplateEngineFactory TemplateEngineFactory { get; set; }
        }

        [TestMethod]
        public void RenderToStringWriterIsVerbatim()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", "This is a file" }
                }
            });

            var output = Render(templateEngine, "test", null);

            Assert.AreEqual("This is a file", output);
        }

        [TestMethod]
        public void ContextValuesCanBeReferenced()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", "This is {{Alpha}}." }
                }
            });

            var output = Render(templateEngine, "test", new { Alpha = "beta" });

            Assert.AreEqual("This is beta.", output);
        }

        [TestMethod]
        public void RenderCanDeserializeJson()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", "{ \"notes\": \"this is json\" } " }
                }
            });

            var result = (IDictionary<object, object>)templateEngine.Render<object>("test", null);

            Assert.AreEqual("this is json", result["notes"]);
        }

        [TestMethod]
        public void RenderCanDeserializeYaml()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", "notes: this is yaml" }
                }
            });

            var result = (IDictionary<object, object>)templateEngine.Render<object>("test", null);

            Assert.AreEqual("this is yaml", result["notes"]);
        }

        [TestMethod]
        public void RenderCanDeserializeClasses()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", "description: this is the description" }
                }
            });

            var result = templateEngine.Render<SimpleModel>("test", null);

            Assert.AreEqual("this is the description", result.description);
        }

        [TestMethod]
        public void PartialFilesCanBeReferenced()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", "this is coming from {{> another }} file" },
                    { "another", "a totally different" }
                }
            });

            var output = Render(templateEngine, "test", null);

            Assert.AreEqual("this is coming from a totally different file", output);
        }

        private static string Render(ITemplateEngine templateEngine, string templateFile, object values)
        {
            using (var writer = new StringWriter())
            {
                templateEngine.Render(templateFile, values, writer);
                return writer.GetStringBuilder().ToString();
            }
        }

        public class SimpleModel
        {
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles
            public string description { get; set; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1300 // Element must begin with upper-case letter
        }
    }
}
