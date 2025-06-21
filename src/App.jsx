import React, { useState, useRef } from "react";
import EndpointConfig from "./components/EndpointConfig";
import ButtonEvent from "./components/ButtonEvent";
import MiniMap from "./components/MiniMap";
import LogFeed from "./components/LogFeed";
import emitEvent from "./emitEvent";

const DEFAULT_WS = "ws://192.168.1.42:3000/ws";
const DEFAULT_HTTP = "https://staging.forgedsports.link/api/events/create/";

export default function App() {
  const [emitType, setEmitType] = useState("websocket");
  const [wsEndpoint, setWsEndpoint] = useState(DEFAULT_WS);
  const [httpEndpoint, setHttpEndpoint] = useState(DEFAULT_HTTP);
  const wsRef = useRef(null);
  const [logs, setLogs] = useState([]);

  const addLog = (log) => setLogs((l) => [log, ...l]);

  const currentEndpoint = emitType === "websocket" ? wsEndpoint : httpEndpoint;

  const setCurrentEndpoint = (value) => {
    if (emitType === "websocket") {
      setWsEndpoint(value);
    } else {
      setHttpEndpoint(value);
    }
  };

  // Pass emitEvent with endpoint/type/wsRef
  const handleEmitEvent = (type, payload) => {
    emitEvent({
      eventType: type,
      payload,
      endpoint: currentEndpoint,
      emitType,
      wsRef,
      onLog: addLog,
    });
  };

  return (
    <div className="max-w-2xl mx-auto p-4 flex flex-col gap-6">
      <h1 className="text-2xl font-bold text-center mb-2">Emitter UI</h1>
      <EndpointConfig
        endpoint={currentEndpoint}
        setEndpoint={setCurrentEndpoint}
        emitType={emitType}
        setEmitType={setEmitType}
      />
      <ButtonEvent emitEvent={handleEmitEvent} />
      <MiniMap emitEvent={handleEmitEvent} />
      <LogFeed logs={logs} />
    </div>
  );
}
