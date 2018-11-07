using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Atlas.Service.Core.ContainerInstances
{
    public class ContainerInstanceManager : IContainerInstanceManager
    {
        private readonly ContainerInstanceOptions _options;
        private readonly ILogger<ContainerInstanceManager> _logger;

        public ContainerInstanceManager(
            IOptions<ContainerInstanceOptions> options,
            ILogger<ContainerInstanceManager> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task CreateInstance(CreateInstanceContext context)
        {
            var servicePrincipalLoginInformation = new ServicePrincipalLoginInformation
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
            };

            var azureEnvironment = AzureEnvironment.FromName(_options.AzureEnvironment);

            var azureCredentials = new AzureCredentials(
                servicePrincipalLoginInformation,
                _options.TenantId,
                azureEnvironment);

            var restClient = RestClient.Configure()
                .WithEnvironment(azureEnvironment)
                .WithCredentials(azureCredentials)
                .Build();

            var client = new ContainerInstanceManagementClient(restClient)
            {
                SubscriptionId = _options.SubscriptionId
            };

            var containerGroup = await client.ContainerGroups.BeginCreateOrUpdateAsync(
                _options.ResourceGroupName,
                $"cg{context.InstanceName}",
                new ContainerGroupInner
                {
                    Volumes = new List<Volume>
                    {

                    },
                    Location = _options.Location,
                    OsType = "Linux",
                    RestartPolicy = "Never",
                    ImageRegistryCredentials = new[]
                    {
                        new ImageRegistryCredential
                        {
                            Server = _options.ContainerRegistryName,
                            Username = _options.ClientId,
                            Password = _options.ClientSecret,
                        }
                    },
                    Containers = new List<Container>
                    {
                        new Container
                        {
                            Name = "atlas-cli",
                            Image = $"{_options.ContainerRegistryName}/atlas-cli:{context.AtlasVersion}",
                            Command = new[] {"dotnet", "atlas.dll"}.Concat(context.Arguments).ToList(),
                            Resources = new  ResourceRequirements
                            {
                                Requests = new ResourceRequests
                                {
                                    Cpu = 1,
                                    MemoryInGB = 4,
                                },
                            },
                        },
                    },
                });

            context.SetResult(containerGroup.Id);

        }
    }
}
