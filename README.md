

# Atlas

Atlas is a tool for automating the deployment, configuration, and maintenance of DevOps engineering 
systems. An Atlas workflow revolves around making the appropriate REST API calls to VSTS, 
Active Directory, and Azure Resource Manager. It can be run interactively from the command line, 
or can be run entirely unattended as part of a VSTS build or release defintion.

There is a REST API for everything. With Atlas you can make the configuration of everything from CI/CD to
production servers consistent, reproducable, and reviewable by capturing them as source controlled templates.

# Features

* [YAML](http://yaml.org/) or [JSON](http://json.org/) syntax to define workflows and input parameters

* [Handlebars](http://handlebarsjs.com/) template engine enables workflows to be highly flexible

* [JMESPath](http://jmespath.org/) provides query language for inputs, outputs, and data transformations

* Extensively detailed log output and safe `--dry-run` support simplify troubleshooting

* From the command line, REST API calls are secured via interactive Azure AD OAUTH sign-in

* From the build or release definitions, REST API calls are secured by [VSTS service connection to Azure](https://docs.microsoft.com/en-us/vsts/pipelines/library/service-endpoints?view=vsts)


# Contributing

This project welcomes contributions and suggestions. Most contributions require you to
agree to a Contributor License Agreement (CLA) declaring that you have the right to,
and actually do, grant us the rights to use your contribution. For details, visit
https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need
to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the
instructions provided by the bot. You will only need to do this once across all repositories using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Reporting Security Issues

Security issues and bugs should be reported privately, via email, to the Microsoft Security
Response Center (MSRC) at [secure@microsoft.com](mailto:secure@microsoft.com). You should
receive a response within 24 hours. If for some reason you do not, please follow up via
email to ensure we received your original message. Further information, including the
[MSRC PGP](https://technet.microsoft.com/en-us/security/dn606155) key, can be found in
the [Security TechCenter](https://technet.microsoft.com/en-us/security/default).

