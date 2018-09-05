// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Microsoft.Atlas.CommandLine.Serialization
{
    public class DoubleQuoteAmbiguousStringScalarEmitter : ChainedEventEmitter
    {
        protected DoubleQuoteAmbiguousStringScalarEmitter(IEventEmitter nextEmitter)
            : base(nextEmitter)
        {
        }

        public static DoubleQuoteAmbiguousStringScalarEmitter Factory(IEventEmitter nextEmitter) => new DoubleQuoteAmbiguousStringScalarEmitter(nextEmitter);

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            if (eventInfo.Source.StaticType == typeof(object) &&
                eventInfo.Source.Type == typeof(string) &&
                eventInfo.Style == ScalarStyle.Any)
            {
                var scalarValue = (string)eventInfo.Source.Value;
                if (bool.TryParse(scalarValue, out var boolValue) ||
                    int.TryParse(scalarValue, out var intValue) ||
                    double.TryParse(scalarValue, out var doubleValue) ||
                    string.Equals(scalarValue, "null", StringComparison.Ordinal))
                {
                    eventInfo.Style = ScalarStyle.DoubleQuoted;
                }
            }

            base.Emit(eventInfo, emitter);
        }
    }
}
