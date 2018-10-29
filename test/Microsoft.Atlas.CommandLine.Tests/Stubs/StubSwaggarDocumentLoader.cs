// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Swagger;
using Microsoft.Atlas.CommandLine.Swagger.Models;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{
    public class StubSwaggarDocumentLoader : ISwaggarDocumentLoader
    {
        public TObject GetResolved<TObject>(TObject referenceObject)
            where TObject : Reference
        {
            return referenceObject;
        }

        public Task<SwaggerDocument> LoadDocument(string basePath, string relativePath)
        {
            throw new System.NotImplementedException();
        }
    }
}
