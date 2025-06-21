import React from 'react';

export default function LogFeed({ logs }) {
  return (
    <div className="bg-white border rounded p-2 h-48 overflow-y-auto text-sm font-mono space-y-1">
      {logs.map((log, idx) => (
        <div key={idx} className={log.type === 'error' ? 'text-red-600' : 'text-gray-800'}>
          {log.text}
        </div>
      ))}
    </div>
  );
}

