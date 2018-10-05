// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Atlas.CommandLine.Queries;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests.Queries
{
    [TestClass]
    public class BitsFunctionTests
    {
        [TestMethod]
        public void BitsTurnsNumberIntoArray()
        {
            var query = new JmesPathQuery(new YamlSerializers());

            var result = (List<object>)query.Search("bits(`9`)", null);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(8, result[1]);
        }

        [TestMethod]
        public void MultipleNumbersAreBitwiseOredTogether()
        {
            var query = new JmesPathQuery(new YamlSerializers());

            var result = (List<object>)query.Search("bits(`9`, `5`)", null);

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(8, result[2]);
        }

        [TestMethod]
        public void NumbersCanComeFromContext()
        {
            var query = new JmesPathQuery(new YamlSerializers());
            var context = new Dictionary<string, object>
            {
                { "x", 18 }
            };

            var result = (List<object>)query.Search("bits(x)", context);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(16, result[1]);
        }
    }
}
