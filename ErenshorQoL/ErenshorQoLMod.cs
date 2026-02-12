using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.ParticleSystemJobs;
using UnityEngine.Audio;
using BepInEx.Logging;
using BepInEx.Configuration;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using System.Runtime.Serialization.Formatters;


namespace ErenshorQoL
{
    [BepInPlugin(ModGUID, ModDescription, ModVersion)]
    [BepInProcess("Erenshor.exe")]
    public class ErenshorQoLMod : BaseUnityPlugin
    {
        internal const string ModName = "ErenshorQoLMod";
        internal const string ModVersion = "2.2.11.0000"; //const so should be manually updated before release
        internal const string ModTitle = "Erenshor Quality of Life Mods";
        internal const string ModDescription = "Erenshor Quality of Life Mods";
        internal const string Author = "Brumdail";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        public bool appliedConfigChange = false; //used to check for options to change on each launch
        internal static ErenshorQoLMod context = null!;

        private readonly Harmony harmony = new Harmony(ModGUID);
        public static readonly ManualLogSource ErenshorQoLLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public enum Toggle
        {
            Off,
            On
        }

        void Awake()
        {
            context = this; // Set the context to the current instance
            Utilities.GenerateConfigs(); // Generate initial configuration if necessary
            harmony.PatchAll(); // Apply all Harmony patches
            SetupWatcher(); // Start watching for configuration file changes
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void Update()
        {
            if (QoLBankKey.Value.IsDown())
            {
                ErenshorQoLLogger.LogDebug("QoLCommands.DoBank() called");
                QoLCommands.DoBank();
            }
            else if (QoLAuctionKey.Value.IsDown())
            {
                ErenshorQoLLogger.LogDebug("QoLCommands.DoAuction() called");
                QoLCommands.DoAuction();
            }
            else if (QoLForgeKey.Value.IsDown())
            {
                ErenshorQoLLogger.LogDebug("QoLCommands.DoForge() called");
                QoLCommands.DoForge();
            }
        }


        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                ErenshorQoLLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
                Config.SaveOnConfigSet = true;

            }
            catch
            {
                ErenshorQoLLogger.LogError($"There was an issue loading your {ConfigFileName}");
                ErenshorQoLLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        #region ConfigOptions

        internal static ConfigEntry<Toggle> AutoLootToggle = null!;
        internal static ConfigEntry<Toggle> AutoLootDebug = null!;
        internal static ConfigEntry<float> AutoLootDistance = null!;
        internal static ConfigEntry<int> AutoLootMinimum = null!;
        internal static ConfigEntry<Toggle> QoLCommandsToggle = null!;
        internal static ConfigEntry<KeyboardShortcut> QoLBankKey = null!;
        internal static ConfigEntry<KeyboardShortcut> QoLAuctionKey = null!;
        internal static ConfigEntry<KeyboardShortcut> QoLForgeKey = null!;
        internal static ConfigEntry<Toggle> AutoAttackToggle = null!;
        internal static ConfigEntry<Toggle> AutoAttackOnSkillToggle = null!;
        internal static ConfigEntry<Toggle> AutoAttackOnAggro = null!;
        internal static ConfigEntry<Toggle> AutoPetToggle = null!;
        internal static ConfigEntry<Toggle> AutoPetOnSkillToggle = null!;
        internal static ConfigEntry<Toggle> AutoPetOnAggro = null!;
        internal static ConfigEntry<Toggle> AutoPetOnAutoAttackToggle = null!;
        internal static ConfigEntry<Toggle> AutoPriceItem = null!;

        internal static bool _configApplied;

        internal ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);
            return configEntry;
        }

        internal ConfigEntry<T> config<T>(string group, string name, T value, string description)
        {
            return Config.Bind(group, name, value, new ConfigDescription(description));
        }

        internal class AcceptableShortcuts : AcceptableValueBase // Used for KeyboardShortcut Configs 
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
        }

        #endregion

        [HarmonyPatch(typeof(TypeText))]
        [HarmonyPatch("CheckCommands")]
        public class QoLCommands
        {
            /// <summary>
            /// Adds new /commands to the game: /bank, /vendor, /auction and /help to include gm commands
            /// </summary>

            static void HelpMods()
            {
                if (ErenshorQoLMod.QoLCommandsToggle.Value == Toggle.Off)
                {
                    return;
                }
                UpdateSocialLog.LogAdd("QoL Modded commands: ", "lightblue");
                UpdateSocialLog.LogAdd("/bank - Opens the bank window", "lightblue");
                UpdateSocialLog.LogAdd("/forge - Opens the forge (blacksmithing) window", "lightblue");
                UpdateSocialLog.LogAdd("/auction - Opens the auction hall window", "lightblue");
                UpdateSocialLog.LogAdd("/allscenes - Lists all scenes", "lightblue");
            }
            static void HelpPlayer()
            {
                UpdateSocialLog.LogAdd("\nPlayers commands:", "yellow");
                UpdateSocialLog.LogAdd("/players - Get a list of players in zone", "yellow");
                UpdateSocialLog.LogAdd("/whisper PlayerName Msg - Send a private message", "yellow");
                UpdateSocialLog.LogAdd("/group - Send a message to your group (wait, attack, guard, etc)", "yellow");
                UpdateSocialLog.LogAdd("/dance - boogie down.", "yellow");
                UpdateSocialLog.LogAdd("/keyring - List held keys", "yellow");
                UpdateSocialLog.LogAdd("/showmap - Toggle the Map", "yellow");
                UpdateSocialLog.LogAdd("/ruleset - Display server modifiers", "yellow");
                UpdateSocialLog.LogAdd("/all players || /all pla - List all players in Erenshor", "yellow");
                UpdateSocialLog.LogAdd("/portsim SimName - Teleport specified SimPlayer to player", "yellow");
                UpdateSocialLog.LogAdd("/setname SimName - Rename targetted SimPlayer", "yellow");
                UpdateSocialLog.LogAdd("/shout - Message the entire zone", "yellow");
                UpdateSocialLog.LogAdd("/friend - Target SimPlayer is a FRIEND of this character. Their progress will be loosely tied to this character's progress.", "yellow");
                UpdateSocialLog.LogAdd("/time1 - Set Day/Night Cycle to normal speed. ");
                UpdateSocialLog.LogAdd("/time10, /time25, /time50 - Set TimeScale multiplier to 10x 25x or 50x speed.", "yellow");
                UpdateSocialLog.LogAdd("/time - See what time it is. ");
                UpdateSocialLog.LogAdd("/loc - Output player location X, Y, Z values", "yellow");
                UpdateSocialLog.LogAdd("Hotkeys:", "yellow");
                UpdateSocialLog.LogAdd("o - options", "yellow");
                UpdateSocialLog.LogAdd("i - inventory", "yellow");
                UpdateSocialLog.LogAdd("b - skills and spells book", "yellow");
                UpdateSocialLog.LogAdd("c - consider opponent", "yellow");
                UpdateSocialLog.LogAdd("h - greet your target", "yellow");
                UpdateSocialLog.LogAdd("q - autoattack toggle", "yellow");
                UpdateSocialLog.LogAdd("tab - cycle target", "yellow");
                UpdateSocialLog.LogAdd("spacebar - jump", "yellow");
                UpdateSocialLog.LogAdd("escape (hold) - exit to menu", "yellow");
            }
            static void HelpGM()
            {
                if (ErenshorQoLMod.QoLCommandsToggle.Value == Toggle.Off)
                {
                    return;
                }
                UpdateSocialLog.LogAdd("\nGM commands: *not available in the demo build", "orange");
                UpdateSocialLog.LogAdd("/iamadev - Enable Dev Controls", "orange");
                UpdateSocialLog.LogAdd("/allitem - List all items", "orange");
                UpdateSocialLog.LogAdd("/item000, /item100, .../item 800  - List Items x00 through x99", "orange");
                UpdateSocialLog.LogAdd("/blessit - Bless item on mouse cursor", "orange");
                UpdateSocialLog.LogAdd("/additem 12 - (12: Cloth Sleeves) Add item to inventory; use /allitem or /item#00 to get item codes)", "orange");
                UpdateSocialLog.LogAdd("/cheater 35 (level 1-35) - OVERWRITE character level and load random level-appropriate equipment (CAUTION: Current character equipment and level will be overwritten!!)", "red");
                UpdateSocialLog.LogAdd("/loadset 35 (level 1-35) - Sets targetted SimPlayer to level and gear for level (CAUTION: Current character equipment and level will be overwritten!!)", "red");
                UpdateSocialLog.LogAdd("/bedazzl - Targetted SimPlayer gets equipment randomly upgraded to sparkly", "orange");
                UpdateSocialLog.LogAdd("/setnude - Targetted SimPlayer loses all equipment", "orange");
                UpdateSocialLog.LogAdd("/hpscale 1.0 (multiplier) - NPC HP scale modifier. You must zone to activate this modifier", "orange");
                UpdateSocialLog.LogAdd("/livenpc - List living NPCs in zone", "orange");
                UpdateSocialLog.LogAdd("/levelup - Maxes target's Earned XP", "orange");
                UpdateSocialLog.LogAdd("/level-- - Reduce target's level by 1", "orange");
                UpdateSocialLog.LogAdd("/simlocs - Report sim zone population", "orange");
                UpdateSocialLog.LogAdd("/sivakme - Add Sivakrux coin to inventory", "orange");
                UpdateSocialLog.LogAdd("/fastdev - Increases player RunSpeed to 24", "orange");
                UpdateSocialLog.LogAdd("/resists - Add 33 to all base resists", "orange");
                UpdateSocialLog.LogAdd("/raining - Changes atmosphere to raining", "orange");
                UpdateSocialLog.LogAdd("/thunder - Changes atmosphere to thunderstorm", "orange");
                UpdateSocialLog.LogAdd("/bluesky - Changes atmosphere to blue sky", "orange");
                UpdateSocialLog.LogAdd("/dosunny - Toggles sun", "orange");
                UpdateSocialLog.LogAdd("/cantdie - Target stays at max HP", "orange");
                UpdateSocialLog.LogAdd("/devkill - Kill current target", "orange");
                UpdateSocialLog.LogAdd("/killall - Kill all spawned NPCs", "orange");
                UpdateSocialLog.LogAdd("/add50xp - Add 50 xp to the player", "orange");
                UpdateSocialLog.LogAdd("/debugxp - Add 1000 xp to the player", "orange");
                UpdateSocialLog.LogAdd("/respec - Proficiency Points Reset", "orange");
                UpdateSocialLog.LogAdd("/specs - List targetted SimPlayer's proficiency points", "orange");
                UpdateSocialLog.LogAdd("/dospec - Targetted SimPlayer has proficiency points reset", "orange");
                UpdateSocialLog.LogAdd("/addphse - Applies Cave Lung debuff to the player", "red");
                UpdateSocialLog.LogAdd("/bombard - Applies a myriad of detrimental spells to the target.", "orange");
                UpdateSocialLog.LogAdd("/invisme - Toggle Dev Invis", "orange");
                UpdateSocialLog.LogAdd("/toscene Stowaway - Teleport to the named scene (may need to use Unstuck Me). Check scenes with /simlocs or use this list of Scenes: Stowaway, Tutorial, Abyssal, Azure, AzynthiClear, Blight, Bonepits, Brake, Braxonia, Braxonian, Duskenlight, Elderstone, FernallaField, Hidden, Krakengard, Loomingwood, Lost Cellar, Malaroth, PrielPlateau, Ripper, Rockshade, Rottenfoot, SaltedStrand, Silkengrass, Soluna, Underspine, Vitheo, VitheosEnd, Willowwatch, Windwashed", "orange");
                UpdateSocialLog.LogAdd("/invinci - Make player invincible", "orange");
                UpdateSocialLog.LogAdd("/faction 5 - Modify player's faction standing of the target's faction. Use negative numbers to decrease faction.", "orange");
            }
            static void HelpOther()
            {
                UpdateSocialLog.LogAdd("\nOther GM commands: *not available in the demo build", "orange");
                UpdateSocialLog.LogAdd("/viewgec - View the total global economy", "orange");
                UpdateSocialLog.LogAdd("/resetec - Rest the total global economy to 0", "orange");
                UpdateSocialLog.LogAdd("/stoprev - Set party to relaxed state or report who is in combat", "orange");
                UpdateSocialLog.LogAdd("/gamepad - Gamepad Control Enabled (experimental)", "orange");
                UpdateSocialLog.LogAdd("/control - Toggle between modern and standard controls", "orange");
                UpdateSocialLog.LogAdd("/steamid - Lists which AppID and build is running)", "orange");
                UpdateSocialLog.LogAdd("/gamerdr - NPC shouts about GamesRadar release date", "orange");
                UpdateSocialLog.LogAdd("/testnre - Causes a Null Reference Exception for testing", "orange");
                UpdateSocialLog.LogAdd("/force2h - Target SimPlayer forced to use two-handed weapon", "orange");
                UpdateSocialLog.LogAdd("/ascview - View target SimPlayer ascensions", "orange");
                UpdateSocialLog.LogAdd("/limit20, /limit30, /limit60 - Sets target frame rate", "orange");
                UpdateSocialLog.LogAdd("/preview - Enable Demonstration Mode (CAUTION: Save Files will be overwritten!!)", "red");
                UpdateSocialLog.LogAdd("/rocfest - Enable RocFest Demonstration Mode (CAUTION: Save Files will be overwritten!!)", "red");
                UpdateSocialLog.LogAdd("/saveloc - Output Save Data location", "orange");
                UpdateSocialLog.LogAdd("/droneme - Toggle Drone Mode for Camera (/droneme again to turn off)", "orange");
                UpdateSocialLog.LogAdd("/debugap - List NPCs attacking the player", "orange");
                UpdateSocialLog.LogAdd("/spychar - List information about the target", "orange");
                UpdateSocialLog.LogAdd("/spyloot - List loot information about the target", "orange");
                UpdateSocialLog.LogAdd("/nodechk - List Nodes in the current zone", "orange");
                UpdateSocialLog.LogAdd("/yousolo - Removes SimPlayer from group", "orange");
                UpdateSocialLog.LogAdd("/dismiss - Dismiss SimPlayer from group", "orange");
                UpdateSocialLog.LogAdd("/allgrps - List group data", "orange");
                UpdateSocialLog.LogAdd("/delachv - Clear Achievements", "red");
                UpdateSocialLog.LogAdd("/bkquest - Load Back Quest Achievements", "orange");
            }
            public static void DoHelp()
            {
                ErenshorQoLLogger.LogDebug("QoLCommands.DoHelp() called");
                GameData.Misc.OpenCloseHelp();
                HelpOther();
                HelpGM();
                HelpPlayer();
                HelpMods();

                UpdateSocialLog.LogAdd("\nUse /help mods, /help gm, /help player, or /help other for the individual lists,", "orange");
            }
            public static void DoBank()
            {
                if (ErenshorQoLMod.QoLCommandsToggle.Value == Toggle.Off)
                {
                    return;
                }
                if (GameData.ItemOnCursor == null || GameData.ItemOnCursor == GameData.PlayerInv.Empty)
                {
                    GameData.BankUI.OpenBank(GameData.PlayerControl.transform.position);
                }
                else
                {
                    UpdateSocialLog.LogAdd("Remove item from cursor before interacting with the bank.", "yellow");
                }
            }
            public static void DoAuction()
            {
                if (ErenshorQoLMod.QoLCommandsToggle.Value == Toggle.Off)
                {
                    return;
                }
                if (GameData.ItemOnCursor == null || GameData.ItemOnCursor == GameData.PlayerInv.Empty)
                {
                    GameData.AHUI.OpenAuctionHouse(GameData.PlayerControl.transform.position);
                }
                else
                {
                    UpdateSocialLog.LogAdd("Remove item from cursor before interacting with the auction house.", "yellow");
                }
            }
            public static void DoForge()
            {
                if (ErenshorQoLMod.QoLCommandsToggle.Value == Toggle.Off) {return;}
                if (GameData.ItemOnCursor == null || GameData.ItemOnCursor == GameData.PlayerInv.Empty)
                {
                    GameData.PlayerAud.PlayOneShot(GameData.Misc.SmithingOpen, GameData.PlayerAud.volume * GameData.SFXVol);
                    GameData.Smithing.OpenWindow(GameData.PlayerControl.transform.position);
                }
                else
                {
                    UpdateSocialLog.LogAdd("Remove item from cursor before interacting with the forge.", "yellow");
                }
            }
            public static void DoAllScenes()
            {
                if (ErenshorQoLMod.QoLCommandsToggle.Value == Toggle.Off)
                {
                    return;
                }
                StringBuilder zoneNamesBuilder = new StringBuilder("Stowaway, Tutorial");

                foreach (ZoneAtlasEntry zoneAtlasEntry in ZoneAtlas.Atlas)
                {
                    zoneNamesBuilder.Append(", " + zoneAtlasEntry.ZoneName);
                }

                string zoneNames = zoneNamesBuilder.ToString();
                UpdateSocialLog.LogAdd("Zone Names: " + zoneNames);
            }
            public static bool Prefix(TypeText __instance)
            {
                string txt = __instance.typed.text;
                if (string.IsNullOrEmpty(txt)) return true;

                if (txt.StartsWith("/"))
                {
                    string[] spl = txt.Substring(1).Split(' ');
                    string command = spl[0].ToLower();
                    string arg = spl.Length > 1 ? spl[1].ToLower() : string.Empty;

                    switch (command)
                    {
                        case "bank":
                            QoLCommands.DoBank();
                            return false;
                        case "forge":
                            QoLCommands.DoForge();
                            return false;
                        case "auction":
                            QoLCommands.DoAuction();
                            return false;
                        case "allscenes":
                            QoLCommands.DoAllScenes();
                            return false;
                        case "help":
                            if (arg == "player")
                            {
                                HelpPlayer();
                            }
                            else if (arg == "other")
                            {
                                HelpOther();
                            }
                            else if (arg == "mods")
                            {
                                HelpMods();
                            }
                            else if (arg == "gm")
                            {
                                HelpGM();
                            }
                            else
                            {
                                ErenshorQoLLogger.LogDebug("QoLCommands.DoHelp() called");
                                DoHelp();
                            }
                            return false;
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(AuctionHouseUI))]
        [HarmonyPatch("OpenListItem")]
        public class AutoPriceYourItem
        {
            /// <summary>
            /// Automatically set the maximum gold value for an item that will sell
            /// </summary>
            static void Postfix(AuctionHouseUI __instance)
            {
                if (ErenshorQoLMod.AutoPriceItem.Value == Toggle.On)
                {
                    int maxAHPrice = 0;
                    maxAHPrice = GameData.SlotToBeListed.MyItem.ItemValue * 6 - 1;
                    GameData.AHUI.ListPrice.text = $"{maxAHPrice}";
                }
            }
        }
    }
}
