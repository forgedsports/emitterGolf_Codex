export default async function emitEvent({
  eventType,
  payload,
  endpoint,
  emitType,
  wsRef,
  onLog,
}) {
  const message = { eventType, payload };
  console.log('Emit:', message, 'to', endpoint, 'via', emitType);

  // initial log
  onLog?.({
    type: 'info',
    text: `Sending ${eventType} via ${emitType} to ${endpoint}`,
    eventType,
    payload,
  });

  if (!endpoint) {
    onLog?.({ type: 'error', text: 'No endpoint specified', eventType, payload });
    return;
  }

  if (emitType === 'websocket') {
    // Reuse or create WebSocket
    if (!wsRef.current || wsRef.current.url !== endpoint || wsRef.current.readyState > 1) {
      if (wsRef.current) wsRef.current.close();
      wsRef.current = new window.WebSocket(endpoint);
      wsRef.current.onopen = () => {
        wsRef.current.send(JSON.stringify(message));
        onLog?.({
          type: 'info',
          text: `WebSocket sent ${JSON.stringify(message)}`,
          eventType,
          payload,
        });
      };
      wsRef.current.onerror = err => {
        console.warn('WebSocket error:', err);
        onLog?.({
          type: 'error',
          text: `WebSocket error: ${err.message || err}`,
          eventType,
          payload,
        });
      };
      wsRef.current.onclose = evt => {
        if (evt.code !== 1000) {
          onLog?.({
            type: 'error',
            text: `WebSocket closed code ${evt.code}`,
            eventType,
            payload,
          });
        }
      };
    } else if (wsRef.current.readyState === 1) {
      wsRef.current.send(JSON.stringify(message));
      onLog?.({
        type: 'info',
        text: `WebSocket sent ${JSON.stringify(message)}`,
        eventType,
        payload,
      });
    }
  } else if (emitType === 'http') {
    try {
      const res = await fetch(endpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(message),
      });
      if (!res.ok) {
        onLog?.({
          type: 'error',
          text: `HTTP error ${res.status}`,
          eventType,
          payload,
        });
      } else {
        onLog?.({
          type: 'info',
          text: `HTTP POST sent ${JSON.stringify(message)}`,
          eventType,
          payload,
        });
      }
    } catch (e) {
      console.warn('HTTP POST error:', e);
      onLog?.({
        type: 'error',
        text: `HTTP POST error: ${e.message || e}`,
        eventType,
        payload,
      });
    }
  }
}
