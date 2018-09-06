// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Atlas.CommandLine.Secrets;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{
    public class StubSecretTracker : ISecretTracker
    {
        public List<string> Secrets { get; } = new List<string>();

        public void AddSecret(string secret)
        {
            Secrets.Add(secret);
        }

        public string FilterString(string text)
        {
            throw new NotImplementedException();
        }

        public TextWriter FilterTextWriter(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
