import React, { useEffect, useRef } from 'react';

function LogEntry({ log }) {
  const color = log.type === 'error' ? 'text-red-600' : 'text-blue-700';

  let details = null;
  if (log.payload) {
    if ('x' in log.payload && 'y' in log.payload) {
      details = (
        <span className="ml-1 text-purple-600">
          (X: {log.payload.x}, Y: {log.payload.y})
        </span>
      );
    } else if ('held' in log.payload) {
      details = (
        <span className="ml-1 text-amber-600">held: {String(log.payload.held)}</span>
      );
    } else {
      details = <span className="ml-1 text-gray-600">{JSON.stringify(log.payload)}</span>;
    }
  }

  return (
    <div className={`p-1 rounded ${color}`}>
      {log.text}
      {details}
    </div>
  );
}

export default function LogFeed({ logs }) {
  const containerRef = useRef(null);
  useEffect(() => {
    if (containerRef.current) containerRef.current.scrollTop = 0;
  }, [logs]);

  return (
    <div
      ref={containerRef}
      className="bg-white border rounded p-2 h-48 overflow-y-auto text-sm font-mono space-y-1"
    >
      {logs.map((log, idx) => (
        <LogEntry key={idx} log={log} />
      ))}
    </div>
  );
}

