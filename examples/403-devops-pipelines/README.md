
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
