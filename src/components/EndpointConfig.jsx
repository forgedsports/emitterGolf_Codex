import React from "react";

export default function EndpointConfig({
  endpoint,
  setEndpoint,
  emitType,
  setEmitType,
}) {
  const placeholder =
    emitType === "websocket"
      ? "wss://birdieboosters-production-8e42.up.railway.app"
      : "https://birdieboosters-production-8e42.up.railway.app/api/events";
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
