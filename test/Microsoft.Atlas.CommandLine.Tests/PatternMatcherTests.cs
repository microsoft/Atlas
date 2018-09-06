// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Atlas.CommandLine.Targets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests
{
    [TestClass]
    public class PatternMatcherTests
    {
        [TestMethod]
        public void SimplePatternMatchesPath()
        {
            var factory = new PatternMatcherFactory();
            var matcher = factory.Create(new[] { "foo" });

            Assert.IsTrue(matcher.IsMatch("/foo/"));
            Assert.IsFalse(matcher.IsMatch("/fooo/"));
            Assert.IsFalse(matcher.IsMatch("/fo/"));
        }

        [TestMethod]
        public void StarMatchesZeroOrMore()
        {
            var factory = new PatternMatcherFactory();
            var matcher = factory.Create(new[] { "f*o" });

            Assert.IsTrue(matcher.IsMatch("/foo/"));
            Assert.IsTrue(matcher.IsMatch("/fooo/"));
            Assert.IsTrue(matcher.IsMatch("/fo/"));
        }

        [TestMethod]
        public void MultiplePartsMatchUpToAndAnythingUnder()
        {
            var factory = new PatternMatcherFactory();
            var matcher = factory.Create(new[] { "foo/bar" });

            Assert.IsTrue(matcher.IsMatch("/"));
            Assert.IsTrue(matcher.IsMatch("/foo/"));
            Assert.IsTrue(matcher.IsMatch("/foo/bar/"));
            Assert.IsTrue(matcher.IsMatch("/foo/bar/quux/"));
            Assert.IsTrue(matcher.IsMatch("/foo/bar/quux/quad/"));

            Assert.IsFalse(matcher.IsMatch("/foo/baaz/"));
        }

        [TestMethod]
        [Ignore("TODO: wildcard layers combined with path stemming can give false positives.")]
        public void DoubleStarMatchesAnyNumberOfFolders()
        {
            var factory = new PatternMatcherFactory();
            var matcher = factory.Create(new[] { "foo/**/quad" });

            Assert.IsTrue(matcher.IsMatch("/"));
            Assert.IsTrue(matcher.IsMatch("/foo/"));
            Assert.IsTrue(matcher.IsMatch("/foo/bar/"));
            Assert.IsTrue(matcher.IsMatch("/foo/bar/quux/"));
            Assert.IsTrue(matcher.IsMatch("/foo/bar/quux/quad/"));

            Assert.IsFalse(matcher.IsMatch("/foo/baaz/"));
        }
    }
}
