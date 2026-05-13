const { app, BrowserWindow } = require('electron');
const { spawn } = require('child_process');
const path = require('path');
const http = require('http');

const isDev = !app.isPackaged;
const openDevTools = process.env.AEGISLOOP_OPEN_DEVTOOLS === '1';
const autoCloseMs = Number.parseInt(process.env.AEGISLOOP_E2E_AUTO_CLOSE_MS ?? '', 10);
const API_PORT = 5100;
const API_READY_URL = `http://localhost:${API_PORT}/health`;
const repoRoot = path.resolve(__dirname, '..', '..', '..');

// --- Child process management ---
const childProcesses = [];
let mainWindow = null;
let isShuttingDown = false;

function logLifecycle(message, ...args) {
  console.log(`[AegisLoop Desktop] ${message}`, ...args);
}

function runDotnet(name, args) {
  return spawn('dotnet', args, {
    cwd: repoRoot,
    stdio: 'pipe',
  });
}

function pipeProcessLogs(name, proc) {
  proc.stdout.on('data', (data) => {
    console.log(`[${name}] ${data.toString().trim()}`);
  });

  proc.stderr.on('data', (data) => {
    console.error(`[${name}] ${data.toString().trim()}`);
  });
}

function buildBackend() {
  return new Promise((resolve, reject) => {
    const proc = runDotnet('Build', ['build', 'AegisLoop.sln']);

    pipeProcessLogs('Build', proc);

    proc.on('exit', (code) => {
      if (code === 0) {
        resolve();
      } else {
        reject(new Error(`Backend build failed with code ${code}`));
      }
    });
  });
}

function spawnBackend(name, projectPath) {
  const proc = runDotnet(name, ['run', '--no-build', '--project', projectPath]);

  pipeProcessLogs(name, proc);

  proc.on('exit', (code) => {
    console.log(`[${name}] exited with code ${code}`);
  });

  childProcesses.push({ name, proc });
  return proc;
}

function waitForApi(maxRetries = 30, intervalMs = 2000) {
  return new Promise((resolve, reject) => {
    let attempts = 0;
    const check = () => {
      attempts++;
      http.get(API_READY_URL, (res) => {
        if (res.statusCode === 200) {
          resolve(true);
        } else {
          retryOrFail();
        }
      }).on('error', () => {
        retryOrFail();
      });

      function retryOrFail() {
        if (attempts >= maxRetries) {
          reject(new Error(`API not ready after ${maxRetries} attempts`));
        } else {
          setTimeout(check, intervalMs);
        }
      }
    };
    check();
  });
}

function killChildren() {
  if (isShuttingDown) {
    return;
  }

  isShuttingDown = true;
  const processesToStop = [...childProcesses];
  childProcesses.length = 0;

  for (const { name, proc } of processesToStop) {
    if (proc.killed || proc.exitCode !== null) {
      continue;
    }

    console.log(`Stopping ${name} (PID ${proc.pid})...`);
    proc.kill('SIGTERM');
  }

  // Force kill after 5s if still running
  setTimeout(() => {
    for (const { name, proc } of processesToStop) {
      if (!proc.killed && proc.exitCode === null) {
        console.log(`Force killing ${name} (PID ${proc.pid})...`);
        proc.kill('SIGKILL');
      }
    }
  }, 5000);
}

// --- Window ---
function createWindow() {
  logLifecycle('Creating Electron window...');

  mainWindow = new BrowserWindow({
    width: 1280,
    height: 800,
    show: false,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
    },
  });

  mainWindow.once('ready-to-show', () => {
    logLifecycle('Window ready to show.');
    mainWindow?.show();
  });

  mainWindow.webContents.on('did-finish-load', () => {
    logLifecycle('Renderer finished loading.');
    if (Number.isFinite(autoCloseMs) && autoCloseMs > 0) {
      logLifecycle(`E2E auto-close scheduled in ${autoCloseMs} ms.`);
      setTimeout(() => {
        logLifecycle('E2E auto-close requested.');
        mainWindow?.close();
      }, autoCloseMs);
    }
  });

  mainWindow.webContents.on('did-fail-load', (_event, errorCode, errorDescription, validatedURL) => {
    console.error(`[AegisLoop Desktop] Renderer failed to load ${validatedURL}: ${errorCode} ${errorDescription}`);
  });

  mainWindow.webContents.on('render-process-gone', (_event, details) => {
    console.error('[AegisLoop Desktop] Renderer process gone:', details);
  });

  mainWindow.on('unresponsive', () => {
    console.warn('[AegisLoop Desktop] Window became unresponsive.');
  });

  mainWindow.on('close', () => {
    logLifecycle('Window close requested.');
  });

  mainWindow.on('closed', () => {
    logLifecycle('Window closed.');
    mainWindow = null;
  });

  if (isDev) {
    mainWindow.loadURL('http://localhost:5173');
    if (openDevTools) {
      mainWindow.webContents.openDevTools({ mode: 'detach' });
    }
  } else {
    mainWindow.loadFile(path.join(__dirname, '../dist/index.html'));
  }
}

// --- App lifecycle ---
app.whenReady().then(async () => {
  if (isDev) {
    // In dev mode, Electron orchestrates API + Worker
    const apiProject = path.join('src', 'AegisLoop.Api', 'AegisLoop.Api.csproj');
    const workerProject = path.join('src', 'AegisLoop.Worker', 'AegisLoop.Worker.csproj');

    try {
      console.log('[AegisLoop Desktop] Building backend...');
      await buildBackend();
      console.log('[AegisLoop Desktop] Backend build succeeded.');

      console.log('[AegisLoop Desktop] Starting API...');
      spawnBackend('API', apiProject);

      console.log('[AegisLoop Desktop] Starting Worker...');
      spawnBackend('Worker', workerProject);

      console.log('[AegisLoop Desktop] Waiting for API readiness...');
      await waitForApi();
      console.log('[AegisLoop Desktop] API is ready.');
    } catch (err) {
      console.error('[AegisLoop Desktop] API failed to start:', err.message);
    }
  }

  createWindow();
});

app.on('window-all-closed', () => {
  logLifecycle('All windows closed.');
  killChildren();
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('before-quit', () => {
  logLifecycle('Application is quitting.');
  killChildren();
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});