// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public class RequestException : Exception
    {
        public RequestException()
        {
        }

        public RequestException(string message)
            : base(message)
        {
        }

        public RequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [YamlMember(Alias = "request")]
        public Models.Workflow.WorkflowModel.Request Request { get; internal set; }

        [YamlMember(Alias = "response")]
        public Models.Workflow.WorkflowModel.Response Response { get; internal set; }
    }
}
