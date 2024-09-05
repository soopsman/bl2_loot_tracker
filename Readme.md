## BL2/BLTPS Loot Tracker

This app looks for updates to tracker files from the [Loot Randomizer](https://github.com/mopioid/Borderlands-Loot-Randomizer) mod by Mopioid. 

When the app is started a Github Gist is created/updated for the latest seed and kept up to date when the tracker file changes.

If a different seed is created or updated, the same Gist will be updated with the new seed.

### Prerequisites

1. GitHub account
2. GitHub Personal access token with Gist permission: Settings -> Developer Settings -> Personal access tokens -> Tokens (classic)

### Installation

1. Unzip the latest release
2. If this is a fresh install, rename appsettings.json.initial to appsettings.json 
3. Copy the GitHub token from prerequisites into appsettings.json in the indicated spot

### Usage

1. Run the executable
2. The app runs in the system tray, right-click the icon to exit or open the Gist

### Additional Configuration

These settings can be added/changed in appsettings.json

<b>SeedsPath</b> - path to BL2 seeds folder, change this if your BL2 is not installed in the default steam location<br>
<b>AdditionalPaths</b> - paths to other folders with seeds, as an array (the default steam BLTPS location is included by default)<br>
<b>UseSingleSeed</b> - if false, a new Gist is created for every seed