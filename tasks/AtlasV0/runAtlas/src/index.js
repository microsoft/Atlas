"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const tl = require("vsts-task-lib/task");
const path = require("path");
function run() {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            var command = tl.getInput("command", true);
            var atlasPath = path.join(__dirname, '../bin/atlas.dll');
            var args = tl.getInput("arguments", false);
            var dotnetPath = tl.which("dotnet", true);
            var atlas = tl.tool(dotnetPath)
                .arg(atlasPath)
                .arg(command);
            atlas.line(args);
            return yield atlas.exec();
        }
        catch (err) {
            tl.setResult(tl.TaskResult.Failed, err.message);
        }
    });
}
run();
//# sourceMappingURL=index.js.map