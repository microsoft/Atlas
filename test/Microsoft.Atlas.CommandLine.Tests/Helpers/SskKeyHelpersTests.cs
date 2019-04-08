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
    public class SskKeyHelpersTests : ServiceContextTestsBase<SskKeyHelpersTests.ServiceContext>
    {
#pragma warning disable SA1201 // Elements must appear in the correct order
        public class ServiceContext : ServiceContextBase
        {
            public ITemplateEngineFactory TemplateEngineFactory { get; set; }
        }

        [TestMethod]
        public void SshKeyGenBlockHelperHasPublicAndPrivateKeyContextVariables()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    {
                        "test", @"
{{# sshkeygen }}
publicKey: {{{ json publicKey }}}
privateKey: {{{ json privateKey }}}
{{/ sshkeygen }}
"
                    }
                }
            });

            var properties = templateEngine.Render<IDictionary<string, string>>("test", null);
            Assert.AreNotEqual(string.Empty, properties["publicKey"]);
            Assert.AreNotEqual(string.Empty, properties["privateKey"]);

            var lines = properties["privateKey"].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual("-----BEGIN RSA PRIVATE KEY-----", lines.First());
            Assert.AreEqual("-----END RSA PRIVATE KEY-----", lines.Last());
        }

        [TestMethod]
        public void DefaultEncodingAlsoProtectsNewLineCharacters()
        {
            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    {
                        "test", @"
{{# sshkeygen }}
publicKey: ""{{ publicKey }}""
privateKey: ""{{ privateKey }}""
{{/ sshkeygen }}
"
                    }
                }
            });

            var properties = templateEngine.Render<IDictionary<string, string>>("test", null);
            Assert.AreNotEqual(string.Empty, properties["publicKey"]);
            Assert.AreNotEqual(string.Empty, properties["privateKey"]);

            var lines = properties["privateKey"].Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual("-----BEGIN RSA PRIVATE KEY-----", lines.First());
            Assert.AreEqual("-----END RSA PRIVATE KEY-----", lines.Last());
        }

        [TestMethod]
        public void PrivateKeyIsAddedToSecretsTracker()
        {
            var secretTracker = new StubSecretTracker();
            InitializeServices(secretTracker);

            var templateEngine = Services.TemplateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new DictionaryFileSystem
                {
                    {
                        "test", @"
{{# sshkeygen }}
publicKey: {{{ json publicKey }}}
privateKey: {{{ json privateKey }}}
{{/ sshkeygen }}
"
                    }
                }
            });

            var properties = templateEngine.Render<IDictionary<string, string>>("test", null);

            Assert.IsFalse(secretTracker.Secrets.Contains(properties["publicKey"]));
            Assert.IsTrue(secretTracker.Secrets.Contains(properties["privateKey"]));
        }

        private static string Render(ITemplateEngine templateEngine, string templateFile, object values)
        {
            using (var writer = new StringWriter())
            {
                templateEngine.Render(templateFile, values, writer);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
