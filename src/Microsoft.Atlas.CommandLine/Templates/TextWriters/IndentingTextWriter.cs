// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HandlebarsDotNet;

namespace Microsoft.Atlas.CommandLine.Templates.TextWriters
{
    public class IndentingTextWriter : TextWriter
    {
        private static readonly object _eol = new object();
        private readonly TextWriter _inner;
        private readonly string _indentation;
        private List<object> _spool = new List<object>();

        public IndentingTextWriter(TextWriter inner, string indentation)
        {
            _inner = inner;
            _indentation = indentation;
        }

        public override Encoding Encoding => _inner.Encoding;

        public override void Write(char value)
        {
            if (value == '\n')
            {
                _spool.Add(_eol);
            }
            else
            {
                var chars = _spool.LastOrDefault() as StringBuilder;
                if (chars == null)
                {
                    chars = new StringBuilder();
                    _spool.Add(chars);
                }

                chars.Append(value);
            }
        }

        public override void Write(string value)
        {
            _spool.AddRange(value.Split('\n').SelectMany(x => new[] { _eol, x }).Skip(1));
        }

        public override void Write(object value)
        {
            if (value is ISafeString)
            {
                _spool.AddRange(value.ToString().Split('\n').SelectMany(x => new[] { _eol, new SafeString(x) }).Skip(1));
            }
            else
            {
                base.Write(value);
            }
        }

        public void Send()
        {
            bool bol = true;
            foreach (var x in _spool.Concat(new[] { _eol }))
            {
                var item = x is StringBuilder ? x.ToString() : x;
                if (item is StringBuilder)
                {
                    item = item.ToString();
                }

                if (item == _eol)
                {
                    _inner.Write(new SafeString("\n"));
                    bol = true;
                    continue;
                }

                if (bol)
                {
                    bol = false;
                    if (item.ToString() != "\r")
                    {
                        _inner.Write(_indentation);
                    }
                }

                _inner.Write(item is ISafeString ? item : new SafeString(item.ToString()));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Send();
            }
        }

        private class SafeString : ISafeString
        {
            private string _value;

            public SafeString(string value) => _value = value;

            public override string ToString() => _value;
        }
    }
}
