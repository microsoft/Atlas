using System.Threading.Tasks;
using Microsoft.Atlas.Service.Controllers;
using Microsoft.Atlas.Service.Tests.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Atlas.Service.Tests
{
    [TestClass]
    public class ValuesControllerTests
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var controller = new ValuesController(new StubContainerInstanceManager());
            var result = await controller.Get("testing");

        }
    }
}
