// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Atlas.CommandLine.Targets
{
    public interface IPatternMatcher
    {
        bool IsMatch(string path);
    }
}
