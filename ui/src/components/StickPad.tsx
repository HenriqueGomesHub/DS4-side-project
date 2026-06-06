import { motion } from 'framer-motion';

interface StickPadProps {
  label: string;
  x: number; // 0-255, center 128
  y: number;
  clicked: boolean;
}

// Translates raw stick byte (0..255, center 128) to a normalized offset in [-1, 1]
function normalize(raw: number): number {
  const offset = raw - 128;
  return offset >= 0 ? Math.min(offset / 127, 1) : Math.max(offset / 128, -1);
}

export function StickPad({ label, x, y, clicked }: StickPadProps) {
  const nx = normalize(x);
  const ny = normalize(y); // DS4 raw y: up = 0 (negative offset), down = 255 (positive)
  const radius = 38; // px from center the dot can travel

  return (
    <div className="flex flex-col items-center gap-2">
      <motion.div
        className="relative flex h-28 w-28 items-center justify-center rounded-full"
        style={{
          background:
            'radial-gradient(circle at 30% 30%, rgba(255,255,255,0.04), rgba(255,255,255,0) 70%), #0e131c',
          boxShadow:
            'inset 0 0 0 1px rgba(255,255,255,0.06), inset 0 4px 22px rgba(0,0,0,0.6)',
        }}
        animate={{
          boxShadow: clicked
            ? 'inset 0 0 0 1px rgba(6,182,212,0.6), inset 0 0 24px rgba(6,182,212,0.25), 0 0 24px -4px rgba(6,182,212,0.5)'
            : 'inset 0 0 0 1px rgba(255,255,255,0.06), inset 0 4px 22px rgba(0,0,0,0.6)',
        }}
      >
        {/* Cross hairs */}
        <div className="absolute h-full w-px bg-white/[0.04]" />
        <div className="absolute h-px w-full bg-white/[0.04]" />
        {/* Concentric ring */}
        <div className="absolute h-16 w-16 rounded-full ring-1 ring-white/[0.05]" />

        <motion.div
          className="absolute h-4 w-4 rounded-full"
          animate={{
            x: nx * radius,
            y: ny * radius,
            backgroundColor: clicked ? '#06b6d4' : '#e8e9ec',
            boxShadow: clicked
              ? '0 0 16px 2px rgba(6,182,212,0.7), inset 0 0 4px rgba(255,255,255,0.6)'
              : '0 0 10px 1px rgba(255,255,255,0.18), inset 0 0 4px rgba(0,0,0,0.4)',
          }}
          transition={{ type: 'spring', stiffness: 420, damping: 28, mass: 0.35 }}
        />
      </motion.div>
      <span className="font-mono text-[9px] uppercase tracking-wider2 text-fog-500">
        {label}
      </span>
    </div>
  );
}
