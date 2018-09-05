// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Text;

namespace Microsoft.Atlas.CommandLine.ConsoleOutput
{
    internal class AnsiConsoleWriter : TextWriter
    {
        private readonly AnsiConsole _ansiConsole;
        private readonly StringBuilder _buffer = new StringBuilder();

        public AnsiConsoleWriter(AnsiConsole ansiConsole)
        {
            _ansiConsole = ansiConsole;
        }

        public override Encoding Encoding => _ansiConsole.Writer.Encoding;

        public override void Write(char value) => _buffer.Append(value);

        public override void Write(string value) => Send(value);

        public override void Flush() => Send(null);

        private void Send(string value)
        {
            if (_buffer.Length != 0)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _buffer.Append(value);
                }

                _ansiConsole.Write(_buffer.ToString());
                _buffer.Clear();
            }
            else if (!string.IsNullOrEmpty(value))
            {
                _ansiConsole.Write(value);
            }
        }
    }
}
