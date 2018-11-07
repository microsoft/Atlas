using System;
using System.Collections.Generic;

namespace Microsoft.Atlas.Service.Core.ContainerInstances
{
    public class CreateInstanceContext
    {
        public CreateInstanceContext(
            string instanceName,
            string atlasVersion,
            IList<string> arguments)
        {
            InstanceName = instanceName;
            AtlasVersion = atlasVersion;
            Arguments = arguments;
        }

        public string InstanceName { get; }
        public string AtlasVersion { get; }
        public IList<string> Arguments { get; }

        public string ResourceId { get; private set; }

        public void SetResult(string resourceId)
        {
            ResourceId = resourceId;
        }
    }
}
