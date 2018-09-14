import tl = require('vsts-task-lib/task');
import trm = require('vsts-task-lib/toolrunner');
import path = require("path");

async function run() {
    try {
        var command = tl.getInput("command", true);
        var args = tl.getInput("arguments", false);

        var atlasPath = tl.which("atlas", true);
        
        var atlas = tl.tool(atlasPath)
            .arg(command);

        atlas.line(args);
    
        return await atlas.exec();
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();
