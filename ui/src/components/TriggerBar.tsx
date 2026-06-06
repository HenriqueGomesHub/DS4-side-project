import { motion } from 'framer-motion';

interface TriggerBarProps {
  label: string;
  value: number; // 0-255
}

export function TriggerBar({ label, value }: TriggerBarProps) {
  const pct = Math.min(100, Math.max(0, (value / 255) * 100));
  return (
    <div className="flex items-center gap-2">
      <span className="text-fog-500">{label}</span>
      <div className="relative h-1.5 w-20 overflow-hidden rounded-full bg-white/[0.05]">
        <motion.div
          className="absolute inset-y-0 left-0 rounded-full bg-signal-cyan"
          initial={false}
          animate={{ width: `${pct}%` }}
          transition={{ type: 'spring', stiffness: 380, damping: 28 }}
          style={{
            boxShadow: value > 0 ? '0 0 10px -2px rgba(6,182,212,0.7)' : 'none',
          }}
        />
      </div>
    </div>
  );
}
