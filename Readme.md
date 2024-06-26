﻿## BL2 Loot Tracker

This app looks for updates to tracker files from the [Loot Randomizer](https://github.com/mopioid/Borderlands-Loot-Randomizer) mod by Mopioid. 

When the app is started a Github Gist is created/updated for the latest seed and kept up to date when the tracker file changes.

If a different seed is created or updated, the same Gist will be updated with the new seed.

### Prerequisites

1. Github account
2. Github Personal access token with Gist permission: Settings -> Developer Settings -> Personal access tokens -> Tokens (classic)

### Installation

1. Unzip the latest release
2. Create a Personal Access Token in GitHub with the 'Gists' permission
3. Copy the token into appsettings.json in the indicated spot
4. Update your seeds path in appsettings.json if required

### Usage

1. Run the executable
2. The app runs in the system tray, right-click the icon to exit or open the Gist
