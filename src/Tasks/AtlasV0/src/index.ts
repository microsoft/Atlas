import tl = require('vsts-task-lib/task');
import trm = require('vsts-task-lib/toolrunner');
import path = require("path");
import inputs = require('./inputs');

async function run() {
    try {
        var parameters = await new inputs.TaskParameters().getParameters();
        
        var atlasPath = tl.which("atlas", true);

        if (!!parameters.azureSubscriptionEndpoint) {
            
            var accountAdd = tl.tool(atlasPath).arg([ 
                'account', 'add',
                '--name', parameters.azureSubscriptionEndpoint.connectedService,
                '--authority', parameters.azureSubscriptionEndpoint.environmentAuthorityUrl + parameters.azureSubscriptionEndpoint.tenantId,
                '--resource', parameters.azureSubscriptionEndpoint.endpointUrl,
                '--appid', parameters.azureSubscriptionEndpoint.servicePrincipalId,
                '--secret', parameters.azureSubscriptionEndpoint.servicePrincipalKey
            ]);

            await accountAdd.exec();
        }
        
        var atlas = tl.tool(atlasPath)
            .line(parameters.command);

        atlas.line(parameters.arguments);
    
        return await atlas.exec();
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();
