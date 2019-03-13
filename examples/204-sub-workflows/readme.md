
# Sub-workflows

Demonstrates now a workflow can execute nested workflows

Also show now a `readme.md` like this one can declare references to non-nested workflows.

``` yaml
workflows:
  # source defaults to the location of the current package or sub-package
  local:
    inputs: [101-messages]

  # or you can include packages from an absolute path or remote location 
  github-examples:
    source: https://github.com/Microsoft/Atlas/tree/master/examples/
    inputs: [102-yaml, 301-rest]
```
