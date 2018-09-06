// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Atlas.CommandLine.Targets
{
    public interface IPatternMatcherFactory
    {
        IPatternMatcher Create(IEnumerable<string> patterns);
    }
}
