// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Text;

namespace Microsoft.Atlas.CommandLine.Secrets
{
    public class SecretTextWriter : TextWriter
    {
        private readonly ISecretTracker _secretTracker;
        private readonly TextWriter _writer;
        private StringBuilder _buffer = new StringBuilder();

        public SecretTextWriter(ISecretTracker secretTracker, TextWriter writer)
        {
            _secretTracker = secretTracker;
            _writer = writer;
        }

        public override Encoding Encoding => _writer.Encoding;

        public override void Close()
        {
            SendBuffer(completeLines: false);
            _writer.Close();
        }

        public override void Flush()
        {
            SendBuffer(completeLines: false);
            _writer.Flush();
        }

        public override void Write(char value)
        {
            _buffer.Append(value);
            if (value == '\n')
            {
                SendBuffer(completeLines: true);
            }
        }

        public override void Write(string value)
        {
            _buffer.Append(value);
            if (value.IndexOf('\n') != -1)
            {
                SendBuffer(completeLines: true);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SendBuffer(completeLines: false);
                _writer.Dispose();
            }
        }

        private void SendBuffer(bool completeLines)
        {
            var text = _buffer.ToString();
            _buffer.Clear();

            if (completeLines)
            {
                var lastLinefeed = text.LastIndexOf('\n');
                var part1 = text.Substring(0, lastLinefeed + 1);
                var part2 = text.Substring(lastLinefeed + 1);
                text = part1;
                if (!string.IsNullOrEmpty(part2))
                {
                    _buffer.Append(part2);
                }
            }

            var textRedacted = _secretTracker.FilterString(text);
            if (!string.IsNullOrEmpty(textRedacted))
            {
                _writer.Write(textRedacted);
            }
        }
    }
}
