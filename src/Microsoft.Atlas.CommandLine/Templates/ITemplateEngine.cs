// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;

namespace Microsoft.Atlas.CommandLine.Templates
{
    public interface ITemplateEngine
    {
        object LoadValues(string fileName);

        void Render(string templateFile, object values, TextWriter writer);

        TModel Render<TModel>(string templateFile, object values);
    }
}
