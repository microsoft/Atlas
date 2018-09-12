// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable SA1516 // Elements must be separated by blank line
#pragma warning disable SA1649 // File name must match first type name

namespace Microsoft.Atlas.CommandLine.OAuth2
{
    public class HttpAuthentication
    {
        public string tenant { get; set; }

        public string resourceId { get; set; }

        public string clientId { get; set; }

        public bool interactive { get; set; } = true;
    }
}
