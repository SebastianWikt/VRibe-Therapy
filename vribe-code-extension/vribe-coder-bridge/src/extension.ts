// import * as vscode from 'vscode';
// import { WebSocketServer, WebSocket } from 'ws';

// let wss: WebSocketServer;

// export function activate(context: vscode.ExtensionContext) {
//     console.log('VRibe Coder Bridge (MOCK MODE) is now active!');

//     // 1. START WEBSOCKET SERVER (Port 8080)
//     wss = new WebSocketServer({ port: 8080 });
//     wss.on('connection', (ws) => {
//         console.log('Unity Quest 3 Connected to Mock Bridge!');
//     });

//     // 2. MOCK ARDUINO DATA (Random Focus Score 0-100)
//     // This replaces the SerialPort logic until you're ready to plug in.
//     const mockInterval = setInterval(() => {
//         const mockFocusScore = Math.floor(Math.random() * 101);
//         broadcastToUnity({ type: "EEG_DATA", value: mockFocusScore });
//     }, 100);

//     // 3. COMMAND TO LAUNCH COMPUTER VISION (MOOD)
//     let visionCommand = vscode.commands.registerCommand('vribe.startVision', () => {
//         const panel = vscode.window.createWebviewPanel(
//             'vribeVision', 'VRibe Mood Monitor',
//             vscode.ViewColumn.Two, 
//             { enableScripts: true }
//         );

//         panel.webview.html = getWebviewContent();

//         panel.webview.onDidReceiveMessage(message => {
//             if (message.command === 'moodUpdate') {
//                 broadcastToUnity({ type: "MOOD_DATA", value: message.value });
//             }
//         });
//     });

//     context.subscriptions.push(visionCommand);
    
//     // Cleanup the interval when the extension is deactivated
//     context.subscriptions.push({ dispose: () => clearInterval(mockInterval) });
// }

// // 4. THE BROADCAST ENGINE
// function broadcastToUnity(data: any) {
//     if (!wss) return;
//     const message = JSON.stringify(data);
//     wss.clients.forEach((client) => {
//         if (client.readyState === WebSocket.OPEN) {
//             client.send(message);
//         }
//     });
// }

// // 5. THE WEBVIEW (MOOD AI) CONTENT
// function getWebviewContent() {
//     return `<!DOCTYPE html>
//     <html>
//     <body style="background: #111; color: #0f0; font-family: monospace; text-align: center;">
//         <h2>VRibe Vision AI</h2>
//         <video id="video" width="320" height="240" autoplay muted style="border-radius: 8px; transform: scaleX(-1);"></video>
//         <div id="status" style="margin-top: 10px;">Loading Face Models...</div>
//         <script type="module">
//             import * as faceapi from 'https://cdn.jsdelivr.net/npm/@vladmandic/face-api/dist/face-api.esm.js';
//             const video = document.getElementById('video');
//             const status = document.getElementById('status');

//             async function init() {
//                 const MODEL_URL = 'https://raw.githubusercontent.com/vladmandic/face-api/master/model/';
//                 await faceapi.nets.tinyFaceDetector.loadFromUri(MODEL_URL);
//                 await faceapi.nets.faceExpressionNet.loadFromUri(MODEL_URL);
                
//                 const stream = await navigator.mediaDevices.getUserMedia({ video: {} });
//                 video.srcObject = stream;
//                 status.innerText = "Vision Active - Analyzing Mood";

//                 setInterval(async () => {
//                     const detect = await faceapi.detectSingleFace(video, new faceapi.TinyFaceDetectorOptions()).withFaceExpressions();
//                     if (detect) {
//                         const mood = Object.keys(detect.expressions).reduce((a, b) => detect.expressions[a] > detect.expressions[b] ? a : b);
//                         status.innerText = "Current Mood: " + mood.toUpperCase();
//                         const vscode = acquireVsCodeApi();
//                         vscode.postMessage({ command: 'moodUpdate', value: mood });
//                     }
//                 }, 500);
//             }
//             init();
//         </script>
//     </body>
//     </html>`;
// }

// export function deactivate() {
//     if (wss) wss.close();
// }

import * as vscode from 'vscode';
import { WebSocketServer, WebSocket } from 'ws';

let wss: WebSocketServer;

export function activate(context: vscode.ExtensionContext) {
    console.log('VRibe Coder Bridge is now active (Mock Mode)!');

    // 1. START WEBSOCKET SERVER (Port 8080)
    try {
        wss = new WebSocketServer({ port: 8080 });
        wss.on('connection', (ws) => {
            console.log('Unity Client Connected!');
            ws.send(JSON.stringify({ type: "SYSTEM", value: "Bridge Connected" }));
        });
    } catch (e) {
        vscode.window.showErrorMessage("Could not start WebSocket server. Port 8080 might be in use.");
    }

    // 2. MOCK EEG DATA GENERATOR
    // This simulates the Arduino sending data every 100ms
    const mockDataInterval = setInterval(() => {
        const mockFocusScore = Math.floor(Math.random() * 100);
        broadcastToUnity({ type: "EEG_DATA", value: mockFocusScore });
    }, 100);

    // 3. REGISTER THE VISION COMMAND
    let visionCommand = vscode.commands.registerCommand('vribe.startVision', () => {
        const panel = vscode.window.createWebviewPanel(
            'vribeVision', 
            'VRibe Mood Monitor',
            vscode.ViewColumn.Two, 
            { enableScripts: true }
        );

        panel.webview.html = getWebviewContent();

        // Handle mood updates from the AI in the Webview
        panel.webview.onDidReceiveMessage(message => {
            if (message.command === 'moodUpdate') {
                broadcastToUnity({ type: "MOOD_DATA", value: message.value });
            }
        });
    });

    context.subscriptions.push(visionCommand);
    context.subscriptions.push({ dispose: () => clearInterval(mockDataInterval) });
}

// HELPER: BROADCAST TO UNITY
function broadcastToUnity(data: any) {
    if (!wss) return;
    const message = JSON.stringify(data);
    wss.clients.forEach((client) => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(message);
        }
    });
}

// HELPER: WEBVIEW HTML (MOOD AI)
function getWebviewContent() {
    return `<!DOCTYPE html>
    <html>
    <body style="background: #111; color: #0f0; font-family: monospace; text-align: center;">
        <h3>VRibe Vision AI</h3>
        <video id="video" width="320" height="240" autoplay muted style="border-radius: 8px; transform: scaleX(-1);"></video>
        <div id="status" style="margin-top: 10px;">Loading Face Models...</div>
        <script type="module">
            import * as faceapi from 'https://cdn.jsdelivr.net/npm/@vladmandic/face-api/dist/face-api.esm.js';
            const video = document.getElementById('video');
            const status = document.getElementById('status');

            async function init() {
                const MODEL_URL = 'https://raw.githubusercontent.com/vladmandic/face-api/master/model/';
                await faceapi.nets.tinyFaceDetector.loadFromUri(MODEL_URL);
                await faceapi.nets.faceExpressionNet.loadFromUri(MODEL_URL);
                
                const stream = await navigator.mediaDevices.getUserMedia({ video: {} });
                video.srcObject = stream;
                status.innerText = "AI Active - Detecting Mood";

                setInterval(async () => {
                    const detect = await faceapi.detectSingleFace(video, new faceapi.TinyFaceDetectorOptions()).withFaceExpressions();
                    if (detect) {
                        const mood = Object.keys(detect.expressions).reduce((a, b) => detect.expressions[a] > detect.expressions[b] ? a : b);
                        status.innerText = "Mood: " + mood.toUpperCase();
                        const vscode = acquireVsCodeApi();
                        vscode.postMessage({ command: 'moodUpdate', value: mood });
                    }
                }, 500);
            }
            init();
        </script>
    </body>
    </html>`;
}

export function deactivate() {
    if (wss) wss.close();
}