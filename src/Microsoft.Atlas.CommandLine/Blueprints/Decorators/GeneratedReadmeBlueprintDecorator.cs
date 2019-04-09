namespace Microsoft.Atlas.CommandLine.Blueprints.Decorators
{
    public class GeneratedReadmeBlueprintDecorator : GeneratedFileBlueprintDecorator
    {
        public GeneratedReadmeBlueprintDecorator(IBlueprintPackage package, string readmeText)
            : base(package)
        {
            GeneratedFiles["readme.md"] = readmeText;
        }
    }
}
