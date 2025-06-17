import React, { useRef, useState } from 'react';

function clamp(val, min, max) {
  return Math.max(min, Math.min(max, val));
}

export default function MiniMap({ emitEvent }) {
  const mapRef = useRef(null);
  const [dragging, setDragging] = useState(false);
  const [xy, setXY] = useState({ x: 50, y: 50 });
  const [liveXY, setLiveXY] = useState(null);
  const [manual, setManual] = useState({ x: 50, y: 50 });

  const getXY = e => {
    const rect = mapRef.current.getBoundingClientRect();
    let clientX, clientY;
    if (e.touches) {
      clientX = e.touches[0].clientX;
      clientY = e.touches[0].clientY;
    } else {
      clientX = e.clientX;
      clientY = e.clientY;
    }
    let x = clamp(((clientX - rect.left) / rect.width) * 100, 0, 100);
    let y = clamp(((clientY - rect.top) / rect.height) * 100, 0, 100);
    return { x: Math.round(x), y: Math.round(y) };
  };

  const handleStart = e => {
    setDragging(true);
    const pos = getXY(e);
    setLiveXY(pos);
    e.preventDefault();
  };
  const handleMove = e => {
    if (!dragging) return;
    const pos = getXY(e);
    setLiveXY(pos);
    e.preventDefault();
  };
  const handleEnd = e => {
    if (!dragging) return;
    const pos = getXY(e.changedTouches ? e.changedTouches[0] : e);
    setXY(pos);
    setLiveXY(null);
    setDragging(false);
    emitEvent('xyUpdate', pos);
    e.preventDefault();
  };

  // Manual emit
  const handleManualEmit = () => {
    emitEvent('xyUpdate', manual);
  };

  return (
    <div className="flex flex-col gap-3 items-center">
      <div
        ref={mapRef}
        className="relative bg-gray-200 rounded shadow w-full max-w-xs aspect-square touch-none select-none cursor-crosshair"
        style={{ maxWidth: 300 }}
        onMouseDown={handleStart}
        onMouseMove={handleMove}
        onMouseUp={handleEnd}
        onMouseLeave={handleEnd}
        onTouchStart={handleStart}
        onTouchMove={handleMove}
        onTouchEnd={handleEnd}
        onTouchCancel={handleEnd}
      >
        {/* Marker */}
        <div
          className="absolute w-5 h-5 bg-blue-500 rounded-full border-2 border-white pointer-events-none"
          style={{
            left: `calc(${(liveXY || xy).x}% - 10px)` ,
            top: `calc(${(liveXY || xy).y}% - 10px)` ,
            transition: dragging ? 'none' : 'left 0.2s, top 0.2s',
          }}
        />
        {/* Live coords */}
        <div className="absolute left-2 top-2 bg-white/80 rounded px-2 py-1 text-xs">
          X: {(liveXY || xy).x}, Y: {(liveXY || xy).y}
        </div>
      </div>
      <div className="flex gap-2 items-center">
        <input
          type="number"
          min={0}
          max={100}
          value={manual.x}
          onChange={e => setManual(m => ({ ...m, x: clamp(Number(e.target.value), 0, 100) }))}
          className="w-16 border rounded px-2 py-1 text-sm"
        />
        <span>X</span>
        <input
          type="number"
          min={0}
          max={100}
          value={manual.y}
          onChange={e => setManual(m => ({ ...m, y: clamp(Number(e.target.value), 0, 100) }))}
          className="w-16 border rounded px-2 py-1 text-sm"
        />
        <span>Y</span>
        <button
          className="bg-blue-600 text-white rounded px-3 py-1 text-sm font-semibold hover:bg-blue-700"
          onClick={handleManualEmit}
        >
          Emit
        </button>
      </div>
    </div>
  );
} 