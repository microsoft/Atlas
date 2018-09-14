"use strict";

import tl = require('vsts-task-lib/task');
import trm = require('vsts-task-lib/toolrunner');
import * as toolLib from 'vsts-task-tool-lib/tool';
import path = require("path");
import fetch = require("node-fetch");

import fs = require("fs");
import * as os from "os";

async function run() {

    let atlasVersion = tl.getInput("atlasVersion", true).trim();

    let checkLatestVersion = tl.getInput("checkLatestVersion", false);

    if (checkLatestVersion) {
        console.log("Determining latest version available");
        let response = await fetch("https://sa2fitssy3mz7ig.blob.core.windows.net/downloads/latest.json");
        let latest = await response.json();
        if (os.type() == 'Linux') {
            atlasVersion = latest["linux-x64"].version;
        }
        else {
            atlasVersion = latest["win10-x64"].version;
        }
    }

    console.log("Looking for cached tool: atlas " + atlasVersion);
    let toolPath = toolLib.findLocalTool("atlas", atlasVersion);
    if (!toolPath) {
        if (os.type() == 'Linux') {
            let downloadUrl = "https://ep2fitssy3mz7ig.azureedge.net/downloads/atlas-" + atlasVersion + "-linux-x64.tar.gz";
            let downloadPath = await toolLib.downloadTool(downloadUrl);
            let extractPath =  await toolLib.extractTar(downloadPath);
            let cachePath = await toolLib.cacheDir(extractPath, "atlas", atlasVersion);
            toolPath = cachePath;
        } else {
            let downloadUrl = "https://ep2fitssy3mz7ig.azureedge.net/downloads/atlas-" + atlasVersion + "-win10-x64.zip";
            let downloadPath = await toolLib.downloadTool(downloadUrl);
            let extractPath =  await toolLib.extractZip(downloadPath);
            let cachePath = await toolLib.cacheDir(extractPath, "atlas", atlasVersion);
            toolPath = cachePath;
        }
    }

    if (os.type() == 'Linux') {
        let atlasPath = path.join(toolPath, "atlas");
        if (fs.existsSync(atlasPath)) {
            fs.chmodSync(atlasPath, "777");
        }
    }

    toolLib.prependPath(toolPath);

    tl.setVariable('ATLAS_ROOT', toolPath);
}

// tl.setResourcePath(path.join(__dirname, '..', 'task.json'));

run()
    .then(() => tl.setResult(tl.TaskResult.Succeeded, ""))
    .catch((error) => tl.setResult(tl.TaskResult.Failed, error.message));
