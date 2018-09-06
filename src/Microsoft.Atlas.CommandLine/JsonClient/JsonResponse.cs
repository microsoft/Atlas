// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Net;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable SA1516 // Elements must be separated by blank line
#pragma warning disable SA1649 // File name must match first type name

namespace Microsoft.Atlas.CommandLine.JsonClient
{
    public class JsonResponse
    {
        public HttpStatusCode status { get; set; }

        public object body { get; set; }
    }
}
