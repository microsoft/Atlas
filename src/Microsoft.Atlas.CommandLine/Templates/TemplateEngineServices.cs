// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Atlas.CommandLine.Secrets;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Atlas.CommandLine.Templates.Helpers;

namespace Microsoft.Atlas.CommandLine.Templates
{
    public class TemplateEngineServices
    {
        public TemplateEngineServices (
            IYamlSerializers serializers,
            ISecretTracker secretTracker,
            IEnumerable<ITemplateHelperProvider> templateHelperProviders)
        {
            Serializers = serializers;
            SecretTracker = secretTracker;
            TemplateHelperProviders = templateHelperProviders;
        }

        public IYamlSerializers Serializers { get; }

        public ISecretTracker SecretTracker { get; }

        public IEnumerable<ITemplateHelperProvider> TemplateHelperProviders { get; }
    }
}
