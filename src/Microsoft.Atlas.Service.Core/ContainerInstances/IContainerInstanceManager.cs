using System.Threading.Tasks;

namespace Microsoft.Atlas.Service.Core.ContainerInstances
{
    public interface IContainerInstanceManager
    {
        Task CreateInstance(CreateInstanceContext context);
    }
}