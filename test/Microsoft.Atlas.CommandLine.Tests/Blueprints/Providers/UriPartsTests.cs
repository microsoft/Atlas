// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Atlas.CommandLine.Blueprints.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests.Blueprints.Providers
{
    [TestClass]
    public class UriPartsTests
    {
        [TestMethod]
        public void UriIsParsedIntoParts()
        {
            var parts = UriParts.Parse("https://example.com/one/two?three=four&five=six#/seven");
            Assert.AreEqual("https", parts.Scheme);
            Assert.AreEqual(new HostString("example.com"), parts.Host);
            Assert.AreEqual(new PathString("/one") + new PathString("/two"), parts.Path);
            Assert.AreEqual(QueryString.Create("three", "four") + QueryString.Create("five", "six"), parts.Query);
            Assert.AreEqual(new FragmentString("#/seven"), parts.Fragment);
        }

        [TestMethod]
        public void UriBecomesStringAgain()
        {
            var parts = new UriParts
            {
                Scheme = "https",
                Host = new HostString("example.com"),
                Path = new PathString("/one") + new PathString("/two"),
                Query = QueryString.Create("three", "four") + QueryString.Create("five", "six"),
                Fragment = new FragmentString("#/seven"),
            };

            Assert.AreEqual("https://example.com/one/two?three=four&five=six#/seven", parts.ToString());
        }

        [TestMethod]
        [DataRow("https://github.com/name1/name2/tree/master/stable/workflow-name", "https://raw.githubusercontent.com/name1/name2/master/stable/workflow-name")]
        [DataRow("https://github.com/name1/name2/tree/master/stable/workflow-name/", "https://raw.githubusercontent.com/name1/name2/master/stable/workflow-name/")]
        [DataRow("https://github.com/name1/name2/blob/master/README.md", "https://raw.githubusercontent.com/name1/name2/master/README.md")]
        public void GitHubPathsChangedToRawFilePaths(string originalPath, string expectedPath)
        {
            var parts = UriParts.Parse(originalPath);
            parts.RewriteGitHubUris();

            Assert.AreEqual(expectedPath, parts.ToString());
        }

        [TestMethod]
        [DataRow("https://github.com/name1/name2/tree/master/stable/workflow-name/workflow.yaml", "https://github.com/name1/name2/tree/master/stable/workflow-name")]
        [DataRow("https://raw.githubusercontent.com/name1/name2/master/stable/workflow-name/workflow.yaml", "https://raw.githubusercontent.com/name1/name2/master/stable/workflow-name")]
        [DataRow("https://anywhere.com/path/workflow.yaml?x=4#y", "https://anywhere.com/path?x=4#y")]
        public void WorkflowYamlRemovedFromPath(string originalPath, string expectedPath)
        {
            var parts = UriParts.Parse(originalPath);
            parts.RemoveWorkflowYaml();

            Assert.AreEqual(expectedPath, parts.ToString());
        }
    }
}
