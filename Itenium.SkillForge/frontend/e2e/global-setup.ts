import { GenericContainer, Network, Wait } from 'testcontainers';
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
    const state = { apiUrl: backendUrl, backendContainerId: null, postgresContainerId: null, networkId: null };
    fs.writeFileSync(STATE_FILE, JSON.stringify(state, null, 2));
    return;
  }

  // Option 2: Start backend + PostgreSQL in Docker using Testcontainers
  console.log('Starting e2e test environment...');

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

  // Create a network for containers to communicate
  const network = await new Network().start();

  // Start PostgreSQL container
  console.log('Starting PostgreSQL container...');
  const postgresContainer = await new GenericContainer('postgres:17')
    .withNetwork(network)
    .withNetworkAliases('postgres')
    .withEnvironment({
      POSTGRES_USER: 'skillforge',
      POSTGRES_PASSWORD: 'skillforge',
      POSTGRES_DB: 'skillforge',
    })
    .withExposedPorts(5432)
    .withWaitStrategy(Wait.forListeningPorts())
    .start();

  console.log('PostgreSQL started');

  // Build backend Docker image
  const backendPath = path.resolve(__dirname, '../../backend');
  console.log('Building backend Docker image (this may take a few minutes)...');

  const backendImage = await GenericContainer.fromDockerfile(backendPath)
    .withBuildArgs({
      NUGET_USER: nugetUser,
      NUGET_TOKEN: nugetToken,
    })
    .build('skillforge-backend-test', { deleteOnExit: false });

  // Start backend container connected to PostgreSQL
  // Uses DOTNET_ENVIRONMENT=Docker from Dockerfile -> appsettings.Docker.json (Host=postgres)
  console.log('Starting backend container...');
  const backendContainer = await backendImage
    .withNetwork(network)
    .withExposedPorts(8080)
    .withStartupTimeout(120000)
    .withWaitStrategy(Wait.forHttp('/health/live', 8080).forStatusCode(200).withStartupTimeout(120000))
    .start();

  const apiPort = backendContainer.getMappedPort(8080);
  const apiHost = backendContainer.getHost();
  const apiUrl = `http://${apiHost}:${apiPort}`;

  console.log(`Backend started at ${apiUrl}`);

  // Save state for tests and teardown
  const state = {
    backendContainerId: backendContainer.getId(),
    postgresContainerId: postgresContainer.getId(),
    networkId: network.getId(),
    apiUrl,
  };

  fs.writeFileSync(STATE_FILE, JSON.stringify(state, null, 2));
}
