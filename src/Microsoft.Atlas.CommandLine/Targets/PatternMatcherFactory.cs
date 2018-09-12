// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Atlas.CommandLine.Targets
{
    public class PatternMatcherFactory : IPatternMatcherFactory
    {
        public IPatternMatcher Create(IEnumerable<string> patterns)
        {
            return new PatternMatcher(patterns.SelectMany(CreateExpressions));
        }

        public string SegmentToRegex(string segment)
        {
            if (segment == "**")
            {
                return ".*";
            }

            return string.Join("[^/]*", segment.Split('*').Select(Regex.Escape));
        }

        private IEnumerable<PatternMatcherExpression> CreateExpressions(string pattern)
        {
            var normalizedPattern = pattern;
            if (!normalizedPattern.StartsWith("/"))
            {
                normalizedPattern = "/" + normalizedPattern;
            }

            if (!normalizedPattern.EndsWith("/") && !normalizedPattern.EndsWith("/**"))
            {
                normalizedPattern = normalizedPattern + "/**";
            }

            if (!normalizedPattern.EndsWith("/"))
            {
                normalizedPattern = normalizedPattern + "/";
            }

            var segments = normalizedPattern.Split('/');
            foreach (var length in Enumerable.Range(1, segments.Count() - 1))
            {
                var globPattern = segments.Take(length).Aggregate(string.Empty, (path, segment) => path + segment + "/");
                var regexPattern = segments.Take(length).Select(SegmentToRegex).Aggregate("^", (path, segment) => path + segment + "/") + "$";

                yield return new PatternMatcherExpression(pattern, globPattern, regexPattern);
            }
        }
    }
}
