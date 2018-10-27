
## Gather and display Azure account information

For your login this workflow will gather and display information. It lists
the Active Directory tenants in which your login has an identity, and then for each 
tenant it lists the Azure subscriptions in which your login is a member of a role.

### Workflow details
``` yaml
license: https://github.com/Microsoft/Atlas/blob/master/LICENSE
```

### API references

This workflow uses the Azure REST API for subscription information and the Graph REST API for user and domain information.

``` yaml
swagger:

- target: apis/azure
  source: https://github.com/Azure/azure-rest-api-specs/tree/master/specification/subscription/resource-manager/
  inputs: 
  - Microsoft.Subscription/stable/2016-06-01/subscriptions.json
  extra:
    auth:
      tenant: '{{ azure.tenant }}'
      resource: https://management.azure.com/
      client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46 # Azure CLI

- target: apis/graph
  source: https://github.com/Azure/azure-rest-api-specs/tree/master/specification/graphrbac/data-plane/
  inputs: 
  - stable/1.6/graphrbac.json
  extra:
    auth:
      tenant: '{{ azure.tenant }}'
      resource: https://graph.windows.net/
      client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46 # Azure CLI
```
