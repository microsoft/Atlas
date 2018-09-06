// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Microsoft.Atlas.CommandLine.Commands
{
    public class JTokenEmitter : IEmitter
    {
        private RootFocus _root;
        private Focus _focus;

        public JTokenEmitter()
        {
            _focus = _root = new RootFocus(null);
        }

        public JToken Root => _root.Token;

        public void Emit(ParsingEvent parsingEvent)
        {
            _focus = _focus.Emit(parsingEvent);
        }

        private abstract class Focus
        {
            public Focus(Focus parent)
            {
                Parent = parent;
            }

            internal Focus Parent { get; private set; }

            public virtual Focus Emit(ParsingEvent parsingEvent)
            {
                if (parsingEvent is SequenceStart)
                {
                    return new SequenceFocus(this);
                }
                else if (parsingEvent is MappingStart)
                {
                    return new MappingFocus(this);
                }
                else if (parsingEvent is Scalar)
                {
                    var scalar = (Scalar)parsingEvent;
                    if (scalar.Style == ScalarStyle.DoubleQuoted)
                    {
                        return Add(new JValue(scalar.Value));
                    }
                    else
                    {
                        return Add(JValue.Parse(scalar.Value));
                    }
                }
                else
                {
                    throw new ApplicationException($"Unexpected {parsingEvent.GetType().Name}");
                }
            }

            public abstract Focus Add(JToken token);
        }

        private class RootFocus : Focus
        {
            public RootFocus(Focus parent)
                : base(parent)
            {
            }

            public JToken Token { get; private set; }

            public override Focus Add(JToken token)
            {
                Token = token;
                return this;
            }
        }

        private class MappingFocus : Focus
        {
            private readonly JObject _mapping;

            public MappingFocus(Focus parent)
                : base(parent)
            {
                _mapping = new JObject();
            }

            public override Focus Emit(ParsingEvent parsingEvent)
            {
                if (parsingEvent is Scalar)
                {
                    return new PropertyFocus(this, ((Scalar)parsingEvent).Value);
                }
                else if (parsingEvent is MappingEnd)
                {
                    return Parent.Add(_mapping);
                }

                throw new ApplicationException($"Unexpected {parsingEvent.GetType().Name}");
            }

            public override Focus Add(JToken token)
            {
                _mapping.Add(token);
                return this;
            }
        }

        private class PropertyFocus : Focus
        {
            private string _name;

            public PropertyFocus(Focus parent, string name)
                : base(parent)
            {
                _name = name;
            }

            public override Focus Add(JToken token)
            {
                return Parent.Add(new JProperty(_name, token));
            }
        }

        private class SequenceFocus : Focus
        {
            private readonly JArray _sequence;

            public SequenceFocus(Focus parent)
                : base(parent)
            {
                _sequence = new JArray();
            }

            public override Focus Emit(ParsingEvent parsingEvent)
            {
                if (parsingEvent is SequenceEnd)
                {
                    return Parent.Add(_sequence);
                }
                else
                {
                    return base.Emit(parsingEvent);
                }
            }

            public override Focus Add(JToken token)
            {
                _sequence.Add(token);
                return this;
            }
        }
    }
}
