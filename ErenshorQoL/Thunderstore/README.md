# ErenshorQoL
Erenshor Quality of Life Modpack based on BepInEx

## Version: 1.10.14

## Features (Configurable): /auction, /bank, /forge, /help, AutoSendPet, EnableAutoAttack, AutoPriceYourItem

## Latest Changes:
Cleaned up deprecated code and verified with Halloween Event. Added Configurable KeyBinds for Bank, Auction, and Forge commands.

## How it works:

- `/auction` - Opens the auction hall window (Beta/full version only)
- `/bank` - Opens the bank window
- `/forge` - Opens the forge (blacksmithing) window
- `/help` - Expanded list of commands including additional available player and GM commands
- `Auto Send Pet` - DEPRECATED. The current game build has a toggle to auto assist the group.
- `Auto Enable Autoattack` - DEPRECATED. The current game build will enable autoattack on hostile actions.
- `AutoPriceYourItem` - Automatically set the maximum gold value for an item that will sell
- `/autoloot` - DEPRECATED. Check out https://thunderstore.io/c/erenshor/p/et508/Loot_Manager/ for a great implementation

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
Adds Prefix commands to `TypeText.CheckCommands` to include new commands.
Adds Postfix commands to `AuctionHouseUI.OpenListItem` to automatically add an item price.

### QoL Modded Commands:
- `/auction` - Opens the auction hall window
- `/bank` - Opens the bank window.
- `/forge` - Opens the forge (blacksmithing) window
- `/allscenes` - Lists all scenes.
- `/help now allows for /help mods, /help gm, /help player, or /help other` for the full breakdown of available commands within the build;

### Player Commands:
- `/players` - Get a list of players in zone.
- `/whisper PlayerName Msg` - Send a private message.
- `/group` - Send a message to your group (wait, attack, guard, etc).
- `/dance` - Boogie down.
- `/keyring` - List held keys.
- `/showmap` - Toggle the Map
- `/ruleset` - Display server modifiers
- `/all players || /all pla` - List all players in Erenshor.
- `/portsim SimName` - Teleport specified SimPlayer to player.
- `/setname SimName` - Rename targetted SimPlayer
- `/shout` - Message the entire zone.
- `/friend` - Target SimPlayer is a FRIEND of this character. Their progress will be loosely tied to this character's progress.
- `/time1` - Set Day/Night Cycle to normal speed.
- `/time10, /time25, /time50` - Set TimeScale multiplier to 10x 25x or 50x speed.
- `/time` - See what time it is.
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
- `/loadset 35 (level 1-35)` - Sets targeted SimPlayer to level and gear for level. (CAUTION: Current character equipment and level will be overwritten!!)
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
- `/killall` - Kill all spawned NPCs.
- `/add50xp` - Add 50 xp to the player.
- `/debugxp` - Add 1000 xp to the player.
- `/respec` - Proficiency Points Reset.
- `/specs` - List targeted SimPlayer's proficiency points.
- `/dospec` - Targeted SimPlayer has proficiency points reset.
- `/addphse` - Applies Cave Lung debuff to the player.
- `/bombard` - Applies a myriad of detrimental spells to the target.
- `/invisme` - Toggle Dev Invis.
- `/toscene Stowaway` - Teleport to the named scene. Check scenes with `/simlocs` or `/allscenes` or use the listed scenes:
  `Stowaway`, `Tutorial`, `Abyssal`, `Azure`, `AzynthiClear`, `Blight`, `Bonepits`, `Brake`, `Braxonia`, `Braxonian`, `Duskenlight`, `Elderstone`, `FernallaField`, `Hidden`, `Krakengard`, `Loomingwood`, `Lost Cellar`, `Malaroth`, `PrielPlateau`, `Ripper`, `Rockshade`, `Rottenfoot`, `SaltedStrand`, `Silkengrass`, `Soluna`, `Underspine`, `Vitheo`, `VitheosEnd`, `Willowwatch`, `Windwashed`
- `/invinci` - Make player invincible.
- `/faction 5` - Modify player's faction standing of the target's faction. Use negative numbers to decrease faction.

### Other GM Commands: *not available in the demo build*
- `/viewgec` - View the total global economy.
- `/resetec` - Reset the total global economy to 0.
- `/stoprev` - Set party to relaxed state or report who is in combat.
- `/gamepad` - Enable gamepad control (experimental).
- `/control` - Toggle between modern and standard controls.
- `/steamid` - Lists which AppID and build is running.
- `/gamerdr` - NPC shouts about GamesRadar release date.
- `/testnre` - Causes a Null Reference Exception for testing.
- `/force2h` - Target SimPlayer forced to use two-handed weapon.
- `/ascview` - View target SimPlayer ascensions.
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
- `/dismiss` - Dismiss SimPlayer from group.
- `/allgrps` - List group data.
- `/delachv` - **CAUTION** Clear All Achievements. 
- `/bkquest` - Load Back Quest Achievements.

### Changelog:
- 2025-10-13 - Cleaned up deprecated code and verified with Halloween Event. Added Configurable KeyBinds for Bank, Auction, and Forge commands.
- 2025-09-25 - Updated with 0.2 game version fixes. Removed AutoLoot feature (broken and superseded by ET508's LootManager). Fixed /help to open the new Help menu and also added the list of the new debug commands in the latest build.
- 2025-04-21 - Defaulting AutoSendPet and AutoAttack off when the mod loads due to conflicts with the game patch auto-attack changes. Removed unimplemented "Enable AutoLooting into the Bank" config option.
- 2025-04-19 - AutoSendPet and AutoAttack will no longer activate on rocks or invulnerable NPCs.
- 2025-04-18 - Added /forge command, added SendPetOnAggro, AttackOnAggro, and fixed some bugs. AutoSendPet and AutoAttack will only activate on hostile targets.
- 2025-04-17 - Improved performance. Removed unused config option.
- 2025-03-18 - Ensures Autoloot does not trigger if the player is dead. Removed extraneous debug logging. /autoloot config now includes a minimum value threshold.
- 2024-09-20 - Performance rewrite of /autoloot. Fixed compatibility with other mods. Added AutoPriceYourItem.

# Author Information

### Brumdail

`DISCORD:` Brumdail

Special thanks to Mod config code from https://github.com/AzumattDev

For Questions or Comments, find me in the Erenshor Discord:

[![https://erenshor.com/gallery_gen/a5a278886e7e0bdedd386bd74929d9fa_fit.png](https://erenshor.com/gallery_gen/a5a278886e7e0bdedd386bd74929d9fa_fit.png)](https://discord.gg/DxqZc5Dc9q)
<a href="https://discord.gg/DxqZc5Dc9q">