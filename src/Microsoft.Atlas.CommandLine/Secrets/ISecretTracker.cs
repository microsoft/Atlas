// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.Secrets
{
    public interface ISecretTracker
    {
        void AddSecret(string secret);

        string FilterString(string text);

        TextWriter FilterTextWriter(TextWriter writer);
    }
}
