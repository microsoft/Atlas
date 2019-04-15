
## Azure DNS Updater

This workflow works as a dynamic DNS updater for Azure DNS. The workflow makes use of
the ipify.org REST API as well ast Azure's DNS REST APIs.

Note that the example relies on the DNS having already been configured in Azure, it 
does not create those resources. 

Values to be provided in values.yaml
```
azure:
  tenant: [Azure Tenant ID]
  subscription: [Azure Subscription ID]
  resourceGroup: [Name of the Resource Group containing DNS]
  location: [Resource Group Location]

dns:
  zoneName: [DNS Zone name]
  ttl: 3600
```

To run the workflow
```
atlas deploy dnsZones
```

References
``` yaml
workflows:
  github-atlas-library:
    source: https://github.com/Microsoft/Atlas/tree/master/library
    inputs: 
    - azure-deployment
    
swagger:

  resources:
    target: apis/azure
    source: https://github.com/Azure/azure-rest-api-specs/tree/master/specification/dns/resource-manager/
    inputs: 
    - Microsoft.Network/stable/2018-05-01/dns.json
    extra:
      auth:
        tenant: '{{ azure.tenant }}'
        resource: https://management.azure.com/
        client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46 # Azure CLI

```
