// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Text.RegularExpressions;

namespace Microsoft.Atlas.CommandLine.Targets
{
    public class PatternMatcherExpression
    {
        private readonly string _pattern;
        private readonly string _globPattern;
        private readonly string _regexPattern;
        private readonly Regex _regex;

        public PatternMatcherExpression(string pattern, string globPattern, string regexPattern)
        {
            _pattern = pattern;
            _globPattern = globPattern;
            _regexPattern = regexPattern;
            _regex = new Regex(regexPattern);
        }

        public bool IsMatch(string path)
        {
            return _regex.IsMatch(path);
        }
    }
}
