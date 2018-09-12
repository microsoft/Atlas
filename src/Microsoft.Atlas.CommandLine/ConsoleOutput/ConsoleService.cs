// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;

namespace Microsoft.Atlas.CommandLine.ConsoleOutput
{
    public class ConsoleService : IConsole
    {
        public TextWriter Out => Console.Out;

        public TextWriter Error => Console.Error;

        public void WriteLine(string text = null) => Console.WriteLine(text ?? string.Empty);
    }
}
