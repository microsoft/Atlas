// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.Targets;
using Microsoft.Atlas.CommandLine.Templates;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public class ExecutionContext
    {
        public IBlueprintPackage BlueprintPackage { get; private set; }

        public ITemplateEngine TemplateEngine { get; private set; }

        public IPatternMatcher PatternMatcher { get; private set; }

        public DateTimeOffset ExecutionStartedUtc { get; private set; } = DateTimeOffset.UtcNow;

        public string OutputDirectory { get; private set; }

        public bool IsDryRun { get; private set; }

        public bool IsInteractive { get; private set; }

        public class Builder
        {
            private readonly ExecutionContext _context = new ExecutionContext();
            private object _values;
            private int _indent;

            public OperationContext Build() => new OperationContext(_context, _values, _indent);

            public Builder CopyFrom(OperationContext context)
            {
                _context.BlueprintPackage = context.ExecutionContext.BlueprintPackage;
                _context.TemplateEngine = context.ExecutionContext.TemplateEngine;
                _context.PatternMatcher = context.ExecutionContext.PatternMatcher;
                _context.OutputDirectory = context.ExecutionContext.OutputDirectory;
                _context.IsDryRun = context.ExecutionContext.IsDryRun;
                _context.IsInteractive = context.ExecutionContext.IsInteractive;
                _values = context.Values;
                _indent = context.Indent;
                return this;
            }

            public Builder UseTemplateEngine(ITemplateEngine templateEngine)
            {
                _context.TemplateEngine = templateEngine;
                return this;
            }

            public Builder UsePatternMatcher(IPatternMatcher patternMatcher)
            {
                _context.PatternMatcher = patternMatcher;
                return this;
            }

            public Builder SetOutputDirectory(string outputDirectory)
            {
                _context.OutputDirectory = outputDirectory;
                return this;
            }

            public Builder SetDryRun(bool isDryRun)
            {
                _context.IsDryRun = isDryRun;
                return this;
            }

            public Builder SetNonInteractive(bool isNonInteractive)
            {
                _context.IsInteractive = !isNonInteractive;
                return this;
            }

            public Builder SetValues(object values)
            {
                _values = values;
                return this;
            }

            public Builder SetIndent(int indent)
            {
                _indent = indent;
                return this;
            }

            public Builder UseBlueprintPackage(IBlueprintPackage blueprint)
            {
                _context.BlueprintPackage = blueprint;
                return this;
            }
        }
    }
}
