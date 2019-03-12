// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using YamlDotNet.Serialization;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable SA1516 // Elements must be separated by blank line
#pragma warning disable SA1649 // File name must match first type name

namespace Microsoft.Atlas.CommandLine.Models.Workflow
{
    public class WorkflowModel
    {
        public object values { get; set; }
        public object output { get; set; }
        public IList<Operation> operations { get; set; }

        public class Operation
        {
            public string message { get; set; }
            public string target { get; set; }
            public string condition { get; set; }

            [YamlMember(Alias = "foreach")]
            public Foreach @foreach { get; set; }
            public Repeat repeat { get; set; }
            public object values { get; set; }
            public object output { get; set; }

            [YamlMember(Alias = "throw")]
            public Throw @throw { get; set; }

            [YamlMember(Alias = "catch")]
            public Catch @catch { get; set; }

            public string request { get; set; }
            public string template { get; set; }
            public string workflow { get; set; }
            public string write { get; set; }
            public IList<Operation> operations { get; set; }
        }

        public class Request
        {
            public string method { get; set; }
            public string url { get; set; }
            public IDictionary<string, object> query { get; set; }
            public IDictionary<object, object> headers { get; set; }
            public Auth auth { get; set; }
            public object body { get; set; }
            public string secret { get; set; }

            public class Auth
            {
                public string tenant { get; set; }
                public string resource { get; set; }
                public string client { get; set; }
            }
        }

        public class Response
        {
            public int status { get; set; }
            public IDictionary<object, object> headers { get; set; }
            public object body { get; set; }
        }

        public class Foreach
        {
            public object values { get; set; }
            public object output { get; set; }
        }

        public class Repeat
        {
            public string condition { get; set; }
            public string timeout { get; set; }
            public string delay { get; set; }
        }

        public class Throw
        {
            public string message { get; set; }
            public object details { get; set; }
        }

        public class Catch
        {
            public string condition { get; set; }
            public object output { get; set; }
        }
    }
}
