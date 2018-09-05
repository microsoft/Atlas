// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Atlas.CommandLine.Templates
{
    public interface ITemplateEngineFactory
    {
        ITemplateEngine Create(TemplateEngineOptions options);
    }
}
