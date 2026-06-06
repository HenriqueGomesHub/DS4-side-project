# DS4Bridge

Translates Sony DualShock 4 input into a virtual Xbox 360 controller on Windows 10/11 x64 using ViGEmBus.

## Prerequisites
- **.NET 8 SDK** (build) — https://dotnet.microsoft.com/download/dotnet/8.0
- **ViGEmBus driver** (runtime) — https://github.com/nefarius/ViGEmBus/releases
- A DualShock 4 controller (CUH-ZCT1 / CUH-ZCT2 / official wireless adapter)

## Build
```
dotnet restore
dotnet build -c Release
dotnet publish src/DS4Bridge.App -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

Output: `src/DS4Bridge.App/bin/Release/net8.0-windows/win-x64/publish/DS4Bridge.exe`

## Test
```
dotnet test
```

## Run
Launch `DS4Bridge.exe`. The tray icon turns green when a DS4 is detected over USB or Bluetooth.

### Verbose logs
Run with `--verbose` to enable per-frame debug logging.

### Configuration
`%APPDATA%\DS4Bridge\config.json` — edit profiles, deadzones, button maps, lightbar color.

### Logs
`%APPDATA%\DS4Bridge\logs\` — daily-rolling, 7-day retention.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| Tray stays gray | ViGEmBus missing | Install per `installer/README.md` |
| Double inputs in game | Windows sees both DS4 and virtual Xbox | Disable DS4 in game's controller settings or use HidHide |
| BT controller has no rumble | v1 only supports rumble over USB | Use USB cable |
| Steam captures inputs | Steam Input enabled | Steam > Settings > Controller > disable Steam Input for this device |
| Phantom virtual controller after crash | Process killed without cleanup | Windows clears it on next start; or unplug/replug DS4 |

## Architecture
```
DS4Bridge.App (WPF tray UI, hosting)
    │
    ├─> DS4Bridge.Hid (HidSharp wrapper, watcher)
    │       └─> DS4Bridge.Core (parser, mapping, bridge orchestrator)
    └─> DS4Bridge.Virtual (Nefarius.ViGEm.Client wrapper)
                └─> DS4Bridge.Core
```

## Planned (post-v1, intentionally NOT in v1)
- DualSense (PS5) support — different protocol
- Motion controls / gyro-as-mouse
- Touchpad-as-mouse
- Macros / combo bindings
- Multi-controller (only first DS4 is handled in v1)
- Per-game profile auto-switching
- Lightbar color tied to battery level
- UDP gyro server (cemuhook protocol)
- HidHide integration for transparent device hiding
- Bluetooth output reports (rumble + lightbar over BT) with CRC32

## License
TBD by repo owner.
"# DS4-side-project" 
