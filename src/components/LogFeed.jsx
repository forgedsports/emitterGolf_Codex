import React, { useEffect, useRef } from 'react';

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
        <div
          key={idx}
          className={
            'p-1 rounded ' + (log.type === 'error' ? 'text-red-600' : 'text-blue-700')
          }
        >
          {log.text}
        </div>
      ))}
    </div>
  );
}

