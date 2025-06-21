import React, { useState, useRef } from 'react';
import EndpointConfig from './components/EndpointConfig';
import ButtonEvent from './components/ButtonEvent';
import MiniMap from './components/MiniMap';
import LogFeed from './components/LogFeed';
import emitEvent from './emitEvent';

export default function App() {
  const [endpoint, setEndpoint] = useState('');
  const [emitType, setEmitType] = useState('websocket'); // or 'http'
  const wsRef = useRef(null);
  const [logs, setLogs] = useState([]);

  const addLog = log => setLogs(l => [log, ...l]);

  // Pass emitEvent with endpoint/type/wsRef
  const handleEmitEvent = (type, payload) => {
    emitEvent({
      eventType: type,
      payload,
      endpoint,
      emitType,
      wsRef,
      onLog: addLog,
    });
  };

  return (
    <div className="max-w-2xl mx-auto p-4 flex flex-col gap-6">
      <h1 className="text-2xl font-bold text-center mb-2">Emitter UI</h1>
      <EndpointConfig
        endpoint={endpoint}
        setEndpoint={setEndpoint}
        emitType={emitType}
        setEmitType={setEmitType}
      />
      <ButtonEvent emitEvent={handleEmitEvent} />
      <MiniMap emitEvent={handleEmitEvent} />
      <LogFeed logs={logs} />
    </div>
  );
}
