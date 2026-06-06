# DS4Bridge Runtime Prerequisites

DS4Bridge depends on the ViGEmBus kernel driver to create virtual Xbox 360 controllers.

## Install ViGEmBus
1. Download the latest `ViGEmBus_Setup_x64.msi` from https://github.com/nefarius/ViGEmBus/releases
2. Run the MSI (admin consent required)
3. Reboot if prompted
4. Verify by running `pwsh ./check-vigem.ps1`

DS4Bridge will not bundle the driver — it's a kernel-mode component that requires interactive admin consent.
