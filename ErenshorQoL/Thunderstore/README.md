# ErenshorQoL
Erenshor Quality of Life Modpack based on BepInEx

## Version: 1.1.1

## Features (Configurable): /autoloot, /bank, /auction, /help, AutoSendPet, EnableAutoAttackonSkill

## How it works:

- `/autoloot` - Toggles the feature to automatically Loot All items from the nearest corpse each time a creature dies.
- `/bank` - Opens the bank window
- `/auction` - Opens the auction hall window
- `/help` - Expanded list of commands including additional available player and GM commands
- `Auto Send Pet` - If enabled, the pet will automatically be sent when you enable autoattack (*some parts of this feature still in progress*)
- `Auto Enable Autoattack` - If enabled, AutoAttack will be turned on when you use a skill (*some parts of this feature still in progress*)

## How to Install: 

### (Recommended for Beta Testers/full Erenshor License):
Click the Install with Mod Manager button

### Manual (Recommended for Demo Users):

1. Install BepInEx - https://thunderstore.io/c/erenshor/p/BepInEx/BepInExPack/.
2. Download manually, extract the Brumdail-ErenshorQoL folder and move into the Erenshor\BepInEx\plugins folder.

## How to Configure:
A Brumdail.ErenshorQoLMod.cfg file will be automatically created in BepInEx\config the first time you launch the mod.
Toggle full features on or off as well as parts of features by editing the file.
If there are issues, you can revert to default values.

## Technical Details
Adds Postfix commands to find the nearest new corpse after the Character.DoDeath() call.
Adds Prefix commands to TypeText.CheckCommands() to include new commands
Adds Prefix commands to UseSkill.DoSkill() to automatically perform actions when skills are used.
Adds Postfix commands to PlayerCombat.ToggleAttack() to automatically perform actions when autoattack is enabled.

## Updated Commands:

### GM commands: *most not available in the demo build*
- `/iamadev` - Enable Dev Controls
- `/allitem` - List all items
- `/additem 11823624` - Add item to inventory (use /allitem to get item codes)
- `/hpscale 1.0 (multiplier)` - NPC HP scale modifier. You must zone to activate this modifier
- `/loadset 35 (level 1-35)` - Sets targetted SimPlayer to level and gear for level
- `/livenpc` - List living NPCs in zone
- `/levelup` - Maxes target's Earned XP
- `/simlocs` - Report sim zone population
- `/fastdev` - Increases player RunSpeed to 24
- `/raining` - Changes atmosphere to raining
- `/thunder` - Changes atmosphere to thunderstorm
- `/bluesky` - Changes atmosphere to blue sky
- `/dosunny` - Toggles sun
- `/gamepad` - Gamepad Control Enabled (experimental)
- `/devkill` - Kill current target
- `/preview` - Enable Demonstration Mode (CAUTION: Save Files will be overwritten!!)
- `/invisme` - Toggle Dev Invis
- `/toscene Stowaway` - Teleport to the named scene
- `/droneme` - Toggle Drone Mode
- `/debugap` - List NPCs attacking the player
- `/spychar` - List information about the target
- `/nodechk` - List Nodes in the current zone
- `/faction 5` - Modify player's faction standing of the target's faction. Use negative numbers to decrease faction.
- `/yousolo` - Removes SimPlayer from group
- `/allgrps` - List group data
- `/portsim SimName` - Teleport specified SimPlayer to player

### Players commands:
- `/players` - Get a list of players in zone
- `/time` - Get the current game time
- `/whisper PlayerName Msg` - Send a private message
- `/group` - Send a message to your group (wait, attack, guard, etc)
- `/dance` - boogie down.
- `/keyring` - List held keys
- `/all players` or `/all pla` - List all players in Erenshor
- `/shout` - Message the entire zone
- `/friend` - Target SimPlayer is a FRIEND of this character. Their progress will be loosely tied to this character's progress.
- `/time 1`, `/time10`, `/time25`, `/time50` - Set TimeScale multiplier

### Hotkeys:
- `o` - options
- `i` - inventory
- `b` - skill book
- `b` - spell book
- `c` - consider opponent
- `h` - greet your target
- `q` - autoattack toggle
- `escape (hold)` - exit to menu

# Author Information

### Brumdail

`DISCORD:` Brumdail

Special thanks to Mod config code from https://github.com/AzumattDev

For Questions or Comments, find me in the Erenshor Discord:

[![https://erenshor.com/gallery_gen/a5a278886e7e0bdedd386bd74929d9fa_fit.png](https://erenshor.com/gallery_gen/a5a278886e7e0bdedd386bd74929d9fa_fit.png)](https://discord.gg/DxqZc5Dc9q)
<a href="https://discord.gg/DxqZc5Dc9q">