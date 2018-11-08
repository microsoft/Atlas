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
using Microsoft.Atlas.CommandLine.Blueprints.Providers;
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

            foreach (var generatedFile in blueprint.GetGeneratedPaths())
            {
                using (var generatedContent = blueprint.OpenText(generatedFile))
                {
                    GenerateOutput($"generated/{generatedFile}", writer => writer.Write(generatedContent.ReadToEnd()));
                }
            }

            if (generateOnly == false)
            {
                var patternMatcher = _patternMatcherFactory.Create(Target.Values.Any() ? Target.Values : new List<string>() { "/**" });

                var context = new ExecutionContext(templateEngine, patternMatcher, values);
                context.AddValuesIn(ProcessValues(workflow.values, context.Values) ?? context.Values);

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

            if (context.Operation.@foreach == null)
            {
                await ExecuteOperationInner(context);
            }
            else
            {
                var foreachContexts = new List<ExecutionContext>();
                var foreachValuesInList = ProcessValuesForeachIn(operation.@foreach.values, context.Values);

                foreach (var foreachValuesIn in foreachValuesInList)
                {
                    var foreachContext = context.CreateChildContext(operation, MergeUtils.Merge(foreachValuesIn, context.Values));
                    foreachContexts.Add(foreachContext);
                    await ExecuteOperationInner(foreachContext);
                }

                var valuesOut = default(object);
                if (operation.@foreach.output != null)
                {
                    valuesOut = ProcessValuesForeachOut(operation.@foreach.output, foreachContexts.Select(foreachContext => foreachContext.Values).ToList());
                }
                else
                {
                    foreach (var foreachValuesOut in foreachContexts.Select(foreachContext => foreachContext.ValuesOut))
                    {
                        valuesOut = MergeUtils.Merge(foreachValuesOut, valuesOut);
                    }
                }

                context.AddValuesOut(valuesOut);
            }
        }

        private async Task ExecuteOperationInner(ExecutionContext context)
        {
            var operation = context.Operation;

            if (operation.values != null)
            {
                context.AddValuesIn(ProcessValues(operation.values, context.Values));
            }

            var patternOkay = context.PatternMatcher.IsMatch(context.Path);

            var conditionOkay = true;
            if (!string.IsNullOrEmpty(operation.condition))
            {
                var conditionResult = _jmesPathQuery.Search(operation.condition, context.Values);
                conditionOkay = ConditionBoolean(conditionResult);
            }

            for (var shouldExecute = patternOkay && conditionOkay; shouldExecute; shouldExecute = await EvaluateRepeat(context))
            {
                var message = ConvertToString(ProcessValues(operation.message, context.Values));
                var write = ConvertToString(ProcessValues(operation.write, context.Values));

                if (!string.IsNullOrEmpty(message))
                {
                    _console.WriteLine();
                    _console.WriteLine($"{new string(' ', context.Indent * 2)}- {message.Color(ConsoleColor.Cyan)}");
                }

                var debugPath = Path.Combine(OutputDirectory.Required(), "logs", $"{++_operationCount:000}-{new string('-', context.Indent * 2)}{new string((message ?? write ?? operation.request ?? operation.template ?? string.Empty).Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray())}.yaml");
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
                        // object result = null;
                        object outputContext = context.Values;

                        try
                        {
                            // First special type of operation - executing a request
                            if (!string.IsNullOrWhiteSpace(operation.request))
                            {
                                var request = context.TemplateEngine.Render<WorkflowModel.Request>(
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

                                var parts = UriParts.Parse(request.url);
                                foreach (var query in request.query ?? Enumerable.Empty<KeyValuePair<string, object>>())
                                {
                                    parts.Query = parts.Query.Add(query.Key, Convert.ToString(query.Value));
                                }

                                var url = parts.ToString();

                                if (IsDryRun && method.Method != "GET")
                                {
                                    _console.WriteLine($"Skipping {method.Method.ToString().Color(ConsoleColor.DarkYellow)} {request.url}");
                                }
                                else
                                {
                                    var jsonRequest = new JsonRequest
                                    {
                                        method = method,
                                        url = url,
                                        headers = request.headers,
                                        body = request.body,
                                        secret = request.secret,
                                    };

                                    var jsonResponse = await client.SendAsync(jsonRequest);

                                    var response = new WorkflowModel.Response
                                    {
                                        status = (int)jsonResponse.status,
                                        headers = jsonResponse.headers,
                                        body = jsonResponse.body,
                                    };

                                    logentry["response"] = response;

                                    outputContext = MergeUtils.Merge(new Dictionary<object, object> { { "result", response } }, outputContext);

                                    if (response.status >= 400)
                                    {
                                        var error = new RequestException($"Request failed with status code {jsonResponse.status}")
                                        {
                                            Request = request,
                                            Response = response,
                                        };
                                        throw error;
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

                                if (operation.output != null)
                                {
                                    var templateResult = context.TemplateEngine.Render<object>(operation.template, context.Values);

                                    outputContext = MergeUtils.Merge(new Dictionary<object, object> { { "result", templateResult } }, outputContext);
                                }
                            }

                            // Third special type of operation - nested operations
                            if (operation.operations != null)
                            {
                                var nestedResult = await ExecuteOperations(context, operation.operations);

                                if (operation.output == null)
                                {
                                    // if output is unstated, and there are nested operations with output - those flows back as effective output
                                    context.AddValuesOut(nestedResult);
                                }
                                else
                                {
                                    // if output is stated, nested operations with output are visible to output queries
                                    outputContext = MergeUtils.Merge(nestedResult, context.Values) ?? context.Values;
                                }
                            }

                            // If output is specifically stated - use it to query
                            if (operation.output != null)
                            {
                                context.AddValuesOut(ProcessValues(operation.output, outputContext));
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
                        }
                        catch (Exception ex) when (CatchCondition(ex, operation.@catch, outputContext))
                        {
                            if (operation.@catch.output != null)
                            {
                                var mergedContext = MergeError(ex, outputContext);
                                var catchDetails = ProcessValues(operation.@catch.output, mergedContext);
                                context.AddValuesOut(catchDetails);
                            }
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

        private bool CatchCondition(Exception ex, WorkflowModel.Catch @catch, object outputContext)
        {
            try
            {
                if (@catch == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(@catch.condition))
                {
                    return true;
                }

                var mergedContext = MergeError(ex, outputContext);
                var conditionResult = _jmesPathQuery.Search(@catch.condition, mergedContext);
                var conditionIsTrue = ConditionBoolean(conditionResult);
                return conditionIsTrue;
            }
            catch (Exception ex2)
            {
                Console.Error.WriteLine($"{"Fatal".Color(ConsoleColor.Red)}: exception processing catch condition: {ex2.Message.Color(ConsoleColor.DarkRed)}");
                throw;
            }
        }

        private object MergeError(Exception exception, object context)
        {
            var yaml = _serializers.YamlSerializer.Serialize(new { error = exception });
            var error = _serializers.YamlDeserializer.Deserialize<object>($@"
{yaml}
  type: 
    name: {exception.GetType().Name}
    fullName: {exception.GetType().FullName}
");
            var mergedContext = MergeUtils.Merge(error, context);
            return mergedContext;
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
            return ProcessValuesRecursive(source, new[] { context }, promoteArrays: false);
        }

        private IList<object> ProcessValuesForeachIn(object source, object context)
        {
            var result = ProcessValuesRecursive(source, new[] { context }, promoteArrays: true);
            if (result is IList<object> resultList)
            {
                return resultList;
            }

            throw new ApplicationException("Foreach values contained no arrays");
        }

        private object ProcessValuesForeachOut(object source, IList<object> contexts)
        {
            return ProcessValuesRecursive(source, contexts, promoteArrays: false);
        }

        private object ProcessValuesRecursive(object source, IList<object> contexts, bool promoteArrays)
        {
            if (source is IDictionary<object, object> sourceDictionary)
            {
                var arrayIsPromoting = false;
                var arrayLength = 0;

                var output = new Dictionary<object, object>();
                foreach (var kv in sourceDictionary)
                {
                    var result = ProcessValuesRecursive(kv.Value, contexts, promoteArrays: promoteArrays);
                    output[kv.Key] = result;

                    if (promoteArrays && result is IList<object> resultArray)
                    {
                        if (!arrayIsPromoting)
                        {
                            arrayIsPromoting = true;
                            arrayLength = resultArray.Count();
                        }
                        else
                        {
                            if (arrayLength != resultArray.Count())
                            {
                                throw new ApplicationException("Foreach arrays must all be same size");
                            }
                        }
                    }
                }

                if (arrayIsPromoting)
                {
                    var arrayOutput = new List<object>();
                    for (var index = 0; index < arrayLength; ++index)
                    {
                        var arrayItem = output.ToDictionary(kv => kv.Key, kv => kv.Value is IList<object> valueArray ? valueArray[index] : kv.Value);
                        arrayOutput.Add(arrayItem);
                    }

                    return arrayOutput;
                }

                return output;
            }

            if (source is IList<object> sourceList)
            {
                return sourceList.Select(value => ProcessValuesRecursive(value, contexts, promoteArrays: promoteArrays)).ToList();
            }

            if (source is string sourceString)
            {
                if (sourceString.StartsWith('(') && sourceString.EndsWith(')'))
                {
                    var expression = sourceString.Substring(1, sourceString.Length - 2);
                    var mergedResult = default(object);
                    foreach (var context in contexts)
                    {
                        var result = _jmesPathQuery.Search(sourceString, context);
                        if (result is IList<object> resultList && mergedResult is IList<object> mergedList)
                        {
                            mergedResult = mergedList.Concat(resultList).ToList();
                        }
                        else
                        {
                            mergedResult = MergeUtils.Merge(result, mergedResult);
                        }
                    }

                    return mergedResult;
                }
            }

            return source;
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
