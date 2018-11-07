using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Atlas.Service.Core
{
    public class Class1
    {
        public async Task Blah()
        {
            var azureCredentials = new AzureCredentials(
                new DeviceCredentialInformation
                {
                    ClientId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46",
                    DeviceCodeFlowHandler = DeviceCodeFlowHandler,
                },
                "microsoft.onmicrosoft.com",
                AzureEnvironment.AzureGlobalCloud)
                .WithDefaultSubscription("cd0fa82d-b6b6-4361-b002-050c32f71353");

            var restClient = RestClient.Configure()
                // .WithBaseUri("https://x")
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithCredentials(azureCredentials)
                .Build();

            var client = new ContainerInstanceManagementClient(restClient)
            {
                SubscriptionId = "cd0fa82d-b6b6-4361-b002-050c32f71353"
            };

            var containerGroup = await client.ContainerGroups.CreateOrUpdateAsync(
                "lodejard-containergroups",
                "lodejard-cg1",
                new ContainerGroupInner
                {
                    //Id = "lodejard-cg1-1",
                    Volumes = new List<Volume>
                    {

                    },
                    Location = "westus2",
                    OsType = "Linux",
                    RestartPolicy = "Never",
                    ImageRegistryCredentials = new[]
                    {
                        new ImageRegistryCredential
                        {
                            Server = "atlasprivate.azurecr.io",
                            Username = "atlasprivate",
                            Password = "Tl3lXl6K1Uul=RrU=RkDoAl2rBrdxrj2",
                        }
                    },
                    Containers = new List<Container>
                    {
                        new Container
                        {
                            Name = "lodejard-cg1-2",
                            Image = "microsoft/dotnet",
                            Command = new[] {"/bin/sh", "-c", "pwd && ls"},
                            Resources = new  ResourceRequirements
                            {
                                Requests = new ResourceRequests
                                {
                                    Cpu = .5,
                                    MemoryInGB = .5,
                                },
                            },
                        },
                        new Container
                        {
                            Name = "lodejard-cg1-3",
                            Image = "atlasprivate.azurecr.io/atlas-cli:0.1.3730778",
                            Command = new[] {"dotnet", "atlas.dll", "--help"},
                            Resources = new  ResourceRequirements
                            {
                                Requests = new ResourceRequests
                                {
                                    Cpu = .5,
                                    MemoryInGB = .5,
                                },
                            },
                        },
                    },
                });
            var id = containerGroup.Id;


            var logs2 = await client.Container.ListLogsAsync(
                "lodejard-containergroups",
                "lodejard-cg1",
                "lodejard-cg1-2");


            var logs3 = await client.Container.ListLogsAsync(
                "lodejard-containergroups",
                "lodejard-cg1",
                "lodejard-cg1-3");

            var x = 5;
        }

        private bool DeviceCodeFlowHandler(DeviceCodeResult deviceCodeResult)
        {
            Console.WriteLine(deviceCodeResult.Message);
            return true;
        }
    }
}
