// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Atlas.CommandLine.Queries;
using Microsoft.Atlas.CommandLine.Serialization;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public class ValuesEngine : IValuesEngine
    {
        private readonly IJmesPathQuery _jmesPathQuery;
        private readonly IYamlSerializers _serializers;

        public ValuesEngine(
            IJmesPathQuery jmesPathQuery,
            IYamlSerializers serializers)
        {
            _jmesPathQuery = jmesPathQuery;
            _serializers = serializers;
        }

        public string EvaluateToString(string source, object context)
        {
            var result = ProcessValues(source, context);
            var value = ConvertToString(result);
            return value;
        }

        public bool EvaluateToBoolean(string source, object context)
        {
            var result = _jmesPathQuery.Search(source, context);
            var value = ConditionBoolean(result);
            return value;
        }

        public object ProcessValues(object source, object context)
        {
            return ProcessValuesRecursive(source, new[] { context }, promoteArrays: false);
        }

        public IList<object> ProcessValuesForeachIn(object source, object context)
        {
            var result = ProcessValuesRecursive(source, new[] { context }, promoteArrays: true);
            if (result is IList<object> resultList)
            {
                return resultList;
            }

            throw new ApplicationException("Foreach values contained no arrays");
        }

        public object ProcessValuesForeachOut(object source, IList<object> contexts)
        {
            return ProcessValuesRecursive(source, contexts, promoteArrays: false);
        }

        private object ProcessValuesRecursive(object source, IList<object> contexts, bool promoteArrays)
        {
            if (source is IDictionary<object, object> sourceDictionary)
            {
                var arrayIsPromoting = false;
                var arrayLength = 0;

                void CheckArrayIsPromoting(object result)
                {
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

                var output = new Dictionary<object, object>();
                foreach (var kv in sourceDictionary)
                {
                    var propertyName = Convert.ToString(kv.Key, CultureInfo.InvariantCulture);
                    if (propertyName.StartsWith('(') && propertyName.EndsWith(')'))
                    {
                        var propertyGroupings = contexts
                            .Select(eachContext =>
                            {
                                var eachPropertyName = _jmesPathQuery.Search(propertyName, eachContext);
                                return (eachPropertyName, eachContext);
                            })
                            .ToList()
                            .GroupBy(x => x.eachPropertyName, x => x.eachContext);

                        foreach (var propertyGrouping in propertyGroupings)
                        {
                            var result = ProcessValuesRecursive(kv.Value, propertyGrouping.ToList(), promoteArrays: promoteArrays);
                            output[propertyGrouping.Key] = result;
                            CheckArrayIsPromoting(result);
                        }
                    }
                    else
                    {
                        var result = ProcessValuesRecursive(kv.Value, contexts, promoteArrays: promoteArrays);
                        output[propertyName] = result;
                        CheckArrayIsPromoting(result);
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
    }
}
