import tl = require('vsts-task-lib/task');
import trm = require('vsts-task-lib/toolrunner');
import mod = require('./taskmod');
import path = require("path");

async function run() {
    try {
        var command = tl.getInput("command", true);
        var atlasPath = path.join(__dirname, '../bin/atlas.dll');
        var args = tl.getInput("arguments", false);

        var dotnetPath = tl.which("dotnet", true);
        var atlas = tl.tool(dotnetPath)
            .arg(atlasPath)
            .arg(command);

        atlas.line(args);
    
        return await atlas.exec();
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();
