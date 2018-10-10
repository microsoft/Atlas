// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using YamlDotNet.Serialization;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public class OperationException : Exception
    {
        // Summary:
        //     Initializes a new instance of the Microsoft.Atlas.CommandLine.Execution.OperationException class.
        public OperationException()
        {
        }

        // Summary:
        //     Initializes a new instance of the Microsoft.Atlas.CommandLine.Execution.OperationException class with a specified
        //     error message.
        //
        // Parameters:
        //   message:
        //     A message that describes the error.
        public OperationException(string message)
            : base(message)
        {
        }

        // Summary:
        //     Initializes a new instance of the System.ApplicationException class with a specified
        //     error message and a reference to the inner exception that is the cause of this
        //     exception.
        //
        // Parameters:
        //   message:
        //     The error message that explains the reason for the exception.
        //
        //   innerException:
        //     The exception that is the cause of the current exception. If the innerException
        //     parameter is not a null reference, the current exception is raised in a catch
        //     block that handles the inner exception.
        public OperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [YamlMember(Alias = "details")]
        public object Details { get; internal set; }
    }
}
