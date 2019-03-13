// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Models.Workflow;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public interface IWorkflowEngine
    {
        Task<object> ExecuteOperations(OperationContext parentContext, IList<WorkflowModel.Operation> operations);
    }
}
