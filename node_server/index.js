import http from 'http';
import express from 'express';
import { SimHost } from './simHost.js';
import { WsHub } from './wsHub.js';
import { commandsRouter } from './routes/commands.js';

const PORT = process.env.PORT ?? 3001;

const app = express();
app.use(express.json());

const simHost = new SimHost(16);
app.use('/api/commands', commandsRouter(simHost));

const server = http.createServer(app);
const hub = new WsHub(server, () => simHost.getSnapshot());
simHost.setHub(hub);

server.listen(PORT, () => {
  console.log(`CitySim server listening on :${PORT}`);
  simHost.resume();
});
