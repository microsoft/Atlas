namespace Microsoft.Atlas.Service.Core.ContainerInstances
{
    public class ContainerInstanceOptions
    {
        public string AzureEnvironment { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroupName { get; set; }
        public string Location { get; set; }

        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string ContainerRegistryName { get; set; }
    }
}
