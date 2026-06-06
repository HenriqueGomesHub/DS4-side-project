import { motion } from 'framer-motion';
import { Ds4Button } from '../lib/types';

interface FaceClusterProps {
  buttons: number;
}

// The four face buttons in their PS-standard layout:
//        △
//      □   ○
//        ✕
const POSITIONS = [
  { name: 'Triangle', glyph: '△', flag: Ds4Button.Triangle, x: 0,   y: -22, color: '#22c55e' },
  { name: 'Square',   glyph: '□', flag: Ds4Button.Square,   x: -22, y: 0,   color: '#ec4899' },
  { name: 'Circle',   glyph: '○', flag: Ds4Button.Circle,   x: 22,  y: 0,   color: '#ef4444' },
  { name: 'Cross',    glyph: '✕', flag: Ds4Button.Cross,    x: 0,   y: 22,  color: '#06b6d4' },
] as const;

export function FaceCluster({ buttons }: FaceClusterProps) {
  return (
    <div className="relative h-28 w-28">
      {POSITIONS.map((b) => {
        const pressed = (buttons & b.flag) !== 0;
        return (
          <motion.button
            key={b.name}
            type="button"
            tabIndex={-1}
            className="absolute left-1/2 top-1/2 flex h-9 w-9 items-center justify-center rounded-full text-base font-light text-fog-200"
            style={{ x: b.x - 18, y: b.y - 18 }}
            animate={{
              backgroundColor: pressed ? hexWithAlpha(b.color, 0.22) : 'rgba(255,255,255,0.04)',
              color: pressed ? b.color : '#8b94a3',
              boxShadow: pressed
                ? `0 0 18px -2px ${hexWithAlpha(b.color, 0.6)}, inset 0 0 0 1px ${hexWithAlpha(b.color, 0.5)}`
                : 'inset 0 0 0 1px rgba(255,255,255,0.06)',
            }}
            transition={{ duration: 0.12 }}
          >
            {b.glyph}
          </motion.button>
        );
      })}
    </div>
  );
}

function hexWithAlpha(hex: string, alpha: number): string {
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  return `rgba(${r},${g},${b},${alpha})`;
}
