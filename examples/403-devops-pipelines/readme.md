
This example is more involved. Before it may be run you'll need to: 

* Have an Azure DevOps organization account
* Create a new project in that account
* Create a git repo in that project
* Write a values.yaml file in the current directory

At a minimum your values.yaml file in your must override the following:

```
devops:
  organization: {YOUR_ORGANIZATION_NAME}
  project: {YOUR_PROJECT_NAME}
  repository:
    code:
      name: {YOUR_REPO_NAME}
      branches:
        "refs/heads/master":
          "Required reviewers":
            settings:
              requiredReviewerIds:
              - {YOUR_REVIEWER_GROUP} # e.g. '[YOUR_PROJECT_NAME]\YOUR_PROJECT_NAME Team'
```

``` yaml


swagger:

  devops:
    target: apis/devops
    source: https://github.com/MicrosoftDocs/vsts-rest-api-specs/tree/master/specification/
    inputs: 
    - build/5.0/build.json
    - distributedTask/5.0/taskAgent.json
    - policy/5.0/policy.json
    - graph/5.0/graph.json
    - git/5.0/git.json
    - release/5.0/release.json
    extra:
      auth:
        resource: 499b84ac-1321-427f-aa17-267ca6975798
        client: e8f3cc86-b3b2-4ebb-867c-9c314925b384
```
