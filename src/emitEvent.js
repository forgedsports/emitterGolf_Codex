export default async function emitEvent({ eventType, payload, endpoint, emitType, wsRef }) {
  const message = { eventType, payload };
  console.log('Emit:', message, 'to', endpoint, 'via', emitType);

  if (!endpoint) return;

  if (emitType === 'websocket') {
    // Reuse or create WebSocket
    if (!wsRef.current || wsRef.current.url !== endpoint || wsRef.current.readyState > 1) {
      if (wsRef.current) wsRef.current.close();
      wsRef.current = new window.WebSocket(endpoint);
      wsRef.current.onopen = () => {
        wsRef.current.send(JSON.stringify(message));
      };
      wsRef.current.onerror = err => {
        console.warn('WebSocket error:', err);
      };
    } else if (wsRef.current.readyState === 1) {
      wsRef.current.send(JSON.stringify(message));
    }
  } else if (emitType === 'http') {
    try {
      await fetch(endpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(message),
      });
    } catch (e) {
      console.warn('HTTP POST error:', e);
    }
  }
} 