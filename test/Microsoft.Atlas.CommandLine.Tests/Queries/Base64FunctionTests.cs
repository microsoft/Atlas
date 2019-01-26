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
    public class Base64FunctionTests : BaseFunctionTests
    {
        [TestMethod]
        [DataRow("base64_encode('alpha')", "YWxwaGE=")]
        [DataRow("base64_encode(data)", "YmV0YQ==")]
        public void StringDataIsEncodedToBase64String(string expression, string expected)
        {
            var context = Yaml("data: beta");

            var result = (string)Query.Search(expression, context);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("base64_decode('YWxwaGE=')", "alpha")]
        [DataRow("base64_decode(data)", "beta")]
        public void Base64StringIsDecodedToString(string expression, string expected)
        {
            var context = Yaml("data: YmV0YQ==");

            var result = (string)Query.Search(expression, context);

            Assert.AreEqual(expected, result);
        }
    }
}
