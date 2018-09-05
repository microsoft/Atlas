// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Atlas.CommandLine.Targets
{
    public class PatternMatcher : IPatternMatcher
    {
        private PatternMatcherExpression[] _expressions;

        public PatternMatcher(IEnumerable<PatternMatcherExpression> patternMatcherExpression)
        {
            _expressions = patternMatcherExpression.ToArray();
        }

        public bool IsMatch(string path)
        {
            var match = false;
            foreach (var expression in _expressions)
            {
                // each pattern is called so they can individually track if they've been matched or not
                if (expression.IsMatch(path))
                {
                    match = true;
                }
            }

            return match;
        }
    }
}
