// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.ConsoleOutput
{
    public interface IConsole
    {
        TextWriter Out { get; }

        TextWriter Error { get; }

        void WriteLine(string text = null);
    }
}
