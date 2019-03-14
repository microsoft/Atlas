// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Atlas.CommandLine.Targets;
using Microsoft.Atlas.CommandLine.Templates;
using static Microsoft.Atlas.CommandLine.Models.Workflow.WorkflowModel;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public class OperationContext
    {
        public OperationContext(ExecutionContext executionContext, object values, int indent = 0)
        {
            ExecutionContext = executionContext;
            Indent = indent;
            Path = "/";
            Values = values;
            OperationStartedUtc = DateTimeOffset.UtcNow;
        }

        public ExecutionContext ExecutionContext { get; }

        public ITemplateEngine TemplateEngine => ExecutionContext.TemplateEngine;

        public IPatternMatcher PatternMatcher => ExecutionContext.PatternMatcher;

        public DateTimeOffset OperationStartedUtc { get; private set; }

        public int Indent { get; private set; }

        public string Path { get; private set; }

        public Operation Operation { get; private set; }

        public object Values { get; private set; }

        public object ValuesIn { get; private set; }

        public object ValuesOut { get; private set; }

        public OperationContext CreateChildContext(Operation operation, object values)
        {
            return new OperationContext(ExecutionContext, values)
            {
                Indent = Indent + 1,
                Path = string.IsNullOrWhiteSpace(operation.target) ? Path : (Path + operation.target + "/"),
                Operation = operation,
            };
        }

        public void AddValuesIn(object valuesIn)
        {
            ValuesIn = valuesIn;
            Values = MergeUtils.Merge(ValuesIn, Values);
        }

        public void AddValuesOut(object valuesOut)
        {
            ValuesOut = valuesOut;
            Values = MergeUtils.Merge(ValuesOut, Values);
        }
    }
}
