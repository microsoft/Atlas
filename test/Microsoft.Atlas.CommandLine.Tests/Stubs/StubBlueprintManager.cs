// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Abstractions;
using Microsoft.Atlas.CommandLine.Blueprints;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{
    public class StubBlueprintManager : IBlueprintManager
    {
        public IDictionary<string, StubBlueprintPackage> Blueprints { get; set; } = new Dictionary<string, StubBlueprintPackage>();

        async Task<IBlueprintPackage> IBlueprintManager.GetBlueprintPackage(string blueprint) => Blueprints.TryGetValue(blueprint, out var value) ? value : null;

        Task<IBlueprintPackage> IBlueprintManager.GetBlueprintPackageDependency(IBlueprintPackage package, string blueprint)
        {
            throw new System.NotImplementedException("This scenario should be unit tested with StubFileSystem");
        }
    }
}
