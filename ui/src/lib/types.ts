// Wire types — must mirror DS4Bridge.App.Web.WebMessages on the C# side.

export type ConnectionMode = 'Usb' | 'Bluetooth' | '';

export interface ConnectionMessage {
  type: 'connection';
  connected: boolean;
  mode: ConnectionMode;
  devicePath: string;
  battery: number; // 0-100
  charging: boolean;
}

export interface InputMessage {
  type: 'input';
  lx: number; ly: number;
  rx: number; ry: number;
  l2: number; r2: number;
  buttons: number; // bitfield, see Ds4Buttons in C#
  dpad: number;    // 0=N, 1=NE, ..., 8=neutral
}

export interface ProfilesMessage {
  type: 'profiles';
  active: string;
  available: string[];
}

export interface LogMessage {
  type: 'log';
  level: string;
  message: string;
}

export type IncomingMessage =
  | ConnectionMessage
  | InputMessage
  | ProfilesMessage
  | LogMessage;

// Bit flags — keep in sync with DS4Bridge.Core.Models.Ds4Buttons
export const Ds4Button = {
  Square: 1 << 0,
  Cross: 1 << 1,
  Circle: 1 << 2,
  Triangle: 1 << 3,
  L1: 1 << 4,
  R1: 1 << 5,
  L2: 1 << 6,
  R2: 1 << 7,
  Share: 1 << 8,
  Options: 1 << 9,
  L3: 1 << 10,
  R3: 1 << 11,
  Ps: 1 << 12,
  TouchpadClick: 1 << 13,
} as const;

export type Ds4ButtonName = keyof typeof Ds4Button;

export interface AppState {
  connection: ConnectionMessage;
  input: InputMessage;
  profiles: ProfilesMessage;
}

export const NEUTRAL_INPUT: InputMessage = {
  type: 'input',
  lx: 128, ly: 128,
  rx: 128, ry: 128,
  l2: 0, r2: 0,
  buttons: 0,
  dpad: 8,
};

export const DISCONNECTED: ConnectionMessage = {
  type: 'connection',
  connected: false,
  mode: '',
  devicePath: '',
  battery: 0,
  charging: false,
};
