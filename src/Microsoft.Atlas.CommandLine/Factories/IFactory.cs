// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.Atlas.CommandLine.Factories
{
    public interface IFactory<TService>
    {
        Task<TService> Create<TArguments>(TArguments arguments);
    }
}
