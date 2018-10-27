
## Atlas release publishing workflow

Performs the following actions

* Creates or updates a blob storage account and associated CDN

* Uploads latest build binary files to blob storage

* Creates or updates `latest.json` with the current version information

* Writes out `sleet-push.sh` and `sleet-push.cmd` script files

The release pipeline is expected to execute the sleet-push script file, which will push the latest nuget 
packages to the storage account. That storage account acts as a static-file NuGet feed.

## To run locally

Create a `values.yaml` in a working directory with at least the following information:
```
azure:
  tenant: <GUID> or <TENANT.onmicrosoft.com>
  subscription: <GUID>

vsts:
  account: <ACCOUNTNAME> # the part appears in the https://<ACCOUNTNAME>.visualstudio.com
```

Run the following command 
`atlas deploy path/to/examples/401-linux-agent-pool --set vsts.token=[PAT]`

See the `examples/401-linux-agent-pool/values.yaml` for other settings you would want to override.


### API references

This workflow uses the Azure REST API to create a Resource Group and Azure Resource Manager deployment,
and the Storage REST API to list keys in order to upload files.

``` yaml
swagger:

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
      tenant: '{{ request.auth.tenant }}'
      resource: https://management.azure.com/
      client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46

- target: apis/azure
  source: https://github.com/Azure/azure-rest-api-specs/tree/master/specification/storage/resource-manager/
  inputs: 
  - Microsoft.Storage/stable/2018-07-01/storage.json
  - Microsoft.Storage/stable/2018-07-01/blob.json
  - Microsoft.Storage/preview/2018-03-01-preview/managementpolicy.json
  extra:
    auth:
      tenant: '{{ request.auth.tenant }}'
      resource: https://management.azure.com/
      client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46

```
