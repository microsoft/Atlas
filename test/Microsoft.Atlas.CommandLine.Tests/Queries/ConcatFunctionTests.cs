// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests.Queries
{
    [TestClass]
    public class ConcatFunctionTests : BaseFunctionTests
    {
        [TestMethod]
        public void StringsAreCombined()
        {
            var context = Yaml(@"
find: two
data:
- name: one
  value: 1
- name: two
  value: 2
- name: three
  value: 3");

            var result = Query.Search(@"concat('a','b','c')", context);

            Assert.AreEqual("abc", result);
        }

        [TestMethod]
        public void ExpressionsCanBeInterpolated()
        {
            var context = Yaml(@"
find: two
data:
- name: one
  value: 1
- name: two
  value: 2
- name: three
  value: 3");

            var result = Query.Search(@"concat('a', data[?value==`2`].name|[0], 'c')", context);

            Assert.AreEqual("atwoc", result);
        }

        [TestMethod]
        public void ConcatCanBuildSearchExpressions()
        {
            var context = Yaml(@"
find: one
data:
  one: 1
  two: 2
  three: 3
");

            var result = Query.Search(@"search(concat('data.""', find, '""'), @) ", context);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void ConcatCanBeEmpty()
        {
            var context = Yaml(@"
find: one
data:
  one: 1
  two: 2
  three: 3
");

            var result = Query.Search(@"concat()", context);

            Assert.AreEqual(string.Empty, result);
        }
    }
}
