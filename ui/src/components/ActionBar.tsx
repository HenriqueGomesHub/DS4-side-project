import { motion } from 'framer-motion';

interface ActionBarProps {
  onRestart: () => void;
  onExit: () => void;
}

export function ActionBar({ onRestart, onExit }: ActionBarProps) {
  return (
    <div className="flex items-center gap-3">
      <motion.button
        type="button"
        onClick={onRestart}
        whileHover={{ y: -1 }}
        whileTap={{ scale: 0.98 }}
        className="rounded-md border border-white/[0.06] bg-white/[0.03] px-4 py-2 font-mono text-[11px] uppercase tracking-wider2 text-fog-200 transition hover:border-white/[0.14] hover:bg-white/[0.06]"
      >
        Restart bridge
      </motion.button>
      <motion.button
        type="button"
        onClick={onExit}
        whileHover={{ y: -1 }}
        whileTap={{ scale: 0.98 }}
        className="group relative rounded-md border border-signal-magenta/30 bg-signal-magenta/[0.08] px-4 py-2 font-mono text-[11px] uppercase tracking-wider2 text-signal-magenta transition hover:border-signal-magenta/60 hover:bg-signal-magenta/[0.14]"
      >
        <span className="relative z-10">✕  Exit</span>
        <span className="absolute inset-0 rounded-md opacity-0 transition group-hover:opacity-100"
              style={{ boxShadow: '0 0 24px -4px rgba(236,72,153,0.5)' }} />
      </motion.button>
    </div>
  );
}
