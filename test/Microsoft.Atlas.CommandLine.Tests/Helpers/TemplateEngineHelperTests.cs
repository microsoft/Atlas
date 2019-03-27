// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Atlas.CommandLine.Templates;
using Microsoft.Atlas.CommandLine.Tests.Stubs;
using Microsoft.Atlas.CommandLine.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests.Helpers
{
    [TestClass]
    public class TemplateEngineHelperTests : ServiceContextTestsBase<TemplateEngineTests.ServiceContext>
    {
#pragma warning disable SA1201 // Elements must appear in the correct order
        public class ServiceContext : ServiceContextBase
        {
            public ITemplateEngineFactory TemplateEngineFactory { get; set; }
        }

        public static string Render(ITemplateEngine templateEngine, string templateFile, object values)
        {
            using (var writer = new StringWriter())
            {
                templateEngine.Render(templateFile, values, writer);
                return writer.GetStringBuilder().ToString();
            }
        }

        [TestMethod]
        public void GuidHelperProducesRepeatableHashedGuids()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    {
                        "test", @"
guid1: {{ guid 123 ""This is a hash guid"" 456 }} 
guid2: {{ guid 123 ""This is a hash guid"" 456 }} 
"
                    }
                }
            });

            var guids = templateEngine.Render<IDictionary<string, Guid>>("test", null);
            Assert.AreEqual(guids["guid1"], guids["guid2"]);
        }

        [TestMethod]
        public void AnyDifferenceMakesHashGuidsDifferent()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    {
                        "test", @"
guid1: {{ guid 123 ""This is a hash guid"" 456 }} 
guid2: {{ guid 123 ""This is a hash guid!"" 456 }} 
guid3: {{ guid 123 ""This is a hash guid"" 406 }} 
"
                    }
                }
            });

            var guids = templateEngine.Render<IDictionary<string, Guid>>("test", null);
            Assert.AreNotEqual(guids["guid1"], guids["guid2"]);
            Assert.AreNotEqual(guids["guid1"], guids["guid3"]);
        }

        [TestMethod]
        public void GuidHelperCreatesDifferentRandomGuids()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    {
                        "test", @"
guid1: {{ guid provider=""RNGCryptoServiceProvider"" }}
guid2: {{ guid provider=""RNGCryptoServiceProvider"" }}
"
                    }
                }
            });

            var guids = templateEngine.Render<IDictionary<string, Guid>>("test", null);
            Assert.AreNotEqual(guids["guid1"], guids["guid2"]);
        }

        [TestMethod]
        public void SecretHelperAddsValuesToSecretTrackerService()
        {
            var secretTracker = new StubSecretTracker();
            InitializeServices(secretTracker);

            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", @"This {{ secret ""token"" }} is a secret" }
                }
            });

            var output = Render(templateEngine, "test", null);

            Assert.AreEqual("This token is a secret", output);
            Assert.AreEqual("token", secretTracker.Secrets.SingleOrDefault());
        }

        [TestMethod]
        [Ignore("The rendering of the indent block helper is including a blank line with /r instead of /r/n at the end of the blank line.")]
        public void IndentHelperIndentsContent()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    {
                        "test", @"
one
--two
--{{# indent 5 }}
--three
four
----five
--{{/ indent }}
six
".Replace('-', ' ')
                    }
                }
            });

            var output = Render(templateEngine, "test", null);
            Assert.AreEqual(
@"one
--two
-------three
-----four
---------five
-----
six
".Replace('-', ' '), output);
        }

        [TestMethod]
        public void JsonHelperRendersPartOfTheContextBackOutAsJson()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", "data: {{{ json something }}}" }
                }
            });

            var values = new
            {
                something = new
                {
                    list = new object[] { 5, "alpha" }
                }
            };

            var output = Render(templateEngine, "test", values);

            Assert.AreEqual(@"data: {""list"":[5,""alpha""]}", output);
        }

        [TestMethod]
        [Ignore("BUG: JsonCompatible yaml serializer will render single scalar with a trailing end of stream ... marker")]
        public void QueryHelperRunsJMESPathOnContextAndRendersScalarResultAsJson()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", @"data: {{{ query ""length(something.list)"" }}}" }
                }
            });

            var values = new
            {
                something = new
                {
                    list = new object[] { 5, "alpha" }
                }
            };

            var output = Render(templateEngine, "test", values);

            Assert.AreEqual(@"data: 2", output);
        }

        [TestMethod]
        public void QueryHelperRunsJMESPathOnContextAndRendersResultAsJson()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    { "test", @"data: {{{ query ""[ length(something.list), something.list[1] ]"" }}}" }
                }
            });

            var values = new
            {
                something = new
                {
                    list = new object[] { 5, "alpha" }
                }
            };

            var output = Render(templateEngine, "test", values);

            Assert.AreEqual(@"data: [2,""alpha""]", output);
        }

        [TestMethod]
        public void QueryHelperRunsContainedBlockOncePerJMESPathResult()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    {
                        "test", @"
{{# query ""something.list"" }}
{{.}}
{{/ query }}
"
                    }
                }
            });

            var values = new
            {
                something = new
                {
                    list = new[] { "alpha", "beta", "gamma" }
                }
            };

            var output = Render(templateEngine, "test", values);

            Assert.AreEqual(
@"
alpha
beta
gamma
", output);
        }
    }
}
