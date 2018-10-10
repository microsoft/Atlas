

# Atlas

[![Build Status]][Build Latest] 
[![Zip Status]][Zip Latest]
[![Tarball Status]][Tarball Latest]
[![Choco Status]][Choco Latest]

![Atlas Logo]

----

Atlas is a tool for automating the deployment, configuration, and maintenance of DevOps engineering systems. 
It can be run interactively from the command line, or can be run entirely unattended as part of an Azure DevOps (formerly known as VSTS) build or release definition.
An Atlas workflow revolves around making the appropriate REST API calls to [Azure DevOps][Azure DevOps REST API], [Active Directory][Azure AD REST API], and [Azure Resource Manager][Azure RM REST API]. 

There is a REST API for everything. 
With Atlas you can make the configuration of everything from CI/CD to production servers consistent, reproducible, and reviewable by capturing them as source controlled templates.

----

## Install

Atlas is currently under active development.

Daily builds of the Atlas CLI are available as self-contained downloads:

| Platform | [Master Branch (0.1)][Master Branch] | [Latest Build][Latest Json] |
|:------:|:------:|:------:|
| **Windows x64** | [Download latest zip][Zip Latest] | [![Zip Status]][Zip Latest] |
| **Linux x64** | [Download latest tar.gz][Zip Latest] | [![Tarball Status]][Tarball Latest] |

If you want to use a package manager:

#### Install global tool (Windows or Linux)

1. If `dotnet --version` isn't >= 2.1.300 then [install or upgrade .NET Core](https://www.microsoft.com/net/download/dotnet-core/2.1)
1. `dotnet tool install -g dotnet-atlas --add-source https://aka.ms/atlas-ci/index.json`

#### Install using Chocolatey (Windows)
1. If `choco` command is not available then [install Chocolatey](https://chocolatey.org/install#installing-chocolatey)
1. `choco install atlas-cli -s https://www.myget.org/F/atlas-ci` 

## Getting Started

An existing workflow can be executed directly from a public web server. You 
can run any of the [examples][Atlas Examples] in this repository with the `atlas deploy` command:

```
atlas deploy https://github.com/Microsoft/Atlas/tree/master/examples/101-messages
```

#### Creating a new workflow

To create a new workflow, from a console window execute `mkdir demo` to create a new subfolder.

Add a `demo/workflow.yaml` file to declare operations:

```
operations:
- message: Running my workflow...
- message: {{ info.greeting }}, {{ info.name }}!
- message: "All values: {{ json . }}"
```

Add a `demo/values.yaml` file to declare defaults:

```
info:
  greeting: Hello
  name: World
```

Run it!

```
> atlas deploy demo --set info.name=Atlas

Atlas

  - Running my workflow...

  - Hello, Atlas!

  - All values: {"info": {"greeting": "Hello", "name": "Atlas"}}
```

#### Exploring the examples

You can also clone the Atlas GitHub repo to explore the [examples][Atlas Examples] and see
kinds of operations Atlas can perform.

```
git clone https://github.com/Microsoft/Atlas.git
cd Atlas/examples
atlas deploy 101-messages
```

## Features

* [YAML] or [JSON] syntax to define workflows and input parameters

* [Handlebars] template engine enables workflows to be highly flexible

* [JMESPath] provides query language for inputs, outputs, and data transformations

* Works cross-platform as a .NET Core executable

* Invokes any [Azure RM][Azure RM REST API], [Azure AD][Azure AD REST API], or [Azure DevOps][Azure DevOps REST API] REST API 

* From the command line, REST API calls are secured via interactive Active Directory login, similar to `az login`

* From an Azure DevOps build or release definition, REST API calls are secured via [Azure DevOps service connection to Azure](https://docs.microsoft.com/en-us/vsts/pipelines/library/service-endpoints?view=vsts)

* Renders output values and additional templated files to a target folder

* Operations support conditional executions, retries and looping, and can throwing detailed exceptions

* Extensively detailed log output and safe `--dry-run` support simplify troubleshooting

* Values which are declared secret are redacted (replaced with xxxx) when written to console output and log files

## Limitations

* Does not allow arbitrary code or command-line execution in order to limit what can be done to the machine executing a workflow

* Currently designed for Active Directory authentication for Azure and Azure DevOps resources

* Not yet available as a class library package

## Goals

* Packing workflows into zip or tarball archive files, publishing and executing workflows from feed locations

* Establishing a repository for collaboration on common in-progress and stable workflows, and default location for common workflows

* Shared workflows for larger scenarios, e.g. ASP.NET Core services on Kubernetes with Azure DevOps CI/CD, Azure VM clusters, Azure DNS, ATM, and ALB for geo-redundant load balancing and service routing

----

## System Requirements

#### Running Atlas

Atlas runs on Windows and Linux. Windows 10 and Ubuntu 16.04 are the tested environments.

#### Building Atlas from source

Prerequisites:
* Required: Download and [install](https://www.microsoft.com/net/download/dotnet-core/2.1) the .NET Core SDK
* Optional: [Install](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2017) or [update](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2017) Visual Studio 2017
* Optional: Download and [install](https://code.visualstudio.com/Download) Visual Studio Code

To clone and build from source, run the following commands from a console window:

```
git clone https://github.com/Microsoft/Atlas.git
cd Atlas
build.cmd *or* ./build.sh
```

#### Running Atlas from source

To run locally from source, run the following commands:

```
dotnet restore
./atlas.sh
```

## Contributing

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

[Atlas Logo]: https://github.com/Microsoft/Atlas/raw/master/docs/icon-128.png
[Atlas Examples]: https://github.com/Microsoft/Atlas/tree/master/examples
[Handlebars]: http://handlebarsjs.com/
[YAML]: http://yaml.org/
[JSON]: http://json.org/
[JMESPath]: http://jmespath.org/
[Azure RM REST API]: https://docs.microsoft.com/en-us/rest/api/azure/
[Azure AD REST API]: https://docs.microsoft.com/en-us/rest/api/graphrbac/
[Azure DevOps REST API]: https://docs.microsoft.com/en-us/rest/api/vsts/?view=vsts-rest-5.0
[Build Status]: https://msasg.visualstudio.com/Falcon/_apis/build/status/Atlas-CI?branch=master
[Build Latest]: https://msasg.visualstudio.com/Falcon/_build/latest?definitionId=6598&branch=master
[Choco Status]: https://img.shields.io/myget/atlas-ci/vpre/atlas-cli.svg?label=choco
[Choco Latest]: #chocolatey
[Zip Status]: https://img.shields.io/badge/dynamic/json.svg?label=win-x64&url=https%3A%2F%2Fsa2fitssy3mz7ig.blob.core.windows.net%2Fdownloads%2Flatest.json&query=%24[%27win10-x64%27].version
[Zip Latest]: https://sa2fitssy3mz7ig.blob.core.windows.net/downloads/atlas-latest-win10-x64.zip
[Tarball Status]: https://img.shields.io/badge/dynamic/json.svg?label=linux-x64&url=https%3A%2F%2Fsa2fitssy3mz7ig.blob.core.windows.net%2Fdownloads%2Flatest.json&query=%24[%27linux-x64%27].version
[Tarball Latest]: https://sa2fitssy3mz7ig.blob.core.windows.net/downloads/atlas-latest-linux-x64.tar.gz
[Master Branch]: https://github.com/microsoft/atlas/tree/master
[Latest Json]: https://sa2fitssy3mz7ig.blob.core.windows.net/downloads/latest.json
