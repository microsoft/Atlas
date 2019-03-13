
# Sub-workflows

Demonstrates now a workflow can execute nested workflows

Also demonstrates now a readme can declare references to non-nested workflows.

And demonstrates how workflows can be from an absolute source instead of a relative one.

``` yaml
workflows:
  # source defaults to the location of the current package or sub-package
  local:
    inputs: [101-messages]

  # or you can include packages from an absolute path or remote location 
  library:
    source: https://github.com/Microsoft/Atlas/tree/master/examples/
    inputs: [102-yaml, 301-rest]
```
