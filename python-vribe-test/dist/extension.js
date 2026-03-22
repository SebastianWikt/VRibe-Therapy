/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ([
/* 0 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


// // The module 'vscode' contains the VS Code extensibility API
// // Import the module and reference it with the alias vscode in your code below
// import * as vscode from 'vscode';
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.activate = activate;
// // This method is called when your extension is activated
// // Your extension is activated the very first time the command is executed
// export function activate(context: vscode.ExtensionContext) {
// 	// Use the console to output diagnostic information (console.log) and errors (console.error)
// 	// This line of code will only be executed once when your extension is activated
// 	console.log('Congratulations, your extension "python-vribe-test" is now active!');
// 	// The command has been defined in the package.json file
// 	// Now provide the implementation of the command with registerCommand
// 	// The commandId parameter must match the command field in package.json
// 	const disposable = vscode.commands.registerCommand('python-vribe-test.helloWorld', () => {
// 		// The code you place here will be executed every time your command is executed
// 		// Display a message box to the user
// 		vscode.window.showInformationMessage('Hello World from python-vribe-test!');
// 	});
// 	context.subscriptions.push(disposable);
// }
// // This method is called when your extension is deactivated
// export function deactivate() {}
const vscode = __importStar(__webpack_require__(1));
const python_shell_1 = __webpack_require__(2);
const path = __importStar(__webpack_require__(6));
function activate(context) {
    console.log('VRibe Bridge Active!');
    // 1. Setup Paths
    let scriptPath = path.join(context.extensionPath, 'python', 'vision_backend.py');
    // 2. The Interpreter (The .myenv python inside WSL)
    // Note: We use 'wsl' as the command, and pass the path to the venv as the first argument
    const venvPythonPath = '/home/matth/Documents/CS/Projects/pancaking/python-vribe-test/.myenv/bin/python3';
    let pyshell = new python_shell_1.PythonShell(scriptPath, {
        mode: 'json',
        pythonPath: 'wsl', // Tell Windows to use the WSL subsystem
        pythonOptions: ['-u'],
        // We tell WSL to run the specific python inside your virtual env
        args: [venvPythonPath]
    });
    // 2. Register the Command
    let disposable = vscode.commands.registerCommand('python-vribe-test.startVision', () => {
        const panel = vscode.window.createWebviewPanel('vribeVision', 'VRibe Vision Monitor', vscode.ViewColumn.Two, { enableScripts: true });
        panel.webview.html = getWebviewContent();
        // 3. Heartbeat: Ask Python for data every 500ms
        const timer = setInterval(() => {
            pyshell.send({ command: "get_data" });
        }, 500);
        // 4. Relay: Pass Python messages to the Webview
        pyshell.on('message', (message) => {
            try {
                // If message is already an object (because of mode: 'json'), just send it
                if (message && typeof message === 'object') {
                    panel.webview.postMessage(message);
                }
            }
            catch (err) {
                console.log("Skipping non-JSON line from Python");
            }
        });
        panel.onDidDispose(() => clearInterval(timer));
    });
    context.subscriptions.push(disposable);
}
function getWebviewContent() {
    return `
    <html>
    <body style="background:#1e1e1e; color:white; font-family:sans-serif; text-align:center;">
        <h2>VRibe Vision Status</h2>
        <div id="mood" style="font-size:3em; padding:20px; border:4px solid #007acc; border-radius:15px;">--</div>
        <script>
            window.addEventListener('message', event => {
                document.getElementById('mood').innerText = event.data.mood;
                document.getElementById('mood').style.borderColor = 
                    event.data.mood === 'Focused' ? '#4ec9b0' : '#f44747';
            });
        </script>
    </body>
    </html>`;
}


/***/ }),
/* 1 */
/***/ ((module) => {

module.exports = require("vscode");

/***/ }),
/* 2 */
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.PythonShell = exports.NewlineTransformer = exports.PythonShellErrorWithLogs = exports.PythonShellError = void 0;
const events_1 = __webpack_require__(3);
const child_process_1 = __webpack_require__(4);
const os_1 = __webpack_require__(5);
const path_1 = __webpack_require__(6);
const stream_1 = __webpack_require__(7);
const fs_1 = __webpack_require__(8);
const util_1 = __webpack_require__(9);
function toArray(source) {
    if (typeof source === 'undefined' || source === null) {
        return [];
    }
    else if (!Array.isArray(source)) {
        return [source];
    }
    return source;
}
/**
 * adds arguments as properties to obj
 */
function extend(obj, ...args) {
    Array.prototype.slice.call(arguments, 1).forEach(function (source) {
        if (source) {
            for (let key in source) {
                obj[key] = source[key];
            }
        }
    });
    return obj;
}
/**
 * gets a random int from 0-10000000000
 */
function getRandomInt() {
    return Math.floor(Math.random() * 10000000000);
}
const execPromise = (0, util_1.promisify)(child_process_1.exec);
class PythonShellError extends Error {
}
exports.PythonShellError = PythonShellError;
class PythonShellErrorWithLogs extends PythonShellError {
}
exports.PythonShellErrorWithLogs = PythonShellErrorWithLogs;
/**
 * Takes in a string stream and emits batches seperated by newlines
 */
class NewlineTransformer extends stream_1.Transform {
    _transform(chunk, encoding, callback) {
        let data = chunk.toString();
        if (this._lastLineData)
            data = this._lastLineData + data;
        const lines = data.split(os_1.EOL);
        this._lastLineData = lines.pop();
        //@ts-ignore this works, node ignores the encoding if it's a number
        lines.forEach(this.push.bind(this));
        callback();
    }
    _flush(done) {
        if (this._lastLineData)
            this.push(this._lastLineData);
        this._lastLineData = null;
        done();
    }
}
exports.NewlineTransformer = NewlineTransformer;
/**
 * An interactive Python shell exchanging data through stdio
 * @param {string} script    The python script to execute
 * @param {object} [options] The launch options (also passed to child_process.spawn)
 * @param [stdoutSplitter] Optional. Splits stdout into chunks, defaulting to splitting into newline-seperated lines
 * @param [stderrSplitter] Optional. splits stderr into chunks, defaulting to splitting into newline-seperated lines
 * @constructor
 */
class PythonShell extends events_1.EventEmitter {
    /**
     * spawns a python process
     * @param scriptPath path to script. Relative to current directory or options.scriptFolder if specified
     * @param options
     * @param stdoutSplitter Optional. Splits stdout into chunks, defaulting to splitting into newline-seperated lines
     * @param stderrSplitter Optional. splits stderr into chunks, defaulting to splitting into newline-seperated lines
     */
    constructor(scriptPath, options, stdoutSplitter = null, stderrSplitter = null) {
        super();
        /**
         * returns either pythonshell func (if val string) or custom func (if val Function)
         */
        function resolve(type, val) {
            if (typeof val === 'string') {
                // use a built-in function using its name
                return PythonShell[type][val];
            }
            else if (typeof val === 'function') {
                // use a custom function
                return val;
            }
        }
        if (scriptPath.trim().length == 0)
            throw Error("scriptPath cannot be empty! You must give a script for python to run");
        let self = this;
        let errorData = '';
        events_1.EventEmitter.call(this);
        options = extend({}, PythonShell.defaultOptions, options);
        let pythonPath;
        if (!options.pythonPath) {
            pythonPath = PythonShell.defaultPythonPath;
        }
        else
            pythonPath = options.pythonPath;
        let pythonOptions = toArray(options.pythonOptions);
        let scriptArgs = toArray(options.args);
        this.scriptPath = (0, path_1.join)(options.scriptPath || '', scriptPath);
        this.command = pythonOptions.concat(this.scriptPath, scriptArgs);
        this.mode = options.mode || 'text';
        this.formatter = resolve('format', options.formatter || this.mode);
        this.parser = resolve('parse', options.parser || this.mode);
        // We don't expect users to ever format stderr as JSON so we default to text mode
        this.stderrParser = resolve('parse', options.stderrParser || 'text');
        this.terminated = false;
        this.childProcess = (0, child_process_1.spawn)(pythonPath, this.command, options);
        ['stdout', 'stdin', 'stderr'].forEach(function (name) {
            self[name] = self.childProcess[name];
            self.parser && self[name] && self[name].setEncoding(options.encoding || 'utf8');
        });
        // Node buffers stdout&stderr in batches regardless of newline placement
        // This is troublesome if you want to recieve distinct individual messages
        // for example JSON parsing breaks if it recieves partial JSON
        // so we use newlineTransformer to emit each batch seperated by newline
        if (this.parser && this.stdout) {
            if (!stdoutSplitter)
                stdoutSplitter = new NewlineTransformer();
            // note that setting the encoding turns the chunk into a string
            stdoutSplitter.setEncoding(options.encoding || 'utf8');
            this.stdout.pipe(stdoutSplitter).on('data', (chunk) => {
                this.emit('message', self.parser(chunk));
            });
        }
        // listen to stderr and emit errors for incoming data
        if (this.stderrParser && this.stderr) {
            if (!stderrSplitter)
                stderrSplitter = new NewlineTransformer();
            // note that setting the encoding turns the chunk into a string
            stderrSplitter.setEncoding(options.encoding || 'utf8');
            this.stderr.pipe(stderrSplitter).on('data', (chunk) => {
                this.emit('stderr', self.stderrParser(chunk));
            });
        }
        if (this.stderr) {
            this.stderr.on('data', function (data) {
                errorData += '' + data;
            });
            this.stderr.on('end', function () {
                self.stderrHasEnded = true;
                terminateIfNeeded();
            });
        }
        else {
            self.stderrHasEnded = true;
        }
        if (this.stdout) {
            this.stdout.on('end', function () {
                self.stdoutHasEnded = true;
                terminateIfNeeded();
            });
        }
        else {
            self.stdoutHasEnded = true;
        }
        this.childProcess.on('error', function (err) {
            self.emit('error', err);
        });
        this.childProcess.on('exit', function (code, signal) {
            self.exitCode = code;
            self.exitSignal = signal;
            terminateIfNeeded();
        });
        function terminateIfNeeded() {
            if (!self.stderrHasEnded || !self.stdoutHasEnded || (self.exitCode == null && self.exitSignal == null))
                return;
            let err;
            if (self.exitCode && self.exitCode !== 0) {
                if (errorData) {
                    err = self.parseError(errorData);
                }
                else {
                    err = new PythonShellError('process exited with code ' + self.exitCode);
                }
                err = extend(err, {
                    executable: pythonPath,
                    options: pythonOptions.length ? pythonOptions : null,
                    script: self.scriptPath,
                    args: scriptArgs.length ? scriptArgs : null,
                    exitCode: self.exitCode
                });
                // do not emit error if only a callback is used
                if (self.listeners('pythonError').length || !self._endCallback) {
                    self.emit('pythonError', err);
                }
            }
            self.terminated = true;
            self.emit('close');
            self._endCallback && self._endCallback(err, self.exitCode, self.exitSignal);
        }
        ;
    }
    /**
     * checks syntax without executing code
     * @returns rejects promise w/ string error output if syntax failure
     */
    static checkSyntax(code) {
        return __awaiter(this, void 0, void 0, function* () {
            const randomInt = getRandomInt();
            const filePath = (0, os_1.tmpdir)() + path_1.sep + `pythonShellSyntaxCheck${randomInt}.py`;
            const writeFilePromise = (0, util_1.promisify)(fs_1.writeFile);
            return writeFilePromise(filePath, code).then(() => {
                return this.checkSyntaxFile(filePath);
            });
        });
    }
    static getPythonPath() {
        return this.defaultOptions.pythonPath ? this.defaultOptions.pythonPath : this.defaultPythonPath;
    }
    /**
     * checks syntax without executing code
     * @returns {Promise} rejects w/ stderr if syntax failure
     */
    static checkSyntaxFile(filePath) {
        return __awaiter(this, void 0, void 0, function* () {
            const pythonPath = this.getPythonPath();
            let compileCommand = `${pythonPath} -m py_compile ${filePath}`;
            return execPromise(compileCommand);
        });
    }
    /**
     * Runs a Python script and returns collected messages as a promise.
     * If the promise is rejected, the err will probably be of type PythonShellErrorWithLogs
     * @param scriptPath   The path to the script to execute
     * @param options  The execution options
     */
    static run(scriptPath, options) {
        return new Promise((resolve, reject) => {
            let pyshell = new PythonShell(scriptPath, options);
            let output = [];
            pyshell.on('message', function (message) {
                output.push(message);
            }).end(function (err) {
                if (err) {
                    err.logs = output;
                    reject(err);
                }
                else
                    resolve(output);
            });
        });
    }
    ;
    /**
     * Runs the inputted string of python code and returns collected messages as a promise. DO NOT ALLOW UNTRUSTED USER INPUT HERE!
     * @param code   The python code to execute
     * @param options  The execution options
     * @return a promise with the output from the python script
     */
    static runString(code, options) {
        // put code in temp file
        const randomInt = getRandomInt();
        const filePath = os_1.tmpdir + path_1.sep + `pythonShellFile${randomInt}.py`;
        (0, fs_1.writeFileSync)(filePath, code);
        return PythonShell.run(filePath, options);
    }
    ;
    static getVersion(pythonPath) {
        if (!pythonPath)
            pythonPath = this.getPythonPath();
        return execPromise(pythonPath + " --version");
    }
    static getVersionSync(pythonPath) {
        if (!pythonPath)
            pythonPath = this.getPythonPath();
        return (0, child_process_1.execSync)(pythonPath + " --version").toString();
    }
    /**
     * Parses an error thrown from the Python process through stderr
     * @param  {string|Buffer} data The stderr contents to parse
     * @return {Error} The parsed error with extended stack trace when traceback is available
     */
    parseError(data) {
        let text = '' + data;
        let error;
        if (/^Traceback/.test(text)) {
            // traceback data is available
            let lines = text.trim().split(os_1.EOL);
            let exception = lines.pop();
            error = new PythonShellError(exception);
            error.traceback = data;
            // extend stack trace
            error.stack += os_1.EOL + '    ----- Python Traceback -----' + os_1.EOL + '  ';
            error.stack += lines.slice(1).join(os_1.EOL + '  ');
        }
        else {
            // otherwise, create a simpler error with stderr contents
            error = new PythonShellError(text);
        }
        return error;
    }
    ;
    /**
     * Sends a message to the Python shell through stdin
     * Override this method to format data to be sent to the Python process
     * @returns {PythonShell} The same instance for chaining calls
     */
    send(message) {
        if (!this.stdin)
            throw new Error("stdin not open for writing");
        let data = this.formatter ? this.formatter(message) : message;
        if (this.mode !== 'binary')
            data += os_1.EOL;
        this.stdin.write(data);
        return this;
    }
    ;
    /**
     * Closes the stdin stream. Unless python is listening for stdin in a loop
     * this should cause the process to finish its work and close.
     * @returns {PythonShell} The same instance for chaining calls
     */
    end(callback) {
        if (this.childProcess.stdin) {
            this.childProcess.stdin.end();
        }
        this._endCallback = callback;
        return this;
    }
    ;
    /**
     * Sends a kill signal to the process
     * @returns {PythonShell} The same instance for chaining calls
     */
    kill(signal) {
        this.terminated = this.childProcess.kill(signal);
        return this;
    }
    ;
    /**
     * Alias for kill.
     * @deprecated
     */
    terminate(signal) {
        // todo: remove this next breaking release
        return this.kill(signal);
    }
}
exports.PythonShell = PythonShell;
// starting 2020 python2 is deprecated so we choose 3 as default
PythonShell.defaultPythonPath = process.platform != "win32" ? "python3" : "python";
PythonShell.defaultOptions = {}; //allow global overrides for options
// built-in formatters
PythonShell.format = {
    text: function toText(data) {
        if (!data)
            return '';
        else if (typeof data !== 'string')
            return data.toString();
        return data;
    },
    json: function toJson(data) {
        return JSON.stringify(data);
    }
};
//built-in parsers
PythonShell.parse = {
    text: function asText(data) {
        return data;
    },
    json: function asJson(data) {
        return JSON.parse(data);
    }
};
;
//# sourceMappingURL=index.js.map

/***/ }),
/* 3 */
/***/ ((module) => {

module.exports = require("events");

/***/ }),
/* 4 */
/***/ ((module) => {

module.exports = require("child_process");

/***/ }),
/* 5 */
/***/ ((module) => {

module.exports = require("os");

/***/ }),
/* 6 */
/***/ ((module) => {

module.exports = require("path");

/***/ }),
/* 7 */
/***/ ((module) => {

module.exports = require("stream");

/***/ }),
/* 8 */
/***/ ((module) => {

module.exports = require("fs");

/***/ }),
/* 9 */
/***/ ((module) => {

module.exports = require("util");

/***/ })
/******/ 	]);
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module is referenced by other modules so it can't be inlined
/******/ 	var __webpack_exports__ = __webpack_require__(0);
/******/ 	module.exports = __webpack_exports__;
/******/ 	
/******/ })()
;
//# sourceMappingURL=extension.js.map