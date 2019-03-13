// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Atlas.CommandLine.Blueprints;
using Microsoft.Atlas.CommandLine.Models.Workflow;
using Microsoft.Atlas.CommandLine.Serialization;
using Microsoft.Atlas.CommandLine.Templates;
using Microsoft.Atlas.CommandLine.Templates.FileSystems;

namespace Microsoft.Atlas.CommandLine.Execution
{
    public class WorkflowLoader : IWorkflowLoader
    {
        private readonly ITemplateEngineFactory _templateEngineFactory;
        private readonly IYamlSerializers _serializers;

        public WorkflowLoader(
            ITemplateEngineFactory templateEngineFactory,
            IYamlSerializers serializers)
        {
            _templateEngineFactory = templateEngineFactory;
            _serializers = serializers;
        }

        public (ITemplateEngine templateEngine, WorkflowModel workflow, object model) Load(IBlueprintPackage blueprint, object values, Action<string, Action<TextWriter>> generateOutput)
        {
            var templateEngine = _templateEngineFactory.Create(new TemplateEngineOptions
            {
                FileSystem = new BlueprintPackageFileSystem(blueprint)
            });

            object model;

            var modelTemplate = "model.yaml";
            var modelExists = blueprint.Exists(modelTemplate);
            if (modelExists)
            {
                model = templateEngine.Render<object>(modelTemplate, values);
                if (values != null)
                {
                    model = MergeUtils.Merge(model, values);
                }
            }
            else
            {
                model = values;
            }

            var workflowTemplate = "workflow.yaml";
            var workflowContents = new StringBuilder();
            using (var workflowWriter = new StringWriter(workflowContents))
            {
                templateEngine.Render(workflowTemplate, model, workflowWriter);
            }

            // NOTE: the workflow is rendered BEFORE writing these output files because it may contain
            // calls to the "secret" helper which will redact tokens that might have been provided in values

            // write values to output folder
            generateOutput("values.yaml", writer => _serializers.YamlSerializer.Serialize(writer, values));

            if (modelExists)
            {
                // write normalized values to output folder
                generateOutput("model.yaml", writer => templateEngine.Render(modelTemplate, model, writer));
            }

            // write workflow to output folder
            generateOutput("workflow.yaml", writer => writer.Write(workflowContents.ToString()));

            var workflow = _serializers.YamlDeserializer.Deserialize<WorkflowModel>(new StringReader(workflowContents.ToString()));

            foreach (var generatedFile in blueprint.GetGeneratedPaths())
            {
                using (var generatedContent = blueprint.OpenText(generatedFile))
                {
                    generateOutput($"generated/{generatedFile}", writer => writer.Write(generatedContent.ReadToEnd()));
                }
            }

            return (templateEngine, workflow, model);
        }
    }
}
