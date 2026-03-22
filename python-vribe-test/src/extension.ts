// // The module 'vscode' contains the VS Code extensibility API
// // Import the module and reference it with the alias vscode in your code below
// import * as vscode from 'vscode';

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

import * as vscode from 'vscode';
import { PythonShell } from 'python-shell';
import * as path from 'path';

export function activate(context: vscode.ExtensionContext) {
    console.log('VRibe Bridge Active!');

        // 1. Setup Paths
    let scriptPath = path.join(context.extensionPath, 'python', 'vision_backend.py');

    // 2. The Interpreter (The .myenv python inside WSL)
    // Note: We use 'wsl' as the command, and pass the path to the venv as the first argument
    const venvPythonPath = '/home/matth/Documents/CS/Projects/pancaking/python-vribe-test/.myenv/bin/python3';

    let pyshell = new PythonShell(scriptPath, {
        mode: 'json',
        pythonPath: 'wsl', // Tell Windows to use the WSL subsystem
        pythonOptions: ['-u'],
        // We tell WSL to run the specific python inside your virtual env
        args: [venvPythonPath] 
    });

    // 2. Register the Command
    let disposable = vscode.commands.registerCommand('python-vribe-test.startVision', () => {
        const panel = vscode.window.createWebviewPanel(
            'vribeVision', 'VRibe Vision Monitor',
            vscode.ViewColumn.Two, { enableScripts: true }
        );

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
        } catch (err) {
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