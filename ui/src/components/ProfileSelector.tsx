import { useEffect, useRef, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import type { ProfilesMessage } from '../lib/types';

interface ProfileSelectorProps {
  profiles: ProfilesMessage;
  onChange: (name: string) => void;
}

export function ProfileSelector({ profiles, onChange }: ProfileSelectorProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    if (open) {
      window.addEventListener('mousedown', handleClick);
      return () => window.removeEventListener('mousedown', handleClick);
    }
  }, [open]);

  return (
    <div className="flex items-center gap-4" ref={ref}>
      <span className="font-mono text-[10px] uppercase tracking-wider2 text-fog-500">
        Profile
      </span>
      <div className="relative">
        <button
          type="button"
          onClick={() => setOpen((v) => !v)}
          className="group flex items-center gap-3 rounded-md border border-white/[0.06] bg-ink-800/70 px-4 py-2 font-mono text-[12px] text-fog-200 transition hover:border-white/[0.12] hover:bg-ink-700/70"
        >
          <span className="capitalize">{profiles.active}</span>
          <motion.span
            className="text-fog-500"
            animate={{ rotate: open ? 180 : 0 }}
            transition={{ duration: 0.18 }}
          >
            ▾
          </motion.span>
        </button>
        <AnimatePresence>
          {open && (
            <motion.div
              className="absolute bottom-full left-0 z-20 mb-2 min-w-[14rem] overflow-hidden rounded-md border border-white/[0.08] bg-ink-850/95 shadow-card backdrop-blur"
              initial={{ opacity: 0, y: 6 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: 6 }}
              transition={{ duration: 0.18 }}
            >
              {profiles.available.map((name) => (
                <button
                  key={name}
                  type="button"
                  onClick={() => {
                    onChange(name);
                    setOpen(false);
                  }}
                  className="flex w-full items-center justify-between px-4 py-2.5 text-left font-mono text-[12px] capitalize text-fog-200 transition hover:bg-white/[0.04]"
                >
                  <span>{name}</span>
                  {name === profiles.active && (
                    <span className="text-signal-cyan">●</span>
                  )}
                </button>
              ))}
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}
