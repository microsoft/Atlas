// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.Atlas.CommandLine.JsonClient
{
    public interface IJsonHttpClient
    {
        Task<JsonResponse> SendAsync(JsonRequest request);
    }
}
