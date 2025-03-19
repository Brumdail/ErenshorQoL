# ErenshorQoL
Erenshor Quality of Life Modpack based on BepInEx

## Version: 1.3.18

## Features (Configurable): /autoloot, /bank, /auction, /help, AutoSendPet, EnableAutoAttack, AutoPriceYourItem

## How it works:

- `/autoloot` - Toggles the feature to automatically Loot All items from the nearest corpse each time a creature dies.
- `/bank` - Opens the bank window
- `/auction` - Opens the auction hall window (Beta/full version only)
- `/help` - Expanded list of commands including additional available player and GM commands
- `Auto Send Pet` - If enabled, the pet will automatically be sent when you enable autoattack (*some parts of this feature still in progress*)
- `Auto Enable Autoattack` - If enabled, AutoAttack will be turned on when you use a skill (*some parts of this feature still in progress*)
- `AutoPriceYourItem` - Automatically set the maximum gold value for an item that will sell

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
Adds Postfix commands to find the nearest new corpse after the Character.DoDeath() call for Autoloot.
Adds Prefix commands to TypeText.CheckCommands() to include new commands.
Adds Prefix commands to UseSkill.DoSkill() to automatically perform actions when skills are used.
Adds Postfix commands to PlayerCombat.ToggleAttack() to automatically perform actions when autoattack is enabled.

### QoL Modded Commands:
- `/autoloot` - Toggles the feature to automatically Loot All items from the nearest corpse each time a creature dies.
- `/bank` - Opens the bank window.
- `/auction` - Opens the auction hall window.
- `/allscenes` - Lists all scenes.
- `/help now allows for /help mods, /help gm, /help player, or /help other` for the full breakdown of available commands within the build;

### Player Commands:
- `/players` - Get a list of players in zone.
- `/time` - Get the current game time.
- `/whisper PlayerName Msg` - Send a private message.
- `/group` - Send a message to your group (wait, attack, guard, etc).
- `/dance` - Boogie down.
- `/keyring` - List held keys.
- `/all players || /all pla` - List all players in Erenshor.
- `/portsim SimName` - Teleport specified SimPlayer to player.
- `/shout` - Message the entire zone.
- `/friend` - Target SimPlayer is a FRIEND of this character. Their progress will be loosely tied to this character's progress.
- `/time1, /time10, /time25, /time50` - Set TimeScale multiplier.
- `/loc` - Output player location X, Y, Z values.

#### Hotkeys:
- `o` - Options.
- `i` - Inventory.
- `b` - Skills and spells book.
- `c` - Consider opponent.
- `h` - Greet your target.
- `q` - Autoattack toggle.
- `tab` - Cycle target.
- `spacebar` - Jump.
- `escape (hold)` - Exit to menu.

### GM Commands: *not available in the demo build*
- `/iamadev` - Enable Dev Controls.
- `/allitem` - List all items.
- `/item000, /item100, .../item800` - List Items x00 through x99.
- `/blessit` - Bless item on mouse cursor.
- `/additem 12` - (12: Cloth Sleeves) Add item to inventory; use `/allitem` or `/item#00` to get item codes.
- `/cheater 35 (level 1-35)` - **OVERWRITE character level and load random level-appropriate equipment (CAUTION: Current character equipment and level will be overwritten!!)**.
- `/loadset 35 (level 1-35)` - Sets targeted SimPlayer to level and gear for level.
- `/bedazzl` - Targeted SimPlayer gets equipment randomly upgraded to sparkly.
- `/setnude` - Targeted SimPlayer loses all equipment.
- `/hpscale 1.0 (multiplier)` - NPC HP scale modifier. You must zone to activate this modifier.
- `/livenpc` - List living NPCs in zone.
- `/levelup` - Maxes target's Earned XP.
- `/level--` - Reduce target's level by 1.
- `/simlocs` - Report sim zone population.
- `/sivakme` - Add Sivakrux coin to inventory.
- `/fastdev` - Increases player RunSpeed to 24.
- `/resists` - Add 33 to all base resists.
- `/raining` - Changes atmosphere to raining.
- `/thunder` - Changes atmosphere to thunderstorm.
- `/bluesky` - Changes atmosphere to blue sky.
- `/dosunny` - Toggles sun.
- `/cantdie` - Target stays at max HP.
- `/devkill` - Kill current target.
- `/debugxp` - Add 1000 XP to the player.
- `/invisme` - Toggle Dev Invis.
- `/toscene Stowaway` - Teleport to the named scene. Check scenes with `/simlocs` or use the listed scenes.
- `/invinci` - Make player invincible.
- `/faction 5` - Modify player's faction standing of the target's faction. Use negative numbers to decrease faction.

### Other GM Commands: *not available in the demo build*
- `/viewgec` - View the total global economy.
- `/resetec` - Reset the total global economy to 0.
- `/stoprev` - Set party to relaxed state or report who is in combat.
- `/gamepad` - Enable gamepad control (experimental).
- `/steamid` - Lists which AppID and build is running.
- `/gamerdr` - NPC shouts about GamesRadar release date.
- `/testnre` - Causes a Null Reference Exception for testing.
- `/limit20, /limit30, /limit60` - Sets target frame rate.
- `/preview` - **Enable Demonstration Mode (CAUTION: Save Files will be overwritten!!)**.
- `/rocfest` - **Enable RocFest Demonstration Mode (CAUTION: Save Files will be overwritten!!)**.
- `/saveloc` - Output Save Data location.
- `/droneme` - Toggle Drone Mode for Camera. Use `/droneme` again to turn it off.
- `/debugap` - List NPCs attacking the player.
- `/spychar` - List information about the target.
- `/spyloot` - List loot information about the target.
- `/nodechk` - List Nodes in the current zone.
- `/yousolo` - Removes SimPlayer from group.
- `/allgrps` - List group data.

### Changelog:
- 2025-03-18 - Ensures Autoloot does not trigger if the player is dead. Removed extraneous debug logging. /autoloot config now includes a minimum value threshold.
- 2024-09-20 - Performance rewrite of /autoloot. Fixed compatibility with other mods. Added AutoPriceYourItem.

# Author Information

### Brumdail

`DISCORD:` Brumdail

Special thanks to Mod config code from https://github.com/AzumattDev

For Questions or Comments, find me in the Erenshor Discord:

[![https://erenshor.com/gallery_gen/a5a278886e7e0bdedd386bd74929d9fa_fit.png](https://erenshor.com/gallery_gen/a5a278886e7e0bdedd386bd74929d9fa_fit.png)](https://discord.gg/DxqZc5Dc9q)
<a href="https://discord.gg/DxqZc5Dc9q">