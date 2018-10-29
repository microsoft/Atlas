// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.Atlas.CommandLine.Swagger.Models;

namespace Microsoft.Atlas.CommandLine.Swagger
{
    public interface ISwaggarDocumentLoader
    {
        Task<SwaggerDocument> LoadDocument(string basePath, string relativePath);

        TObject GetResolved<TObject>(TObject referenceObject)
            where TObject : Reference;
    }
}
