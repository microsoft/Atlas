namespace Microsoft.Atlas.CommandLine.Swagger
{
    public interface IRequestGenerator
    {
        void GenerateSingleRequestDefinition(GenerateSingleRequestDefinitionContext context);
    }
}