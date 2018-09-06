
This blueprint installs a linux build agent pool. It requires some extra input 
to run correctly 

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

