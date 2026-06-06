import { useEffect, useMemo, useState } from 'react';
import { createBridge } from './lib/bridge';
import {
  DISCONNECTED,
  NEUTRAL_INPUT,
  type ConnectionMessage,
  type InputMessage,
  type ProfilesMessage,
} from './lib/types';
import { Topbar } from './components/Topbar';
import { StatusHero } from './components/StatusHero';
import { LiveInputPanel } from './components/LiveInputPanel';
import { ProfileSelector } from './components/ProfileSelector';
import { ActionBar } from './components/ActionBar';

const DEFAULT_PROFILES: ProfilesMessage = { type: 'profiles', active: 'default', available: ['default'] };

export default function App() {
  const bridge = useMemo(() => createBridge(), []);
  const [connection, setConnection] = useState<ConnectionMessage>(DISCONNECTED);
  const [input, setInput] = useState<InputMessage>(NEUTRAL_INPUT);
  const [profiles, setProfiles] = useState<ProfilesMessage>(DEFAULT_PROFILES);

  useEffect(() => {
    const unsub = bridge.subscribe((msg) => {
      switch (msg.type) {
        case 'connection': setConnection(msg); break;
        case 'input':      setInput(msg); break;
        case 'profiles':   setProfiles(msg); break;
        case 'log':        /* surface to a console panel later */ break;
      }
    });
    return unsub;
  }, [bridge]);

  return (
    <div className="app-shell relative h-full w-full overflow-hidden">
      {/* Grain noise overlay */}
      <div className="pointer-events-none absolute inset-0 z-0 opacity-[0.35] mix-blend-overlay bg-grain" />

      <div className="relative z-10 flex h-full flex-col">
        <Topbar connected={connection.connected} mode={connection.mode} />

        <main className="flex-1 overflow-y-auto px-10 pb-6 pt-2">
          <div className="mx-auto flex max-w-5xl flex-col gap-6">
            <StatusHero connection={connection} />
            <LiveInputPanel input={input} connected={connection.connected} />
          </div>
        </main>

        <footer className="border-t border-white/[0.05] bg-ink-950/40 px-10 py-5 backdrop-blur">
          <div className="mx-auto flex max-w-5xl items-center justify-between gap-6">
            <ProfileSelector
              profiles={profiles}
              onChange={(name) => bridge.send({ cmd: 'setProfile', name })}
            />
            <ActionBar
              onRestart={() => bridge.send({ cmd: 'restart' })}
              onExit={() => bridge.send({ cmd: 'exit' })}
            />
          </div>
        </footer>
      </div>
    </div>
  );
}
