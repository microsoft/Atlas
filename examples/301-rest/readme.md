
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
      tenant: '{{ request.auth.tenant }}'
      resource: https://management.azure.com/
      client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46
```
