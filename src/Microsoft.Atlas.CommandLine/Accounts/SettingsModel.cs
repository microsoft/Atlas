// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable SA1402 // File may only contain a single class
#pragma warning disable SA1516 // Elements must be separated by blank line
#pragma warning disable SA1649 // File name must match first type name

namespace Microsoft.Atlas.CommandLine.Accounts
{
    public class SettingsModel
    {
        public List<AccountEntry> accounts { get; set; } = new List<AccountEntry>();
    }

    public class AccountEntry
    {
        public string name { get; internal set; }

        public string authority { get; set; }
        public string resource { get; set; }

        public string username { get; set; }
        public string password { get; set; }

        public string appid { get; set; }
        public string secret { get; set; }

        public string token { get; set; }
    }
}
