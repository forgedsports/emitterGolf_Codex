import React, { useRef, useState } from 'react';

function clamp(val, min, max) {
  return Math.max(min, Math.min(max, val));
}

export default function MiniMap({ emitEvent }) {
  const mapRef = useRef(null);
  // Map dimensions in pixels (natural size of the background image)
  const MAP_W = 317;
  const MAP_H = 600;

  // Current hole (1-18)
  const TOTAL_HOLES = 18;
  const [hole, setHole] = useState(1);

  const incHole = () => setHole(h => (h % TOTAL_HOLES) + 1);
  const decHole = () => setHole(h => (h - 2 + TOTAL_HOLES) % TOTAL_HOLES + 1);

  const handleHoleInput = e => {
    const val = clamp(Number(e.target.value), 1, TOTAL_HOLES);
    if (!Number.isNaN(val)) setHole(val);
  };

  const [dragging, setDragging] = useState(false);
  // Start roughly in the centre of the map
  const [xy, setXY] = useState({ x: Math.round(MAP_W / 2), y: Math.round(MAP_H / 2) });
  const [liveXY, setLiveXY] = useState(null);
  const [manual, setManual] = useState({ x: Math.round(MAP_W / 2), y: Math.round(MAP_H / 2) });

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
    // Convert the pointer position to the MAP_W x MAP_H coordinate space
    let x = clamp(((clientX - rect.left) / rect.width) * MAP_W, 0, MAP_W);
    // Invert Y so 0 = bottom, MAP_H = top
    let yRatio = (clientY - rect.top) / rect.height; // 0 at top, 1 at bottom
    let y = clamp((1 - yRatio) * MAP_H, 0, MAP_H);
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
        className="relative bg-gray-200 rounded shadow w-full touch-none select-none cursor-crosshair"
        style={{
          maxWidth: MAP_W,
          aspectRatio: `${MAP_W} / ${MAP_H}`,
          backgroundImage: `url(${import.meta.env.BASE_URL}minimap/hole${hole}map.png)`,
          backgroundSize: 'contain',
          backgroundRepeat: 'no-repeat',
          backgroundPosition: 'center',
        }}
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
            left: `calc(${((liveXY || xy).x / MAP_W) * 100}% - 10px)` ,
            top: `calc(${(1 - ((liveXY || xy).y / MAP_H)) * 100}% - 10px)` ,
            transition: dragging ? 'none' : 'left 0.2s, top 0.2s',
          }}
        />
        {/* Live coords */}
        <div className="absolute left-2 top-2 bg-white/80 rounded px-2 py-1 text-xs whitespace-nowrap">
          X: {(liveXY || xy).x}, Y: {(liveXY || xy).y}
        </div>
      </div>
      <div className="flex gap-2 items-center">
        <input
          type="number"
          min={0}
          max={MAP_W}
          value={manual.x}
          onChange={e => setManual(m => ({ ...m, x: clamp(Number(e.target.value), 0, MAP_W) }))}
          className="w-16 border rounded px-2 py-1 text-sm"
        />
        <span>X</span>
        <input
          type="number"
          min={0}
          max={MAP_H}
          value={manual.y}
          onChange={e => setManual(m => ({ ...m, y: clamp(Number(e.target.value), 0, MAP_H) }))}
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
      {/* Hole selector */}
      <div className="flex items-center gap-2">
        <button
          className="bg-gray-300 rounded px-2 py-1 hover:bg-gray-400"
          onClick={decHole}
          aria-label="Previous hole"
        >
          ▼
        </button>
        <input
          type="number"
          min={1}
          max={TOTAL_HOLES}
          value={hole}
          onChange={handleHoleInput}
          className="w-16 border rounded px-2 py-1 text-sm text-center"
        />
        <button
          className="bg-gray-300 rounded px-2 py-1 hover:bg-gray-400"
          onClick={incHole}
          aria-label="Next hole"
        >
          ▲
        </button>
      </div>
    </div>
  );
} 