// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Atlas.CommandLine.Queries
{
    [Serializable]
    internal class QueryException : Exception
    {
        public QueryException()
        {
        }

        public QueryException(string message)
            : base(message)
        {
        }

        public QueryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected QueryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Expression { get; set; }
    }
}
