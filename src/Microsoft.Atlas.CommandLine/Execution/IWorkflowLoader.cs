using System;
using System.IO;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.Models.Workflow;
using Microsoft.Atlas.CommandLine.Templates;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public interface IWorkflowLoader
    {
        (ITemplateEngine templateEngine, WorkflowModel workflow, object model) Load(IBlueprintPackage blueprint, object values, Action<string, Action<TextWriter>> generateOutput);
    }
}
