// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public interface IValuesEngine
    {
        string EvaluateToString(string source, object context);

        bool EvaluateToBoolean(string source, object context);

        object ProcessValues(object source, object context);

        IList<object> ProcessValuesForeachIn(object source, object context);

        object ProcessValuesForeachOut(object source, IList<object> contexts);
    }
}
