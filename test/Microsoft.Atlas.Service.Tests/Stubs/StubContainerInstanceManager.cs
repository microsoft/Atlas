using System.Threading.Tasks;
using Microsoft.Atlas.Service.Core.ContainerInstances;

namespace Microsoft.Atlas.Service.Tests.Stubs
{
    public class StubContainerInstanceManager : IContainerInstanceManager
    {
        public StubContainerInstanceManager()
        {
        }

        public async Task CreateInstance(CreateInstanceContext context)
        {
            context.SetResult("/instance/" + context.InstanceName);
        }
    }
}
