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

    // Only stop container if we started one (not when using local backend)
    if (state.containerId) {
      console.log('Stopping backend container...');
      const client = await getContainerRuntimeClient();
      const container = client.container.getById(state.containerId);
      await container.stop();
      await container.remove();
      console.log('Backend container stopped and removed');
    }

    fs.unlinkSync(STATE_FILE);
  } catch (error) {
    console.error('Error during teardown:', error);
  }
}
