// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.Atlas.CommandLine.ConsoleOutput;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.CommandLine.Tests.Stubs
{
    public class StubConsole : IConsole
    {
        public StringWriter OutStringWriter { get; set; } = new StringWriter();

        public StringWriter ErrorStringWriter { get; set; } = new StringWriter();

        public TextWriter Out => OutStringWriter;

        public TextWriter Error => ErrorStringWriter;

        public void WriteLine(string text = null) => Out.WriteLine(text ?? string.Empty);

        public void AssertContainsInOrder(params string[] segments)
        {
            var output = OutStringWriter.GetStringBuilder().ToString();
            var startIndex = 0;
            foreach (var segment in segments)
            {
                var segmentIndex = output.IndexOf(segment, startIndex);
                if (segmentIndex < 0)
                {
                    Assert.Fail($"Output did not contain {segment}{Environment.NewLine}{output}");
                }

                startIndex = segmentIndex + segment.Length;
            }
        }
    }
}
