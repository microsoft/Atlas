// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Atlas.CommandLine.Swagger
{
    public interface IRequestGenerator
    {
        void GenerateSingleRequestDefinition(GenerateSingleRequestDefinitionContext context);
    }
}
