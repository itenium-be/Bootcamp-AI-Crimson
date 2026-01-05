import * as path from 'path';
import * as fs from 'fs';
import { fileURLToPath } from 'url';
import { getContainerRuntimeClient } from 'testcontainers';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const STATE_FILE = path.join(__dirname, '.test-state.json');

export default async function globalTeardown() {
  try {
    if (!fs.existsSync(STATE_FILE)) {
      return;
    }

    const state = JSON.parse(fs.readFileSync(STATE_FILE, 'utf-8'));
    const client = await getContainerRuntimeClient();

    // Stop backend container
    if (state.backendContainerId) {
      console.log('Stopping backend container...');
      const backendContainer = client.container.getById(state.backendContainerId);
      await backendContainer.stop();
      await backendContainer.remove();
      console.log('Backend container stopped and removed');
    }

    // Stop PostgreSQL container
    if (state.postgresContainerId) {
      console.log('Stopping PostgreSQL container...');
      const postgresContainer = client.container.getById(state.postgresContainerId);
      await postgresContainer.stop();
      await postgresContainer.remove();
      console.log('PostgreSQL container stopped and removed');
    }

    // Remove network
    if (state.networkId) {
      console.log('Removing network...');
      const network = client.network.getById(state.networkId);
      await network.remove();
      console.log('Network removed');
    }

    fs.unlinkSync(STATE_FILE);
  } catch (error) {
    console.error('Error during teardown:', error);
  }
}
