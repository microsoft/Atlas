// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Abstractions;
using Microsoft.Atlas.CommandLine.Blueprints.Models;
using Microsoft.Atlas.CommandLine.Factories;

namespace Microsoft.Atlas.CommandLine.Blueprints.Dependencies
{
    public class DependencyBlueprintDecorator :
        IBlueprintPackage,
        IFactoryInstance<(DependencyReference dependencyInfo, IBlueprintPackage package)>
    {
        private readonly IFileSystem _fileSystem;
        private readonly IBlueprintManager _blueprintManager;
        private IBlueprintPackage _innerPackage;
        private Dictionary<string, IBlueprintPackage> _dependencyPackages = new Dictionary<string, IBlueprintPackage>();

        public DependencyBlueprintDecorator(
            IFileSystem fileSystem,
            IBlueprintManager blueprintManager)
        {
            _fileSystem = fileSystem;
            _blueprintManager = blueprintManager;
        }

        public string Location => _innerPackage.Location;

        public async Task Initialize((DependencyReference dependencyInfo, IBlueprintPackage package) args)
        {
            _innerPackage = args.package;

            var source = args.dependencyInfo.source;
            if (string.IsNullOrEmpty(source))
            {
                source = _fileSystem.PathCombine(args.package.Location, "..");
            }

            foreach (var input in args.dependencyInfo.inputs)
            {
                var dependencyBlueprint = _fileSystem.PathCombine(source, input);
                var dependencyPackage = await _blueprintManager.GetBlueprintPackage(dependencyBlueprint);
                _dependencyPackages.Add(_fileSystem.PathCombine("workflows", input, string.Empty), dependencyPackage);
            }
        }

        public bool Exists(string path)
        {
            return TryDependencies(path, (subpath, subpackage) => subpackage.Exists(subpath), out var result) ? result : _innerPackage.Exists(path);
        }

        public IEnumerable<string> GetGeneratedPaths()
        {
            var dependencyGeneratedPaths = _dependencyPackages
                .SelectMany(kv =>
                {
                    return kv.Value.GetGeneratedPaths().Select(path => _fileSystem.PathCombine(kv.Key, path));
                });

            return dependencyGeneratedPaths.Concat(_innerPackage.GetGeneratedPaths());
        }

        public TextReader OpenText(string path)
        {
            return TryDependencies(path, (subpath, subpackage) => subpackage.OpenText(subpath), out var result) ? result : _innerPackage.OpenText(path);
        }

        private bool TryDependencies<TResult>(string path, Func<string, IBlueprintPackage, TResult> selector, out TResult result)
        {
            foreach (var kv in _dependencyPackages)
            {
                if (path.StartsWith(kv.Key))
                {
                    result = selector(path.Substring(kv.Key.Length), kv.Value);
                    return true;
                }
            }

            result = default(TResult);
            return false;
        }
    }
}
