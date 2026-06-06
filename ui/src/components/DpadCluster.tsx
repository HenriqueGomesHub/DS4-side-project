import { motion } from 'framer-motion';

interface DpadClusterProps {
  dpad: number; // 0=N..7=NW, 8=neutral
}

// Mapping of dpad enum value -> which of the four cardinal arrows light up
const ACTIVE: Record<number, ('N' | 'E' | 'S' | 'W')[]> = {
  0: ['N'],
  1: ['N', 'E'],
  2: ['E'],
  3: ['S', 'E'],
  4: ['S'],
  5: ['S', 'W'],
  6: ['W'],
  7: ['N', 'W'],
  8: [],
};

const ARROWS: { dir: 'N' | 'E' | 'S' | 'W'; glyph: string; x: number; y: number }[] = [
  { dir: 'N', glyph: '▲', x: 0,   y: -22 },
  { dir: 'E', glyph: '▶', x: 22,  y: 0 },
  { dir: 'S', glyph: '▼', x: 0,   y: 22 },
  { dir: 'W', glyph: '◀', x: -22, y: 0 },
];

export function DpadCluster({ dpad }: DpadClusterProps) {
  const active = new Set(ACTIVE[dpad] ?? []);
  return (
    <div className="relative h-28 w-28">
      {ARROWS.map(({ dir, glyph, x, y }) => {
        const isActive = active.has(dir);
        return (
          <motion.div
            key={dir}
            className="absolute left-1/2 top-1/2 flex h-9 w-9 items-center justify-center text-[10px] text-fog-500"
            style={{ x: x - 18, y: y - 18 }}
            animate={{
              color: isActive ? '#06b6d4' : '#475569',
              textShadow: isActive ? '0 0 10px rgba(6,182,212,0.7)' : 'none',
              scale: isActive ? 1.15 : 1,
            }}
            transition={{ duration: 0.12 }}
          >
            {glyph}
          </motion.div>
        );
      })}
    </div>
  );
}
