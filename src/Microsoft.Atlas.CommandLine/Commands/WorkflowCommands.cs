// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.ConsoleOutput;
using Microsoft.Atlas.CommandLine.Execution;
using Microsoft.Atlas.CommandLine.Models.Workflow;
using Microsoft.Atlas.CommandLine.Secrets;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Atlas.CommandLine.Targets;
using Microsoft.Atlas.CommandLine.Templates;
using Microsoft.Atlas.CommandLine.Templates.FileSystems;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.Atlas.CommandLine.Commands
{
    public partial class WorkflowCommands
    {
        private readonly IWorkflowLoader _workflowLoader;
        private readonly IWorkflowEngine _workflowEngine;
        private readonly IValuesEngine _valuesEngine;
        private readonly ITemplateEngineFactory _templateEngineFactory;
        private readonly IYamlSerializers _serializers;
        private readonly IPatternMatcherFactory _patternMatcherFactory;
        private readonly ISecretTracker _secretTracker;
        private readonly IBlueprintManager _blueprintManager;
        private readonly IConsole _console;

        public WorkflowCommands(
            IWorkflowLoader workflowLoader,
            IWorkflowEngine workflowEngine,
            IValuesEngine valuesEngine,
            ITemplateEngineFactory templateEngineFactory,
            IYamlSerializers serializers,
            IPatternMatcherFactory patternMatcherFactory,
            ISecretTracker secretTracker,
            IBlueprintManager blueprintManager,
            IConsole console)
        {
            _workflowLoader = workflowLoader;
            _workflowEngine = workflowEngine;
            _valuesEngine = valuesEngine;
            _templateEngineFactory = templateEngineFactory;
            _serializers = serializers;
            _patternMatcherFactory = patternMatcherFactory;
            _secretTracker = secretTracker;
            _blueprintManager = blueprintManager;
            _console = console;
        }

        public CommandOption Values { get; set; }

        public CommandArgument Target { get; set; }

        public CommandOption OutputDirectory { get; set; }

        public CommandOption DryRun { get; set; }

        public CommandOption NonInteractive { get; set; }

        public CommandOption Set { get; set; }

        public CommandArgument Blueprint { get; set; }

        public CommandArgument Workflow { get; set; }

        public bool IsDryRun => DryRun?.HasValue() ?? false;

        public bool IsNonInteractive => NonInteractive?.HasValue() ?? false;

        public bool IsInteractive => !IsNonInteractive;

        public void GenerateOutput(string filename, Action<TextWriter> generate)
        {
            if (OutputDirectory.HasValue())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(OutputDirectory.Value(), filename)));

                using (var writer = _secretTracker.FilterTextWriter(File.CreateText(Path.Combine(OutputDirectory.Value(), filename))))
                {
                    generate(writer);
                }
            }
        }

        public Task<int> ExecuteGenerate() => ExecuteCore(generateOnly: true);

        public Task<int> ExecuteDeploy() => ExecuteCore(generateOnly: false);

        public async Task<int> ExecuteCore(bool generateOnly)
        {
            if (!OutputDirectory.HasValue())
            {
                OutputDirectory.TryParse($"_output");
            }

            var logsPath = Path.Combine(OutputDirectory.Required(), "logs");
            if (Directory.Exists(logsPath))
            {
                foreach (var file in Directory.EnumerateFiles(logsPath))
                {
                    File.Delete(file);
                }
            }

            var generatedPath = Path.Combine(OutputDirectory.Required(), "generated");
            if (Directory.Exists(generatedPath))
            {
                foreach (var file in Directory.EnumerateFiles(generatedPath, "*.*", new EnumerationOptions { RecurseSubdirectories = true }))
                {
                    File.Delete(file);
                }

                foreach (var folder in Directory.EnumerateDirectories(generatedPath))
                {
                    Directory.Delete(folder, recursive: true);
                }
            }

            var blueprint = await _blueprintManager.GetBlueprintPackage(Blueprint.Required());
            if (blueprint == null)
            {
                throw new ApplicationException($"Unable to locate blueprint {Blueprint.Required()}");
            }

            var eachValues = new List<object>();

            if (blueprint.Exists("values.yaml"))
            {
                using (var reader = blueprint.OpenText("values.yaml"))
                {
                    eachValues.Add(_serializers.YamlDeserializer.Deserialize(reader));
                }
            }

            var defaultValuesFiles =
                File.Exists("atlas-values.yaml") ? new[] { "atlas-values.yaml" } :
                File.Exists("values.yaml") ? new[] { "values.yaml" } :
                new string[0];

            foreach (var valuesFile in Values.OptionalMany(defaultValuesFiles))
            {
                using (var reader = File.OpenText(valuesFile))
                {
                    eachValues.Add(_serializers.YamlDeserializer.Deserialize(reader));
                }
            }

            if (Set.HasValue())
            {
                var setValues = new Dictionary<object, object>();

                foreach (var set in Set.Values)
                {
                    var parts = set.Split('=', 2);
                    if (parts.Length == 1)
                    {
                        throw new ApplicationException("Equal sign required when using the option --set name=value");
                    }

                    var name = parts[0];
                    var value = parts[1];
                    var segments = name.Split('.');
                    if (segments.Any(segment => string.IsNullOrEmpty(segment)))
                    {
                        throw new ApplicationException("Name must not have empty segments when using the option --set name=value");
                    }

                    var cursor = (IDictionary<object, object>)setValues;
                    foreach (var segment in segments.Reverse().Skip(1).Reverse())
                    {
                        if (cursor.TryGetValue(segment, out var child) && child is IDictionary<object, object>)
                        {
                            cursor = (IDictionary<object, object>)child;
                        }
                        else
                        {
                            child = new Dictionary<object, object>();
                            cursor[segment] = child;
                            cursor = (IDictionary<object, object>)child;
                        }
                    }

                    cursor[segments.Last()] = value;
                }

                eachValues.Add(setValues);
            }

            IDictionary<object, object> values = new Dictionary<object, object>();
            foreach (var addValues in eachValues)
            {
                values = (IDictionary<object, object>)MergeUtils.Merge(addValues, values) ?? values;
            }

            var(templateEngine, workflow, model) = _workflowLoader.Load(blueprint, values, GenerateOutput);

            if (generateOnly == false)
            {
                var patternMatcher = _patternMatcherFactory.Create(Target.Values.Any() ? Target.Values : new List<string>() { "/**" });

                var context = new ExecutionContext.Builder()
                    .UseBlueprintPackage(blueprint)
                    .UseTemplateEngine(templateEngine)
                    .UsePatternMatcher(patternMatcher)
                    .SetOutputDirectory(OutputDirectory.Required())
                    .SetNonInteractive(NonInteractive?.HasValue() ?? false)
                    .SetDryRun(DryRun?.HasValue() ?? false)
                    .SetValues(model)
                    .Build();

                context.AddValuesIn(_valuesEngine.ProcessValues(workflow.values, context.Values) ?? context.Values);

                var resultOut = await _workflowEngine.ExecuteOperations(context, workflow.operations);

                if (workflow.output != null)
                {
                    context.AddValuesOut(_valuesEngine.ProcessValues(workflow.output, MergeUtils.Merge(resultOut, context.Values) ?? context.Values));
                }
                else
                {
                    context.AddValuesOut(resultOut);
                }

                if (context.ValuesOut != null)
                {
                    GenerateOutput("output.yaml", writer => _serializers.YamlSerializer.Serialize(writer, context.ValuesOut));

                    using (var writer = _secretTracker.FilterTextWriter(_console.Out))
                    {
                        _serializers.YamlSerializer.Serialize(writer, context.ValuesOut);
                    }
                }
            }

            return 0;
        }
    }
}
