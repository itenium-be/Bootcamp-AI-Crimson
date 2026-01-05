import { GenericContainer, Wait } from 'testcontainers';
import * as path from 'path';
import * as fs from 'fs';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const STATE_FILE = path.join(__dirname, '.test-state.json');

export default async function globalSetup() {
  // Option 1: Use a locally running backend (faster for local dev)
  const backendUrl = process.env.BACKEND_URL;
  if (backendUrl) {
    console.log(`Using existing backend at ${backendUrl}`);
    const state = { apiUrl: backendUrl, containerId: null };
    fs.writeFileSync(STATE_FILE, JSON.stringify(state, null, 2));
    return;
  }

  // Option 2: Start backend in Docker using Testcontainers
  console.log('Starting backend container...');

  const nugetUser = process.env.NUGET_USER;
  const nugetToken = process.env.NUGET_TOKEN;

  if (!nugetUser || !nugetToken) {
    throw new Error(
      'Missing NUGET_USER or NUGET_TOKEN environment variables.\n\n' +
      'Option 1 - Use Docker (requires GitHub Packages auth):\n' +
      '  $env:NUGET_USER="your-github-username"\n' +
      '  $env:NUGET_TOKEN="your-github-pat-with-read:packages"\n' +
      '  npm run test:e2e\n\n' +
      'Option 2 - Use locally running backend (faster for local dev):\n' +
      '  # Start the backend manually first, then:\n' +
      '  $env:BACKEND_URL="https://localhost:5001"\n' +
      '  npm run test:e2e'
    );
  }

  const backendPath = path.resolve(__dirname, '../../backend');

  console.log('Building backend Docker image (this may take a few minutes)...');

  // Build and start the container
  const container = await GenericContainer.fromDockerfile(backendPath)
    .withBuildArgs({
      NUGET_USER: nugetUser,
      NUGET_TOKEN: nugetToken,
    })
    .build('skillforge-backend-test', { deleteOnExit: false });

  const startedContainer = await container
    .withExposedPorts(8080)
    .withEnvironment({
      ASPNETCORE_ENVIRONMENT: 'Development',
      ConnectionStrings__DefaultConnection: 'Data Source=:memory:',
    })
    .withWaitStrategy(Wait.forHttp('/health', 8080).forStatusCode(200))
    .start();

  const apiPort = startedContainer.getMappedPort(8080);
  const apiHost = startedContainer.getHost();
  const apiUrl = `http://${apiHost}:${apiPort}`;

  console.log(`Backend started at ${apiUrl}`);

  // Save state for tests and teardown
  const state = {
    containerId: startedContainer.getId(),
    apiUrl,
  };

  fs.writeFileSync(STATE_FILE, JSON.stringify(state, null, 2));
}
