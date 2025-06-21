import React from "react";

export default function EndpointConfig({
  endpoint,
  setEndpoint,
  emitType,
  setEmitType,
}) {
  const placeholder =
    emitType === "websocket"
      ? "ws://192.168.1.42:3000/ws"
      : "https://staging.forgedsports.link/api/events/create/";
  return (
    <div className="flex flex-col sm:flex-row items-center gap-3">
      <input
        type="text"
        className="flex-1 border rounded px-3 py-2 text-sm"
        placeholder={placeholder}
        value={endpoint}
        onChange={(e) => setEndpoint(e.target.value)}
      />
      <select
        className="border rounded px-2 py-2 text-sm"
        value={emitType}
        onChange={(e) => setEmitType(e.target.value)}
      >
        <option value="websocket">WebSocket</option>
        <option value="http">HTTP POST</option>
      </select>
    </div>
  );
}
