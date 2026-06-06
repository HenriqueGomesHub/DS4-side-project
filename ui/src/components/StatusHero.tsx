import { motion, AnimatePresence } from 'framer-motion';
import type { ConnectionMessage } from '../lib/types';

interface StatusHeroProps {
  connection: ConnectionMessage;
}

export function StatusHero({ connection }: StatusHeroProps) {
  const isConnected = connection.connected;
  return (
    <section className="hairline relative overflow-hidden rounded-2xl bg-ink-850/70 px-10 py-12 shadow-card backdrop-blur-sm">
      {/* Decorative line work */}
      <div className="pointer-events-none absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-white/10 to-transparent" />
      <div className="pointer-events-none absolute -right-32 -top-32 h-72 w-72 rounded-full bg-signal-cyan/[0.07] blur-3xl" />

      <div className="relative flex items-start gap-8">
        {/* Pulse orb */}
        <div className="relative flex h-24 w-24 shrink-0 items-center justify-center">
          <motion.div
            className="absolute inset-0 rounded-full"
            animate={{
              backgroundColor: isConnected ? 'rgba(6,182,212,0.18)' : 'rgba(71,85,105,0.14)',
            }}
            transition={{ duration: 0.5 }}
          />
          {isConnected && (
            <motion.div
              className="absolute inset-0 rounded-full"
              animate={{ scale: [1, 1.45, 1], opacity: [0.5, 0, 0.5] }}
              transition={{ duration: 2.4, repeat: Infinity, ease: 'easeInOut' }}
              style={{ background: 'radial-gradient(circle, rgba(6,182,212,0.4) 0%, transparent 70%)' }}
            />
          )}
          <motion.div
            className="relative h-6 w-6 rounded-full"
            animate={{
              backgroundColor: isConnected ? '#06b6d4' : '#475569',
              boxShadow: isConnected
                ? '0 0 28px 6px rgba(6,182,212,0.55), inset 0 0 8px rgba(255,255,255,0.4)'
                : 'none',
            }}
            transition={{ duration: 0.4 }}
          />
        </div>

        {/* Text block */}
        <div className="min-w-0 flex-1">
          <div className="font-mono text-[10px] uppercase tracking-wider2 text-fog-500">
            Controller status
          </div>
          <AnimatePresence mode="wait">
            <motion.div
              key={isConnected ? 'on' : 'off'}
              initial={{ opacity: 0, y: 8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -8 }}
              transition={{ duration: 0.35, ease: [0.2, 0.7, 0.2, 1] }}
              className="mt-1 text-[42px] font-semibold leading-none tracking-tight"
            >
              {isConnected ? 'CONNECTED' : 'STANDBY'}
            </motion.div>
          </AnimatePresence>

          <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-1 font-mono text-[12px] text-fog-400">
            <span>
              <span className="text-fog-500">via</span>{' '}
              <span className="text-fog-200">{isConnected ? (connection.mode || 'usb') : '—'}</span>
            </span>
            {isConnected && (
              <>
                <span className="text-fog-500/60">·</span>
                <span className="truncate max-w-[26rem]" title={connection.devicePath}>
                  {trimPath(connection.devicePath)}
                </span>
              </>
            )}
          </div>

          {/* Battery */}
          <div className="mt-6 flex items-center gap-4">
            <span className="font-mono text-[10px] uppercase tracking-wider2 text-fog-500">
              Battery
            </span>
            <div className="relative h-1.5 w-56 overflow-hidden rounded-full bg-white/[0.06]">
              <motion.div
                className="absolute inset-y-0 left-0 rounded-full"
                style={{
                  background:
                    connection.charging
                      ? 'linear-gradient(90deg, #06b6d4, #67e8f9)'
                      : connection.battery < 25
                        ? '#f59e0b'
                        : '#06b6d4',
                }}
                initial={false}
                animate={{ width: `${isConnected ? Math.max(connection.battery, 4) : 0}%` }}
                transition={{ type: 'spring', stiffness: 220, damping: 26 }}
              />
            </div>
            <span className="font-mono text-xs tabular-nums text-fog-200">
              {isConnected ? `${connection.battery}%` : '—'}
            </span>
            {connection.charging && (
              <span className="font-mono text-[10px] uppercase tracking-wider2 text-signal-cyan">
                charging
              </span>
            )}
          </div>
        </div>
      </div>
    </section>
  );
}

function trimPath(path: string): string {
  if (!path) return '';
  // Long HID paths get unreadable; show head + tail
  if (path.length <= 60) return path;
  return path.slice(0, 28) + ' … ' + path.slice(-26);
}
