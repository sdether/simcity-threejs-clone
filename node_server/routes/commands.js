import { Router } from 'express';

export function commandsRouter(simHost) {
  const router = Router();

  router.post('/placeBuilding', (req, res) => {
    const { x, y, type } = req.body;
    if (x == null || y == null || !type) {
      return res.status(400).json({ error: 'x, y, type required' });
    }
    console.log(`[CMD] placeBuilding  x=${x} y=${y} type=${type}`);
    simHost.placeBuilding(Number(x), Number(y), type);
    res.status(202).end();
  });

  router.post('/bulldoze', (req, res) => {
    const { x, y } = req.body;
    if (x == null || y == null) {
      return res.status(400).json({ error: 'x, y required' });
    }
    console.log(`[CMD] bulldoze       x=${x} y=${y}`);
    simHost.bulldoze(Number(x), Number(y));
    res.status(202).end();
  });

  router.post('/pause', (_req, res) => {
    console.log('[CMD] pause');
    simHost.pause();
    res.status(202).end();
  });

  router.post('/resume', (_req, res) => {
    console.log('[CMD] resume');
    simHost.resume();
    res.status(202).end();
  });

  router.post('/requestRefresh', (_req, res) => {
    console.log('[CMD] requestRefresh');
    simHost.requestRefresh();
    res.status(202).end();
  });

  router.post('/reset', (_req, res) => {
    console.log('[CMD] reset');
    simHost.reset();
    res.status(202).end();
  });

  return router;
}
