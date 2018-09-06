// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public class BlueprintManager : IBlueprintManager
    {
        private readonly IEnumerable<IBlueprintPackageProvider> _providers;

        public BlueprintManager(IEnumerable<IBlueprintPackageProvider> providers)
        {
            _providers = providers;
        }

        public IBlueprintPackage GetBlueprintPackage(string blueprint)
        {
            foreach (var provider in _providers)
            {
                var blueprintPackage = provider.TryGetBlueprintPackage(blueprint);
                if (blueprintPackage != null)
                {
                    return blueprintPackage;
                }
            }

            return null;
        }
    }
}
