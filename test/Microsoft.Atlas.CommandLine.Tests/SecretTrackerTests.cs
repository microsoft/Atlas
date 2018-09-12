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
    public class SecretTrackerTests : ServiceContextTestsBase<SecretTrackerTests.ServiceContext>
    {
        private const string Redacted = "xxxxxxxx";

        [TestMethod]
        public void SecretsAreRemovedFromText()
        {
            var secretTracker = new SecretTracker();

            secretTracker.AddSecret("secret");
            var redacted1 = secretTracker.FilterString("This is a secret.");

            secretTracker.AddSecret("is");
            var redacted2 = secretTracker.FilterString("This is a secret.");

            Assert.AreEqual($"This is a {Redacted}.", redacted1);
            Assert.AreEqual($"Th{Redacted} {Redacted} a {Redacted}.", redacted2);
        }

        [TestMethod]
        public void SecretsAreReplacedInAnOrderThatIsDeterminedBySubstringContainment()
        {
            var secretTracker1 = new SecretTracker();

            secretTracker1.AddSecret("defghijkl");
            secretTracker1.AddSecret("abc");
            secretTracker1.AddSecret("ghi");
            secretTracker1.AddSecret("d");
            var redacted1 = secretTracker1.FilterString("abcdefghijklmno abc-def-ghi-jkl-mno");

            var secretTracker2 = new SecretTracker();
            secretTracker2.AddSecret("a");
            secretTracker2.AddSecret("d");
            secretTracker2.AddSecret("g");
            secretTracker2.AddSecret("ghi");
            secretTracker2.AddSecret("abc");
            secretTracker2.AddSecret("defghijkl");
            var redacted2 = secretTracker2.FilterString("abcdefghijklmno abc-def-ghi-jkl-mno");

            var secretTracker3 = new SecretTracker();
            secretTracker3.AddSecret("g");
            secretTracker3.AddSecret("defghijkl");
            secretTracker3.AddSecret("abc");
            secretTracker3.AddSecret("d");
            secretTracker3.AddSecret("ghi");
            secretTracker3.AddSecret("a");
            var redacted3 = secretTracker3.FilterString("abcdefghijklmno abc-def-ghi-jkl-mno");

            Assert.AreEqual($"{Redacted}{Redacted}mno {Redacted}-{Redacted}ef-{Redacted}-jkl-mno", redacted1);
            Assert.AreEqual($"{Redacted}{Redacted}mno {Redacted}-{Redacted}ef-{Redacted}-jkl-mno", redacted2);
            Assert.AreEqual($"{Redacted}{Redacted}mno {Redacted}-{Redacted}ef-{Redacted}-jkl-mno", redacted3);
        }

        [TestMethod]
        public void SecretsAreReplacedInTextStream()
        {
            var secretTracker = new SecretTracker();

            var outputStream = new StringWriter();
            using (var secretStream = secretTracker.FilterTextWriter(outputStream))
            {
                secretTracker.AddSecret("secret");
                secretStream.WriteLine("This is a secret.");

                secretTracker.AddSecret("is");
                secretStream.WriteLine("This is a secret.");
            }

            var output = outputStream.GetStringBuilder().ToString();
            var expected = $"This is a {Redacted}.{Environment.NewLine}Th{Redacted} {Redacted} a {Redacted}.{Environment.NewLine}";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void SecretsAreReplacedEvenIfTheyAreOverSmallerWrites()
        {
            var secretTracker = new SecretTracker();

            var outputStream = new StringWriter();
            using (var secretStream = secretTracker.FilterTextWriter(outputStream))
            {
                secretTracker.AddSecret("sIs5429Lon");

                // ThisIs5429Longer
                secretStream.Write("ThisIs5");
                secretStream.Write(42);
                secretStream.Write('9');
                secretStream.Write('L');
                secretStream.WriteLine("onger");
            }

            var output = outputStream.GetStringBuilder().ToString();

            Assert.AreEqual($"Thi{Redacted}ger{Environment.NewLine}", output);
        }

        [TestMethod]
        public void IncompleteLineIsWrittenByDispose()
        {
            var secretTracker = new SecretTracker();

            var outputStream = new StringWriter();
            using (var secretStream = secretTracker.FilterTextWriter(outputStream))
            {
                secretStream.WriteLine("FullLine");
                secretStream.Write("IncompleteLine");
            }

            var output = outputStream.GetStringBuilder().ToString();
            Assert.AreEqual($"FullLine{Environment.NewLine}IncompleteLine", output);
        }

        [TestMethod]
        public void IncompleteLineIsWrittenByFlush()
        {
            var secretTracker = new SecretTracker();

            var outputStream = new StringWriter();
            string output = null;
            using (var secretStream = secretTracker.FilterTextWriter(outputStream))
            {
                secretStream.WriteLine("FullLine");
                secretStream.Write("IncompleteLine");

                secretStream.Flush();

                output = outputStream.GetStringBuilder().ToString();
            }

            Assert.AreEqual($"FullLine{Environment.NewLine}IncompleteLine", output);
        }

        [TestMethod]
        public void IncompleteLineIsWrittenByClose()
        {
            var secretTracker = new SecretTracker();

            var outputStream = new StringWriter();
            string output = null;
            using (var secretStream = secretTracker.FilterTextWriter(outputStream))
            {
                secretStream.WriteLine("FullLine");
                secretStream.Write("IncompleteLine");

                secretStream.Close();

                output = outputStream.GetStringBuilder().ToString();
            }

            Assert.AreEqual($"FullLine{Environment.NewLine}IncompleteLine", output);
        }

        [TestMethod]
        public void SecretIsDetectedInPartialLineFollowingNewline()
        {
            var secretTracker = new SecretTracker();
            secretTracker.AddSecret("SecretEmbedded");

            var outputStream = new StringWriter();
            using (var secretStream = secretTracker.FilterTextWriter(outputStream))
            {
                secretStream.WriteLine("FullLine");
                secretStream.Write($"IncompleteLine{Environment.NewLine}WithSecret");
                secretStream.WriteLine("EmbeddedInIt");
            }

            var output = outputStream.GetStringBuilder().ToString();
            Assert.AreEqual($"FullLine{Environment.NewLine}IncompleteLine{Environment.NewLine}With{Redacted}InIt{Environment.NewLine}", output);
        }

        [TestMethod]
        public void NullOrEmptySecretsAreIgnored()
        {
            var secretTracker = new SecretTracker();
            secretTracker.AddSecret(string.Empty);
            secretTracker.AddSecret(null);

            var outputStream = new StringWriter();
            using (var secretStream = secretTracker.FilterTextWriter(outputStream))
            {
                secretStream.WriteLine("DoesNotFail");
            }

            var output = outputStream.GetStringBuilder().ToString();
            Assert.AreEqual($"DoesNotFail{Environment.NewLine}", output);
        }

        [TestMethod]
        public void HttpResponseSecretsAreHidden()
        {
            var stubBlueprints = Yaml<StubBlueprintManager>(@"
                Blueprints:
                  hide-secrets:
                    Files:
                      workflow.yaml: |
                        operations:
                        - message: Fetching secrets
                          request: request.yaml
                          output:
                            things:
                              one: (keys[0].value)
                              two: (keys[1].value)
                      request.yaml: |
                        method: GET
                        url: https://localhost/
                        secret: keys[*].value
            ");

            var stubHttpClients = Yaml<StubHttpClientHandlerFactory>(@"
                Responses:
                  https://localhost/:
                    GET:
                      status: 200
                      body:
                        keys:
                        - name: key-1
                          value: alpha
                        - name: key-2
                          value: beta
            ");

            InitializeServices(stubBlueprints, stubHttpClients);

            var result = Services.App.Execute("deploy", "hide-secrets");

            Assert.AreEqual(0, result);

            Console.AssertContainsInOrder("Fetching secrets", "one: xxxxxxxx", "two: xxxxxxxx");
        }

        public class ServiceContext : ServiceContextBase
        {
            public ISecretTracker SecretTracker { get; set; }

            public CommandLineApplicationServices App { get; set; }
        }
    }
}
