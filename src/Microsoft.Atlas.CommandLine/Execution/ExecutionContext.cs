// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Atlas.CommandLine.Targets;
using Microsoft.Atlas.CommandLine.Templates;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public class ExecutionContext
    {
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

            public OperationContext Build() => new OperationContext(_context, _values);

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
        }
    }
}
