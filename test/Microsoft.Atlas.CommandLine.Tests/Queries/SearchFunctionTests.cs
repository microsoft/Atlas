// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests.Queries
{
    [TestClass]
    public class SearchFunctionTests : BaseFunctionTests
    {
        [TestMethod]
        public void SearchRunsExpressionOnInput()
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

            var result = Query.Search(@"search('data[?name==\'three\']|[0].value', @) ", context);

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void ExpressionCanBeBuiltToLookupArrayItem()
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

            var result = Query.Search(@"search(concat('data[?name==\'', find, '\'] | [0].value'), @) ", context);

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void ExpressionCanBeBuiltToLookupObjectProperty()
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
        public void SearchResultCanBeNull()
        {
            var context = Yaml(@"
find: one
data:
  one: 1
  two: 2
  three: 3
");

            var result = Query.Search(@"search('data.four', @)", context);

            Assert.AreEqual(null, result);
        }


        [TestMethod]
        public void SearchResultCanBeAnObject()
        {
            var context = Yaml(@"
find: one
data:
  one: 1
  two: 2
  three: 3
");

            var result = (IDictionary<object, object>)Query.Search(@"search('{four: sum([data.one, data.three])}', @)", context);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(4, result["four"]);
        }
    }
}
