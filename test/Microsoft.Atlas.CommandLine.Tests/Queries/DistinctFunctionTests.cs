// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Atlas.CommandLine.Queries;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests.Queries
{
    [TestClass]
    public class DistinctFunctionTests : BaseFunctionTests
    {
        [TestMethod]
        [DataRow("distinct(data)")]
        [DataRow("distinct_by(data, &@)")]
        public void DistinctEmptyArrayIsEmpty(string expression)
        {
            var context = Yaml("data: []");

            var result = (List<object>)Query.Search(expression, context);

            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        [DataRow("distinct(data)")]
        [DataRow("distinct_by(data, &@)")]
        public void NumbersAreDistinct(string expression)
        {
            var context = Yaml("data: [5,9,5]");

            var result = (List<object>)Query.Search(expression, context);

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Contains(5));
            Assert.IsTrue(result.Contains(9));
        }

        [TestMethod]
        [DataRow("distinct(data)")]
        [DataRow("distinct_by(data, &@)")]
        public void StringsAreDistinct(string expression)
        {
            var context = Yaml("data: [one,two,one]");

            var result = (List<object>)Query.Search(expression, context);

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Contains("one"));
            Assert.IsTrue(result.Contains("two"));
        }

        [TestMethod]
        [DataRow("distinct(data)")]
        [DataRow("distinct_by(data, &@)")]
        public void StringAndNumbersAreDifferent(string expression)
        {
            var context = Yaml("data: [5,5,'5',5,'5']");

            var result = (List<object>)Query.Search(expression, context);

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Contains("5"));
            Assert.IsTrue(result.Contains(5));
        }

        [TestMethod]
        [DataRow("distinct_by(data, &x)")]
        public void DistinctByFunctionForProperty(string expression)
        {
            var context = Yaml(@"
data: 
- x: one
  y: alpha
- x: two
  y: beta
- x: one
  y: gamma
");

            var result = (List<object>)Query.Search("distinct_by(data, &x)", context);
            var keys = result.Cast<IDictionary<object, object>>().Select(item => item["x"]).Cast<string>();

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(2, keys.Count());
            Assert.IsTrue(keys.Contains("one"));
            Assert.IsTrue(keys.Contains("two"));
        }
    }
}
