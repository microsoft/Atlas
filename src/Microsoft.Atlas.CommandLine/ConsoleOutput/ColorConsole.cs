// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Atlas.CommandLine.ConsoleOutput
{
    internal class ColorConsole
    {
        private Stack<ConsoleColor> _stack = new Stack<ConsoleColor>();

        private ColorConsole(TextWriter writer)
        {
            Writer = writer;
        }

        public TextWriter Writer { get; }

        public ConsoleColor OriginalForegroundColor { get; }

        public static ColorConsole GetOutput()
        {
            return new ColorConsole(Console.Out);
        }

        public static ColorConsole GetError()
        {
            return new ColorConsole(Console.Error);
        }

        public void Write(string message)
        {
            var escapeScan = 0;
            for (; ;)
            {
                var escapeIndex = message.IndexOf("\x1b{", escapeScan, StringComparison.Ordinal);
                if (escapeIndex == -1)
                {
                    var text = message.Substring(escapeScan);
                    Writer.Write(text);
                    break;
                }
                else
                {
                    var text = message.Substring(escapeScan, escapeIndex - escapeScan);
                    Writer.Write(text);

                    var startIndex = escapeIndex + 2;
                    var endIndex = startIndex;
                    while (endIndex != message.Length &&
                        message[endIndex] >= 0x20 &&
                        message[endIndex] <= 0x3f)
                    {
                        endIndex += 1;
                    }

                    if (endIndex == message.Length)
                    {
                        break;
                    }

                    switch (message[endIndex])
                    {
                        case '}':
                            int value;
                            if (int.TryParse(message.Substring(startIndex, endIndex - startIndex), out value))
                            {
                                if (value == -1)
                                {
                                    PopColor();
                                }
                                else
                                {
                                    PushColor((ConsoleColor)value);
                                }
                            }

                            break;
                    }

                    escapeScan = endIndex + 1;
                }
            }
        }

        private void PushColor(ConsoleColor color)
        {
            _stack.Push(color);
            Console.ForegroundColor = color;
        }

        private void PopColor()
        {
            _stack.Pop();
            if (_stack.TryPeek(out var color))
            {
                Console.ForegroundColor = color;
            }
            else
            {
                Console.ResetColor();
            }
        }
    }
}
