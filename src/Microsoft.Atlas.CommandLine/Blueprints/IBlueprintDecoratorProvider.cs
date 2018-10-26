using System.Threading.Tasks;

namespace Microsoft.Atlas.CommandLine.Blueprints
{
    public interface IBlueprintDecoratorProvider
    {
        Task<IBlueprintPackage> CreateDecorator<TInfo>(TInfo info, IBlueprintPackage package);
    }
}
