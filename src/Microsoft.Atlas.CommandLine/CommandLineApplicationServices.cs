// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.Atlas.CommandLine
{
    public class CommandLineApplicationServices : CommandLineApplication
    {
        public CommandLineApplicationServices(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }
    }
}
