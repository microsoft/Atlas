// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.ConsoleOutput;
using Microsoft.Atlas.CommandLine.Execution;
using Microsoft.Atlas.CommandLine.JsonClient;
using Microsoft.Atlas.CommandLine.Models.Workflow;
using Microsoft.Atlas.CommandLine.OAuth2;
using Microsoft.Atlas.CommandLine.Queries;
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
        private readonly IJsonHttpClientFactory _clientFactory;
        private readonly ITemplateEngineFactory _templateEngineFactory;
        private readonly IYamlSerializers _serializers;
        private readonly IJmesPathQuery _jmesPathQuery;
        private readonly IPatternMatcherFactory _patternMatcherFactory;
        private readonly ISecretTracker _secretTracker;
        private readonly IBlueprintManager _blueprintManager;
        private readonly IConsole _console;
        private int _operationCount;

        public WorkflowCommands(
            IJsonHttpClientFactory clientFactory,
            ITemplateEngineFactory templateEngineFactory,
            IYamlSerializers serializers,
            IJmesPathQuery jmesPathQuery,
            IPatternMatcherFactory patternMatcherFactory,
            ISecretTracker secretTracker,
            IBlueprintManager blueprintManager,
            IConsole console)
        {
            _clientFactory = clientFactory;
            _templateEngineFactory = templateEngineFactory;
            _serializers = serializers;
            _jmesPathQuery = jmesPathQuery;
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
                Directory.CreateDirectory(OutputDirectory.Value());

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

            var blueprint = _blueprintManager.GetBlueprintPackage(Blueprint.Required());
            if (blueprint == null)
            {
                throw new ApplicationException($"Unable to locate blueprint {Blueprint.Required()}");
            }

            var templateEngine = _templateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new BlueprintPackageFileSystem(blueprint)
            });

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

            object model;

            var modelTemplate = "model.yaml";
            var modelExists = blueprint.Exists(modelTemplate);
            if (modelExists)
            {
                model = templateEngine.Render<object>(modelTemplate, values);
            }
            else
            {
                model = values;
            }

            var workflowTemplate = "workflow.yaml";
            var workflowContents = new StringBuilder();
            using (var workflowWriter = new StringWriter(workflowContents))
            {
                templateEngine.Render(workflowTemplate, model, workflowWriter);
            }

            // NOTE: the workflow is rendered BEFORE writing these output files because it may contain
            // calls to the "secret" helper which will redact tokens that might have been provided in values

            // write values to output folder
            GenerateOutput("values.yaml", writer => _serializers.YamlSerializer.Serialize(writer, values));

            if (modelExists)
            {
                // write normalized values to output folder
                GenerateOutput(modelTemplate, writer => templateEngine.Render(modelTemplate, values, writer));
            }

            // write workflow to output folder
            GenerateOutput("workflow.yaml", writer => writer.Write(workflowContents.ToString()));

            var workflow = _serializers.YamlDeserializer.Deserialize<WorkflowModel>(new StringReader(workflowContents.ToString()));

            var patternMatcher = _patternMatcherFactory.Create(Target.Values.Any() ? Target.Values : new List<string>() { "/**" });

            if (generateOnly == false)
            {
                var context = new ExecutionContext(templateEngine, patternMatcher, null);
                context.AddValuesIn(ProcessValues(workflow.values, context.Values));

                var resultOut = await ExecuteOperations(context, workflow.operations);

                if (workflow.output != null)
                {
                    context.AddValuesOut(ProcessValues(workflow.output, MergeUtils.Merge(resultOut, context.Values) ?? context.Values));
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

        public async Task<object> ExecuteOperations(ExecutionContext parentContext, IList<WorkflowModel.Operation> operations)
        {
            var cumulativeValues = parentContext.Values;
            object resultOut = null;

            foreach (var operation in operations)
            {
                var childContext = parentContext.CreateChildContext(operation, cumulativeValues);

                await ExecuteOperation(childContext);

                if (childContext.ValuesOut != null)
                {
                    cumulativeValues = MergeUtils.Merge(childContext.ValuesOut, cumulativeValues);
                    resultOut = MergeUtils.Merge(childContext.ValuesOut, resultOut);
                }
            }

            return resultOut;
        }

        private async Task ExecuteOperation(ExecutionContext context)
        {
            var operation = context.Operation;

            if (operation.values != null)
            {
                context.AddValuesIn(ProcessValues(operation.values, context.Values));
            }

            var patternOkay = context.PatternMatcher.IsMatch(context.Path);

            var message = ConvertToString(ProcessValues(operation.message, context.Values));
	        var write = ConvertToString(ProcessValues(operation.write, context.Values));

            var conditionOkay = true;
            if (!string.IsNullOrEmpty(operation.condition))
            {
                var conditionResult = _jmesPathQuery.Search(operation.condition, context.Values);
                conditionOkay = ConditionBoolean(conditionResult);
            }

            for (var shouldExecute = patternOkay && conditionOkay; shouldExecute; shouldExecute = await EvaluateRepeat(context))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    _console.WriteLine();
                    _console.WriteLine($"{new string(' ', context.Indent * 2)}- {message.Color(ConsoleColor.Cyan)}");
                }

                var debugPath = Path.Combine(OutputDirectory.Required(), "logs", $"{++_operationCount:000}-{new string('-', context.Indent * 2)}{new string((message ?? operation.write ?? operation.request ?? operation.template ?? string.Empty).Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray())}.yaml");
                Directory.CreateDirectory(Path.GetDirectoryName(debugPath));
                using (var writer = _secretTracker.FilterTextWriter(File.CreateText(debugPath)))
                {
                    var logentry = new Dictionary<object, object>
                    {
                        {
                            "operation",
                            new Dictionary<object, object>
                            {
                                { "message", message },
                                { "target", operation.target },
                                { "condition", operation.condition },
                                { "repeat", operation.repeat },
                                { "request", operation.request },
                                { "template", operation.template },
                                { "write", write },
                            }
                        },
                        { "valuesIn", context.ValuesIn },
                        { "valuesOut", context.ValuesOut },
                        { "request", null },
                        { "response", null },
                        { "cumulativeValues", context.Values },
                    };
                    try
                    {
                        object result = null;

                        // First special type of operation - executing a request
                        if (!string.IsNullOrWhiteSpace(operation.request))
                        {
                            WorkflowModel.Request request = context.TemplateEngine.Render<WorkflowModel.Request>(
                                operation.request,
                                context.Values);

                            logentry["request"] = request;

                            HttpAuthentication auth = null;
                            if (request.auth != null)
                            {
                                // TODO: remove these defaults
                                auth = new HttpAuthentication
                                {
                                    tenant = request?.auth?.tenant ?? "common",
                                    resourceId = request?.auth?.resource ?? "499b84ac-1321-427f-aa17-267ca6975798",
                                    clientId = request?.auth?.client ?? "e8f3cc86-b3b2-4ebb-867c-9c314925b384",
                                    interactive = IsInteractive
                                };
                            }

                            var client = _clientFactory.Create(auth);

                            var method = new HttpMethod(request.method ?? "GET");
                            if (IsDryRun && method.Method != "GET")
                            {
                                _console.WriteLine($"Skipping {method.Method.ToString().Color(ConsoleColor.DarkYellow)} {request.url}");
                            }
                            else
                            {
                                try
                                {
                                    var jsonRequest = new JsonRequest
                                    {
                                        method = method,
                                        url = request.url,
                                        headers = request.headers,
                                        body = request.body,
                                        secret = request.secret,
                                    };

                                    var jsonResponse = await client.SendAsync(jsonRequest);
                                    if ((int)jsonResponse.status >= 400)
                                    {
                                        throw new ApplicationException($"Request failed with status code {jsonResponse.status}");
                                    }

                                    result = new WorkflowModel.Response
                                    {
                                        status = (int)jsonResponse.status,
                                        headers = jsonResponse.headers,
                                        body = jsonResponse.body,
                                    };

                                    logentry["response"] = result;
                                }
                                catch
                                {
                                    // TODO - retry logic here?
                                    throw;
                                }
                            }
                        }

                        // Second special type of operation - rendering a template
                        if (!string.IsNullOrWhiteSpace(operation.template))
                        {
                            if (!string.IsNullOrEmpty(write))
                            {
                                var targetPath = Path.Combine(OutputDirectory.Required(), write);

                                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                                using (var targetWriter = File.CreateText(targetPath))
                                {
                                    if (!string.IsNullOrWhiteSpace(operation.template))
                                    {
                                        context.TemplateEngine.Render(operation.template, context.Values, targetWriter);
                                    }
                                }
                            }
                            else
                            {
                                result = context.TemplateEngine.Render<object>(operation.template, context.Values);
                            }
                        }

                        // Third special type of operation - nested operations
                        if (operation.operations != null)
                        {
                            result = await ExecuteOperations(context, operation.operations);
                        }

                        // If output is specifically stated - use it to query
                        if (operation.output != null)
                        {
                            if (operation.operations != null)
                            {
                                // for nested operations, output expressions can pull in the current operation's cumulative values as well
                                context.AddValuesOut(ProcessValues(operation.output, MergeUtils.Merge(result, context.Values) ?? context.Values));
                            }
                            else if (result != null)
                            {
                                // for request and template operations, the current operation result is a well-known property to avoid collisions
                                var merged = MergeUtils.Merge(new Dictionary<object, object> { { "result", result } }, context.Values);

                                context.AddValuesOut(ProcessValues(operation.output, merged));
                            }
                            else
                            {
                                // there are no values coming out of this operation - output queries are based only on cumulative values
                                context.AddValuesOut(ProcessValues(operation.output, context.Values));
                            }
                        }

                        if (operation.@throw != null)
                        {
                            var throwMessage = ConvertToString(ProcessValues(operation.@throw.message, context.Values));
                            throwMessage = string.IsNullOrEmpty(throwMessage) ? message : throwMessage;

                            var throwDetails = ProcessValues(operation.@throw.details, context.Values);

                            _console.WriteLine(throwMessage.Color(ConsoleColor.DarkRed));
                            if (throwDetails != null)
                            {
                                _console.WriteLine(_serializers.YamlSerializer.Serialize(throwDetails).Color(ConsoleColor.DarkRed));
                            }

                            throw new OperationException(string.IsNullOrEmpty(throwMessage) ? message : throwMessage)
                            {
                                Details = throwDetails
                            };
                        }

                        // otherwise if output is unstated, and there are nested operations with output - those flows back as effective output
                        if (operation.output == null && operation.operations != null && result != null)
                        {
                            context.AddValuesOut(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        logentry["error"] = new Dictionary<object, object>
                        {
                            { "type", ex.GetType().FullName },
                            { "message", ex.Message },
                            { "stack", ex.StackTrace },
                        };

                        throw;
                    }
                    finally
                    {
                        logentry["valuesIn"] = context?.ValuesIn;
                        logentry["valuesOut"] = context?.ValuesOut;
                        logentry["cumulativeValues"] = context?.Values;
                        _serializers.YamlSerializer.Serialize(writer, logentry);
                    }
                }
            }
        }

        private async Task<bool> EvaluateRepeat(ExecutionContext context)
        {
            var repeat = context.Operation.repeat;
            if (repeat == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(repeat.condition))
            {
                var conditionResult = _jmesPathQuery.Search(repeat.condition, context.Values);
                if (ConditionBoolean(conditionResult) == false)
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(repeat.timeout))
            {
                var timeout = XmlConvert.ToTimeSpan(repeat.timeout);
                if (DateTimeOffset.Now > context.StartedUtc.Add(timeout))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(repeat.delay))
            {
                var delay = XmlConvert.ToTimeSpan(repeat.delay);
                await Task.Delay(delay);
            }

            return true;
        }

        private object ProcessValues(object source, object context)
        {
            if (source is IDictionary<object, object> sourceDictionary)
            {
                var output = new Dictionary<object, object>();
                foreach (var kv in sourceDictionary)
                {
                    var isQuery = IsQuery(kv);
                    if (isQuery)
                    {
                        output[kv.Key.ToString().TrimEnd('?')] = ProcessQueries(kv.Value, context);
                    }
                    else
                    {
                        output[kv.Key] = ProcessValues(kv.Value, context);
                    }
                }

                return output;
            }

            if (source is IList<object> sourceList)
            {
                return sourceList.Select(value => ProcessValues(value, context)).ToList();
            }

            if (source is string sourceString)
            {
                if (sourceString.StartsWith('(') && sourceString.EndsWith(')'))
                {
                    return ProcessQueries(sourceString.Substring(1, sourceString.Length - 2), context);
                }
            }

            return source;
        }

        private object ProcessQueries(object source, object context)
        {
            if (source is IDictionary<object, object> sourceDictionary)
            {
                var output = new Dictionary<object, object>();
                foreach (var kv in sourceDictionary)
                {
                    output[kv.Key] = ProcessQueries(kv.Value, context);
                }

                return output;
            }

            if (source is IList<object> sourceList)
            {
                return sourceList.Select(value => ProcessQueries(value, context)).ToList();
            }

            if (source is string sourceString)
            {
                return _jmesPathQuery.Search(sourceString, context);
            }

            throw new ApplicationException($"Unexpected value type {source.GetType().FullName} {source}");
        }

        private bool IsQuery(KeyValuePair<object, object> kv)
        {
            return kv.Key?.ToString()?.EndsWith('?') ?? false;
        }

        private string ConvertToString(object source)
        {
            if (source is IDictionary<object, object> sourceDictionary)
            {
                return _serializers.JsonSerializer.Serialize(sourceDictionary).TrimEnd('\r', '\n');
            }

            if (source is IList<object> sourceList)
            {
                return string.Concat(sourceList.Select(ConvertToString));
            }

            return source?.ToString();
        }

        private bool ConditionBoolean(object condition)
        {
            if (condition == null)
            {
                return false;
            }

            if (condition is bool)
            {
                return (bool)condition;
            }

            if (condition is string)
            {
                return Convert.ToBoolean(condition);
            }

            if (condition is IEnumerable)
            {
                return ((IEnumerable)condition).Cast<object>().Any();
            }

            return Convert.ToBoolean(condition);
        }

        private void AdjustReleaseDefinition(dynamic rdRequestDynamic, dynamic rdExistingDynamic)
        {
            // TODO: assign .owner.id from .owner.uniqueName using vsts user/group lookup api
            var rank = 0;
            foreach (var requestEnvironment in rdRequestDynamic.environments)
            {
                requestEnvironment.rank = ++rank;

                foreach (var existingEnvironment in rdExistingDynamic.environments)
                {
                    if (string.Equals(requestEnvironment.name, existingEnvironment.name, StringComparison.Ordinal))
                    {
                        requestEnvironment.id = existingEnvironment.id;
                        requestEnvironment.deployStep.id = existingEnvironment.deployStep.id;

                        foreach (var requestApproval in requestEnvironment.preDeployApprovals.approvals)
                        {
                            foreach (var existingApproval in existingEnvironment.preDeployApprovals.approvals)
                            {
                                if (requestApproval.rank == existingApproval.rank)
                                {
                                    requestApproval.id = existingApproval.id;
                                }
                            }
                        }

                        foreach (var requestApproval in requestEnvironment.postDeployApprovals.approvals)
                        {
                            foreach (var existingApproval in existingEnvironment.postDeployApprovals.approvals)
                            {
                                if (requestApproval.rank == existingApproval.rank)
                                {
                                    requestApproval.id = existingApproval.id;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
