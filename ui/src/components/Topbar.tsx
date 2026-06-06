import { motion } from 'framer-motion';
import type { ConnectionMode } from '../lib/types';

interface TopbarProps {
  connected: boolean;
  mode: ConnectionMode;
}

export function Topbar({ connected, mode }: TopbarProps) {
  return (
    <header className="flex items-center justify-between px-10 py-6">
      <div className="flex items-baseline gap-3">
        <span className="font-mono text-[11px] tracking-wider2 text-fog-500">DS4</span>
        <h1 className="text-2xl font-semibold tracking-tight">
          Bridge
        </h1>
        <span className="font-mono text-[10px] tracking-wider2 text-fog-500/70">v1.0</span>
      </div>

      <div className="flex items-center gap-3 font-mono text-[10px] uppercase tracking-wider2 text-fog-500">
        <motion.span
          className="inline-flex h-1.5 w-1.5 rounded-full"
          animate={{
            backgroundColor: connected ? '#06b6d4' : '#475569',
            boxShadow: connected ? '0 0 12px 2px rgba(6,182,212,0.6)' : '0 0 0 0 transparent',
          }}
          transition={{ duration: 0.4 }}
        />
        <span>{connected ? (mode || 'connected') : 'standby'}</span>
      </div>
    </header>
  );
}
