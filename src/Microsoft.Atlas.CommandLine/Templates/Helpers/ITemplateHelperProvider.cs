// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using HandlebarsDotNet;

namespace Microsoft.Atlas.CommandLine.Templates.Helpers
{
    public interface ITemplateHelperProvider
    {
        IEnumerable<KeyValuePair<string, HandlebarsHelper>> GetHelpers();

        IEnumerable<KeyValuePair<string, HandlebarsBlockHelper>> GetBlockHelpers();
    }
}
