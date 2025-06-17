import React, { useRef, useState } from 'react';

const HOLD_DELAY = 600; // ms for 'held' event

export default function ButtonEvent({ emitEvent }) {
  const [state, setState] = useState('idle');
  const holdTimeout = useRef(null);
  const heldRef = useRef(false);

  const handlePress = () => {
    setState('pressed');
    emitEvent('buttonPress', {});
    heldRef.current = false;
    holdTimeout.current = setTimeout(() => {
      setState('held');
      emitEvent('buttonHold', {});
      heldRef.current = true;
    }, HOLD_DELAY);
  };

  const handleRelease = () => {
    clearTimeout(holdTimeout.current);
    if (state === 'pressed' && !heldRef.current) {
      setState('released');
      emitEvent('buttonRelease', { held: false });
    } else if (state === 'held') {
      setState('released');
      emitEvent('buttonRelease', { held: true });
    } else {
      setState('idle');
    }
    setTimeout(() => setState('idle'), 300);
  };

  return (
    <div className="flex flex-col items-center gap-2">
      <button
        className="w-40 h-40 rounded-full bg-blue-600 text-white text-xl font-bold shadow-lg active:bg-blue-800 select-none"
        onMouseDown={handlePress}
        onMouseUp={handleRelease}
        onMouseLeave={handleRelease}
        onTouchStart={e => { e.preventDefault(); handlePress(); }}
        onTouchEnd={e => { e.preventDefault(); handleRelease(); }}
        onTouchCancel={e => { e.preventDefault(); handleRelease(); }}
      >
        Press Me
      </button>
      <div className="text-gray-600 text-sm h-5">{state !== 'idle' ? state : ''}</div>
    </div>
  );
} 