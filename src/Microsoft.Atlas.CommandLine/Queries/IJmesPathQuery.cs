// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Atlas.CommandLine.Queries
{
    public interface IJmesPathQuery
    {
        object Search(string expression, object json);

        string SearchJsonText(string expression, string jsonText);
    }
}
