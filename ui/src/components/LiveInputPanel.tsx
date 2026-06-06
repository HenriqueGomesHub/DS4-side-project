import { motion } from 'framer-motion';
import type { InputMessage } from '../lib/types';
import { Ds4Button } from '../lib/types';
import { StickPad } from './StickPad';
import { TriggerBar } from './TriggerBar';
import { FaceCluster } from './FaceCluster';
import { DpadCluster } from './DpadCluster';

interface LiveInputPanelProps {
  input: InputMessage;
  connected: boolean;
}

export function LiveInputPanel({ input, connected }: LiveInputPanelProps) {
  return (
    <section className="hairline rounded-2xl bg-ink-850/70 px-8 py-7 shadow-card backdrop-blur-sm">
      <div className="mb-5 flex items-baseline justify-between">
        <div>
          <div className="font-mono text-[10px] uppercase tracking-wider2 text-fog-500">
            Live input
          </div>
          <h2 className="mt-0.5 text-lg font-medium tracking-tight">
            Real-time controller mirror
          </h2>
        </div>
        <motion.div
          className="flex items-center gap-2 font-mono text-[10px] uppercase tracking-wider2 text-fog-500"
          animate={{ opacity: connected ? 1 : 0.4 }}
        >
          <span className="inline-block h-1 w-1 rounded-full bg-signal-cyan" />
          {connected ? '30 fps stream' : 'idle'}
        </motion.div>
      </div>

      <div className="grid grid-cols-[1fr_auto_1fr] items-center gap-8">
        {/* Left side: dpad + L stick + L1/L2 */}
        <div className="flex flex-col items-center gap-5">
          <div className="flex items-center gap-6">
            <DpadCluster dpad={input.dpad} />
            <StickPad
              label="L STICK"
              x={input.lx}
              y={input.ly}
              clicked={(input.buttons & Ds4Button.L3) !== 0}
            />
          </div>
          <div className="flex items-center gap-3 font-mono text-[10px] uppercase tracking-wider2 text-fog-500">
            <ShoulderPill label="L1" pressed={(input.buttons & Ds4Button.L1) !== 0} />
            <TriggerBar label="L2" value={input.l2} />
          </div>
        </div>

        {/* Center: meta buttons */}
        <div className="flex flex-col items-center gap-3">
          <MetaPill label="Share"   pressed={(input.buttons & Ds4Button.Share) !== 0} />
          <MetaPill label="PS"      pressed={(input.buttons & Ds4Button.Ps) !== 0} accent />
          <MetaPill label="Options" pressed={(input.buttons & Ds4Button.Options) !== 0} />
          <MetaPill label="Touchpad" pressed={(input.buttons & Ds4Button.TouchpadClick) !== 0} />
        </div>

        {/* Right side: face cluster + R stick + R1/R2 */}
        <div className="flex flex-col items-center gap-5">
          <div className="flex items-center gap-6">
            <StickPad
              label="R STICK"
              x={input.rx}
              y={input.ry}
              clicked={(input.buttons & Ds4Button.R3) !== 0}
            />
            <FaceCluster buttons={input.buttons} />
          </div>
          <div className="flex items-center gap-3 font-mono text-[10px] uppercase tracking-wider2 text-fog-500">
            <TriggerBar label="R2" value={input.r2} />
            <ShoulderPill label="R1" pressed={(input.buttons & Ds4Button.R1) !== 0} />
          </div>
        </div>
      </div>
    </section>
  );
}

interface PillProps {
  label: string;
  pressed: boolean;
  accent?: boolean;
}

function ShoulderPill({ label, pressed }: PillProps) {
  return (
    <motion.div
      className="rounded-md px-2.5 py-1 text-fog-200"
      animate={{
        backgroundColor: pressed ? 'rgba(6,182,212,0.18)' : 'rgba(255,255,255,0.04)',
        color: pressed ? '#06b6d4' : '#c9ced6',
        boxShadow: pressed ? '0 0 14px -2px rgba(6,182,212,0.5)' : 'none',
      }}
      transition={{ duration: 0.15 }}
    >
      {label}
    </motion.div>
  );
}

function MetaPill({ label, pressed, accent }: PillProps) {
  const active = pressed;
  return (
    <motion.div
      className="min-w-[6.5rem] rounded-full px-4 py-1.5 text-center font-mono text-[10px] uppercase tracking-wider2"
      animate={{
        backgroundColor: active
          ? accent
            ? 'rgba(236,72,153,0.18)'
            : 'rgba(6,182,212,0.18)'
          : 'rgba(255,255,255,0.04)',
        color: active ? (accent ? '#ec4899' : '#06b6d4') : '#8b94a3',
        boxShadow: active
          ? accent
            ? '0 0 16px -2px rgba(236,72,153,0.5)'
            : '0 0 14px -2px rgba(6,182,212,0.5)'
          : 'none',
      }}
      transition={{ duration: 0.15 }}
    >
      {label}
    </motion.div>
  );
}
