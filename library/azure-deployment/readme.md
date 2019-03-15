
# Azure resource manager template deployment

Deploys an ARM template to Azure.

Waits until the deployment has succeeded or fails.

``` yaml
info:
  title: azure-deployment
  version: 0.1
  description: Deploys an ARM template to Azure
  contact:
    name: Microsoft
    url: https://github.com/Microsoft/Atlas/issues/new
  license:
    name: MIT
    url: https://github.com/Microsoft/Atlas/blob/master/LICENSE
```

## Examples

### Example 1: Invoking as a sub-workflow

**readme.md**
````
# My Workflow

I am importing the following sub-workflows from github.

It is a good idea to replace 'master' with a sha.

``` yaml
workflows:
  github-atlas-library:
    source: https://github.com/Microsoft/Atlas/tree/master/library
    inputs: 
    - azure-deployment
```
````

**values.yaml**
```
azure:
  tenant: <YOUR-AAD-TENANT-ID>
  subscription: <YOUR-SUBSCRIPTION-ID>
  resourceGroup: <TARGET-RESOURCE-GROUP>
  location: <TARGET-AZURE-LOCATION>
```

**workflow.yaml**
``` 
operations:
- message: render arm template and store results
  template: my/azuredeploy.json
  output: 
    my-azuredeploy: ( result )

- message: render parameters and store results
  template: my/azuredeploy.parameters.json
  output: 
    my-azuredeploy-parameters: ( result.parameters )

- workflow: workflows/azure-deployment
  values:
    azure: ( azure )
    deploymentName: my-deployment-{{ guid (datetime add="PT0S") }} # random suffix on deployment name preserves history
    deployment:
      parameters: ( my-azuredeploy-parameters )
      template: ( my-azuredeploy )
  output:
    my-outputs: ( result.properties.outputs ) # any arm "outputs" stored here
```

### Example 2: Executing as a standalone workflow

**my-values.yaml**
```
azure:
  tenant: <YOUR-AAD-TENANT-ID>
  subscription: <YOUR-SUBSCRIPTION-ID>
  resourceGroup: <TARGET-RESOURCE-GROUP>
  location: <TARGET-AZURE-LOCATION>

deployment:
  template: {
    "$schema": "https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
      "storageAccountType": {
        "type": "string",
        "defaultValue": "Standard_LRS"
      },
      "location": {
        "type": "string",
        "defaultValue": "[resourceGroup().location]",
        "metadata": {
          "description": "Location for all resources."
        }
      }
    },
    "variables": {
      "storageAccountName": "[concat('store', uniquestring(resourceGroup().id))]"
    },
    "resources": [
      {
        "type": "Microsoft.Storage/storageAccounts",
        "name": "[variables('storageAccountName')]",
        "location": "[parameters('location')]",
        "apiVersion": "2018-07-01",
        "sku": {
          "name": "[parameters('storageAccountType')]"
        },
        "kind": "StorageV2",
        "properties": {}
      }
    ],
    "outputs": {
      "storageAccountName": {
        "type": "string",
        "value": "[variables('storageAccountName')]"
      }
    }
  }
```

Deploying this template:

```
atlas deploy -f my-values.yaml https://github.com/Microsoft/Atlas/tree/master/library/azure-deployment
```

Deploying with debug details:

```
atlas deploy -f my-values.yaml --set deployment.debugSetting.detailLevel=requestContent,responseContent https://github.com/Microsoft/Atlas/tree/master/library/azure-deployment
```

## Dependencies

This workflow calls the following REST APIs

``` yaml
swagger:
  resources:
    target: apis/azure
    source: https://github.com/Azure/azure-rest-api-specs/tree/8cc682832ab95838806bf080152759c1898063da/specification/resources/resource-manager/
    inputs: 
    - Microsoft.Resources/stable/2018-05-01/resources.json
    extra:
      auth:
        tenant: "{{ azure.tenant }}"
        resource: https://management.azure.com/
        client: 04b07795-8ddb-461a-bbee-02f9e1bf7b46 # Azure CLI
```
