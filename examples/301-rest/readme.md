
## Demonstrates making a REST call

``` yaml
license: https://github.com/Microsoft/Atlas/blob/master/LICENSE
```

This package uses the Azure REST API for subscriptions.

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
      client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46
- target: apis/azure
  source: https://github.com/Azure/azure-rest-api-specs/tree/master/specification/resources/resource-manager/
  inputs: 
  - Microsoft.Resources/stable/2018-05-01/resources.json
  # - Microsoft.Resources/stable/2016-09-01/links.json
  # - Microsoft.Resources/stable/2016-06-01/subscriptions.json
  # - Microsoft.Authorization/stable/2016-09-01/locks.json
  # - Microsoft.Authorization/stable/2016-12-01/policyAssignments.json
  # - Microsoft.Authorization/stable/2016-12-01/policyDefinitions.json
  extra:
    auth:
      tenant: '{{ azure.tenant }}'
      resource: https://management.azure.com/
      client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46
```
