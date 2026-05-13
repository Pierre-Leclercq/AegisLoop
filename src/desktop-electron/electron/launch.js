const { spawn } = require('child_process');
const path = require('path');

const electronPath = require('electron');
const appDir = path.resolve(__dirname, '..');
const env = { ...process.env };

delete env.ELECTRON_RUN_AS_NODE;

const proc = spawn(electronPath, ['.'], {
  cwd: appDir,
  env,
  stdio: 'inherit',
});

proc.on('error', (err) => {
  console.error('[AegisLoop Desktop] Failed to launch Electron:', err);
  process.exit(1);
});

proc.on('exit', (code, signal) => {
  if (signal) {
    process.kill(process.pid, signal);
    return;
  }

  process.exit(code ?? 0);
});