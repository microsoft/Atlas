// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.Blueprints.Providers;
using Microsoft.Atlas.CommandLine.ConsoleOutput;
using Microsoft.Atlas.CommandLine.JsonClient;
using Microsoft.Atlas.CommandLine.Models.Workflow;
using Microsoft.Atlas.CommandLine.OAuth2;
using Microsoft.Atlas.CommandLine.Secrets;
using Microsoft.Atlas.CommandLine.Serialization;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public class WorkflowEngine : IWorkflowEngine
    {
        private readonly IValuesEngine _valuesEngine;
        private readonly IYamlSerializers _serializers;
        private readonly ISecretTracker _secretTracker;
        private readonly IConsole _console;
        private readonly IJsonHttpClientFactory _clientFactory;
        private readonly IBlueprintManager _blueprintManager;
        private readonly IWorkflowLoader _workflowLoader;
        private int _operationCount;

        public WorkflowEngine(
            IValuesEngine valuesEngine,
            IYamlSerializers serializers,
            ISecretTracker secretTracker,
            IConsole console,
            IJsonHttpClientFactory clientFactory,
            IBlueprintManager blueprintManager,
            IWorkflowLoader workflowLoader)
        {
            _valuesEngine = valuesEngine;
            _serializers = serializers;
            _secretTracker = secretTracker;
            _console = console;
            _clientFactory = clientFactory;
            _blueprintManager = blueprintManager;
            _workflowLoader = workflowLoader;
        }

        public async Task<object> ExecuteOperations(OperationContext parentContext, IList<WorkflowModel.Operation> operations)
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

        private async Task ExecuteOperation(OperationContext context)
        {
            var operation = context.Operation;

            if (context.Operation.@foreach == null)
            {
                await ExecuteOperationInner(context);
            }
            else
            {
                var foreachContexts = new List<OperationContext>();
                var foreachValuesInList = _valuesEngine.ProcessValuesForeachIn(operation.@foreach.values, context.Values);

                foreach (var foreachValuesIn in foreachValuesInList)
                {
                    var foreachContext = context.CreateChildContext(operation, MergeUtils.Merge(foreachValuesIn, context.Values));
                    foreachContexts.Add(foreachContext);
                    await ExecuteOperationInner(foreachContext);
                }

                var valuesOut = default(object);
                if (operation.@foreach.output != null)
                {
                    valuesOut = _valuesEngine.ProcessValuesForeachOut(operation.@foreach.output, foreachContexts.Select(foreachContext => foreachContext.Values).ToList());
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

        private async Task ExecuteOperationInner(OperationContext context)
        {
            var operation = context.Operation;

            if (operation.values != null)
            {
                context.AddValuesIn(_valuesEngine.ProcessValues(operation.values, context.Values));
            }

            var patternOkay = context.PatternMatcher.IsMatch(context.Path);

            var conditionOkay = operation.condition == null ? true : _valuesEngine.EvaluateToBoolean(operation.condition, context.Values);

            for (var shouldExecute = patternOkay && conditionOkay; shouldExecute; shouldExecute = await EvaluateRepeat(context))
            {
                var message = _valuesEngine.EvaluateToString(operation.message, context.Values);
                var write = _valuesEngine.EvaluateToString(operation.write, context.Values);
                var workflow = _valuesEngine.EvaluateToString(operation.workflow, context.Values);

                if (!string.IsNullOrEmpty(message))
                {
                    _console.WriteLine();
                    _console.WriteLine($"{new string(' ', context.Indent * 2)}- {message.Color(ConsoleColor.Cyan)}");
                }

                var debugPath = Path.Combine(context.ExecutionContext.OutputDirectory, "logs", $"{++_operationCount:000}-{new string('-', context.Indent * 2)}{new string((message ?? write ?? operation.request ?? operation.template ?? workflow ?? string.Empty).Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray())}.yaml");
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
                                { "workflow", workflow },
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
                                        interactive = context.ExecutionContext.IsInteractive
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

                                if (context.ExecutionContext.IsDryRun && method.Method != "GET")
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
                                    var targetPath = Path.Combine(context.ExecutionContext.OutputDirectory, write);

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

                            if (!string.IsNullOrEmpty(workflow))
                            {
                                var subBlueprint = await _blueprintManager.GetBlueprintPackageDependency(context.ExecutionContext.BlueprintPackage, workflow);
                                if (subBlueprint == null)
                                {
                                    throw new OperationException($"Unable to load sub-workflow {workflow}");
                                }

                                var(subTemplateEngine, subWorkflow, subModel) = _workflowLoader.Load(subBlueprint, context.ValuesIn, GenerateOutput);

                                var subContext = new ExecutionContext.Builder()
                                    .CopyFrom(context)
                                    .UseBlueprintPackage(subBlueprint)
                                    .UseTemplateEngine(subTemplateEngine)
                                    .SetValues(subModel)
                                    .Build();

                                var nestedResult = await ExecuteOperations(subContext, subWorkflow.operations);

                                outputContext = MergeUtils.Merge(new Dictionary<object, object> { { "result", nestedResult } }, outputContext);
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
                                var throwMessage = _valuesEngine.EvaluateToString(operation.@throw.message, context.Values);
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

        private void GenerateOutput(string arg1, Action<TextWriter> arg2)
        {
            // TODO: proper rendered file output location for sub-workflow
            // _console.WriteLine(arg1);
            // arg2(_console.Out);
        }

        private object ProcessValues(object source, object context)
        {
            return _valuesEngine.ProcessValues(source, context);
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
                var conditionIsTrue = _valuesEngine.EvaluateToBoolean(@catch.condition, mergedContext);
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

        private async Task<bool> EvaluateRepeat(OperationContext context)
        {
            var repeat = context.Operation.repeat;
            if (repeat == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(repeat.condition))
            {
                var conditionValue = _valuesEngine.EvaluateToBoolean(repeat.condition, context.Values);
                if (conditionValue == false)
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(repeat.timeout))
            {
                var timeout = XmlConvert.ToTimeSpan(repeat.timeout);
                if (DateTimeOffset.Now > context.OperationStartedUtc.Add(timeout))
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
    }
}
