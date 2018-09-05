// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Atlas.CommandLine.Templates
{
    public class TemplateEngineFactory : ITemplateEngineFactory
    {
        private readonly TemplateEngineServices _services;

        public TemplateEngineFactory(TemplateEngineServices services)
        {
            _services = services;
        }

        public ITemplateEngine Create(TemplateEngineOptions options)
        {
            return new TemplateEngine(_services, options);
        }
    }
}
