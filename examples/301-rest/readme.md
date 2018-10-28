
## Demonstrates making a REST call

Specify a tenant with `--set azure.tenant=YOUR-TENANT-GUID` or `--set azure.tenant=YOUR-DOMAIN.onmicrosoft.com`

### About

``` yaml
info:
  title: 301-rest
  version: 0.1
  description: Demonstrates making a REST call
  contact:
    name: Microsoft
    url: https://github.com/Microsoft/Atlas/issues/new
  license:
    name: MIT
    url: https://github.com/Microsoft/Atlas/blob/master/LICENSE
```

### API references

This package uses the Azure REST API for subscriptions.

``` yaml
swagger:
  subscriptions:
    target: apis/azure
    source: https://github.com/Azure/azure-rest-api-specs/tree/master/specification/subscription/resource-manager/
    inputs: 
    - Microsoft.Subscription/stable/2016-06-01/subscriptions.json
    extra:
      auth:
        tenant: '{{ azure.tenant }}'
        resource: https://management.azure.com/
        client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46
```
