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
using UnityEngine.Audio;
using Unity;
using BepInEx.Logging;
using BepInEx.Configuration;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text;

namespace ErenshorQoL
{
    [BepInPlugin(ModGUID, ModDescription, ModVersion)]
    [BepInProcess("Erenshor.exe")]
    public class ErenshorQoLMod : BaseUnityPlugin
    {
        internal const string ModName = "ErenshorQoLMod";
        internal const string ModVersion = "1.4.17.0942"; //const so should be manually updated before release
        internal const string ModTitle = "Erenshor Quality of Life Mods";
        internal const string ModDescription = "Erenshor Quality of Life Mods";
        internal const string Author = "Brumdail";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        //internal static readonly int windowId = 777001; //will be used later for identifying the mod's window
        internal static ErenshorQoLMod context = null!;

        private readonly Harmony harmony = new Harmony(ModGUID);

        public static readonly ManualLogSource ErenshorQoLLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        private static string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionAttribute = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            return versionAttribute?.Version ?? "0.0.0.0";
        }

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
                //ErenshorQoLLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
                Config.SaveOnConfigSet = true;

                if (ErenshorQoLMod.AutoLootToBankToggle.Value == ErenshorQoLMod.Toggle.On)
                {
                    ErenshorQoLMod.AutoLootToBankToggle = ErenshorQoLMod.context.config("1 - AutoLoot", "Enable AutoLooting into the Bank", ErenshorQoLMod.Toggle.Off, "(not yet implemented) Enable automatic looting of items into your bank?");
                }
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
        internal static ConfigEntry<Toggle> AutoLootToBankToggle = null!;
        internal static ConfigEntry<Toggle> QoLCommandsToggle = null!;
        internal static ConfigEntry<Toggle> AutoAttackToggle = null!;
        internal static ConfigEntry<Toggle> AutoAttackOnSkillToggle = null!;
        internal static ConfigEntry<Toggle> AutoAttackOnSpellToggle = null!;
        internal static ConfigEntry<Toggle> AutoAttackOnGroupAttackToggle = null!;
        internal static ConfigEntry<Toggle> AutoAttackOnPetAttackToggle = null!;
        internal static ConfigEntry<Toggle> AutoPetToggle = null!;
        internal static ConfigEntry<Toggle> AutoPetOnSkillToggle = null!;
        internal static ConfigEntry<Toggle> AutoPetOnSpellToggle = null!;
        internal static ConfigEntry<Toggle> AutoPetOnGroupAttackToggle = null!;
        internal static ConfigEntry<Toggle> AutoPetOnAutoAttackToggle = null!;
        internal static ConfigEntry<Toggle> AutoGroupAttackToggle = null!;
        internal static ConfigEntry<Toggle> AutoGroupAttackOnSkillToggle = null!;
        internal static ConfigEntry<Toggle> AutoGroupAttackOnSpellToggle = null!;
        internal static ConfigEntry<Toggle> AutoGroupAttackOnPetAttackToggle = null!;
        internal static ConfigEntry<Toggle> AutoGroupAttackOnAutoAttackToggle = null!;
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

        /*
        internal ConfigEntry<T> TextEntryConfig<T>(string group, string name, T value, string desc)
        {
            ConfigurationManagerAttributes attributes = new()
            {
                CustomDrawer = Utilities.TextAreaDrawer
            };
            return Config.Bind(group, name, value, new ConfigDescription(desc, null, attributes));
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order;
            [UsedImplicitly] public bool? Browsable;
            [UsedImplicitly] public string? Category;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        }*/

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

        public class AutoPet
        /// <summary>
        /// Automatically sends the pet if it's not attacking your target
        /// </summary>
        {
            public static void AutoSendPet(string activatedFrom)
            {
                bool petActive = GameData.PlayerControl.Myself.MyCharmedNPC != null;
                bool hasTarget = GameData.PlayerControl.CurrentTarget != null;
                var charmedNPC = GameData.PlayerControl.Myself.MyCharmedNPC;
                var curTarget = GameData.PlayerControl.CurrentTarget;
                bool isAggressive = false;
                if (hasTarget && petActive)
                {
                    //TODO - something like:
                    //if (GameData.PlayerControl.CurrentTarget.MyFaction. < -200f)
                    isAggressive = true;
                }

                // GameData.PlayerControl.CurrentTarget.AggressiveTowards
                if (petActive && hasTarget && isAggressive)
                {
                    if ((charmedNPC.CurrentAggroTarget == null) || (charmedNPC.CurrentAggroTarget != curTarget))
                    {
                        if (hasTarget)
                        {
                            charmedNPC.CurrentAggroTarget = curTarget;
                        }
                    }
                }
            }
            public static void AutoBackoffPet(string activatedFrom)
            {
                bool petActive = GameData.PlayerControl.Myself.MyCharmedNPC != null;

                if ((petActive) && (GameData.PlayerControl.Myself.MyCharmedNPC.CurrentAggroTarget != null))
                {
                    GameData.PlayerControl.Myself.MyCharmedNPC.CurrentAggroTarget = null;
                    UpdateSocialLog.LogAdd($"{GameData.PlayerControl.Myself.MyCharmedNPC.NPCName.ToString()} says: backing off...");
                }
            }
        }
        public class AutoAttack
        /// <summary>
        /// Automatically sends the pet if it's not attacking your target
        /// </summary>
        {
            public static void EnableAutoAttack(string activatedFrom)
            {
                bool autoAttackDebug = false;

                if (autoAttackDebug) { UpdateSocialLog.LogAdd($"Auto-Attack On " + activatedFrom + " - GameData.Autoattacking is: {GameData.Autoattacking}", "lightblue"); }

                if (GameData.Autoattacking == false)
                {
                    // Find the PlayerCombat component
                    PlayerCombat playerCombat = GameData.PlayerControl.Myself.GetComponent<PlayerCombat>();

                    if (playerCombat != null)
                    {
                        // Use AccessTools reflection to get the private method info
                        MethodInfo toggleAttackMethod = AccessTools.Method(typeof(PlayerCombat), "ToggleAttack");

                        if (toggleAttackMethod != null)
                        {
                            // Invoke the private method
                            toggleAttackMethod.Invoke(playerCombat, null);
                        }
                        if (autoAttackDebug) { UpdateSocialLog.LogAdd($"Activeated Auto-Attack On " + activatedFrom + " - GameData.Autoattacking is: {GameData.Autoattacking}", "orange"); }
                    }
                }
            }
        }
        public class AutoGroupCommand
        /// <summary>
        /// Automatically command the group to attack if they are not attacking your target
        /// </summary>
        {
            public static void AutoCommandAttack(string activatedFrom)
            {
                /*
                    bool autoAttackDebug = false;

                    if (autoAttackDebug) { UpdateSocialLog.LogAdd($"Auto-Command Group From " + activatedFrom + " - GameData.Autoattacking is: {GameData.Autoattacking}", "lightblue"); }

                    if (GameData.Autoattacking == false)
                    {
                        // Find the PlayerCombat component
                        PlayerCombat playerCombat = GameData.PlayerControl.Myself.GetComponent<PlayerCombat>();

                        if (playerCombat != null)
                        {
                            // Use AccessTools reflection to get the private method info
                            MethodInfo toggleAttackMethod = AccessTools.Method(typeof(PlayerCombat), "ToggleAttack");

                            if (toggleAttackMethod != null)
                            {
                                // Invoke the private method
                                toggleAttackMethod.Invoke(playerCombat, null);
                            }
                            if (autoAttackDebug) { UpdateSocialLog.LogAdd($"Activeated Auto-Attack On " + activatedFrom + " - GameData.Autoattacking is: {GameData.Autoattacking}", "orange"); }
                        }
                    }
                */
            }
        }

        [HarmonyPatch(typeof(Character))]
        [HarmonyPatch("DoDeath")]
        [HarmonyPriority(50000)]
        [HarmonyAfter("Brumdail.ErenshorREL")]
        class AutoLoot
        {
            /// <summary>
            /// Attempts to find the latest nearby corpse to loot after each Character.DoDeath call
            /// </summary>

            static void Postfix(Character __instance)
            {
                if ((ErenshorQoLMod.AutoLootToggle.Value == Toggle.On) && (__instance != null) && (__instance.isNPC) && (__instance.MyNPC != null) && (GameData.PlayerControl.Myself.Alive))
                {
                    bool autoLootDebug = false;
                    float autoLootDistance = 30f;
                    autoLootDistance = ErenshorQoLMod.AutoLootDistance.Value;
                    if (autoLootDistance < 0) { autoLootDistance = 0; }
                    float NPCDistance = Vector3.Distance(GameData.PlayerControl.transform.position, __instance.MyNPC.transform.position);
                    if (NPCDistance < autoLootDistance)
                    {
                        LootTable loottabletoloot = __instance.MyNPC.GetComponent<LootTable>();
                        if (loottabletoloot != null)
                        {
                            if (autoLootDebug) { UpdateSocialLog.LogAdd($"LootTable: {loottabletoloot.ToString()}"); }
                            loottabletoloot.LoadLootTable();
                            if (autoLootDebug) { UpdateSocialLog.LogAdd($"LoadedLootTable: {loottabletoloot.ToString()}"); }
                            GameData.LootWindow.LootAll();
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TypeText))]
        [HarmonyPatch("CheckCommands")]
        class QoLCommands
        {
            /// <summary>
            /// Adds new /commands to the game: /bank, /vendor, /auction and updates /help to include gm commands
            /// </summary>


            static void HelpMods()
            {
                UpdateSocialLog.LogAdd("QoL Modded commands: ", "lightblue");
                UpdateSocialLog.LogAdd("/autoloot - Toggles the feature to automatically Loot All items from the nearest corpse each time a creature dies.", "lightblue");
                UpdateSocialLog.LogAdd("/bank - Opens the bank window", "lightblue");
                //UpdateSocialLog.LogAdd("/vendor - Sets target NPC to a vendor", "lightblue");
                UpdateSocialLog.LogAdd("/auction - Opens the auction hall window", "lightblue");
                UpdateSocialLog.LogAdd("/allscenes - Lists all scenes", "lightblue");
            }

            static void HelpPlayer()
            {
                UpdateSocialLog.LogAdd("\nPlayers commands:", "yellow");
                UpdateSocialLog.LogAdd("/players - Get a list of players in zone", "yellow");
                UpdateSocialLog.LogAdd("/time - Get the current game time", "yellow");
                UpdateSocialLog.LogAdd("/whisper PlayerName Msg - Send a private message", "yellow");
                UpdateSocialLog.LogAdd("/group - Send a message to your group (wait, attack, guard, etc)", "yellow");
                UpdateSocialLog.LogAdd("/dance - boogie down.", "yellow");
                UpdateSocialLog.LogAdd("/keyring - List held keys", "yellow");
                UpdateSocialLog.LogAdd("/all players || /all pla - List all players in Erenshor", "yellow");
                UpdateSocialLog.LogAdd("/portsim SimName - Teleport specified SimPlayer to player", "yellow");
                UpdateSocialLog.LogAdd("/shout - Message the entire zone", "yellow");
                UpdateSocialLog.LogAdd("/friend - Target SimPlayer is a FRIEND of this character. Their progress will be loosely tied to this character's progress.", "yellow");
                UpdateSocialLog.LogAdd("/time1, /time10, /time25, /time50 - Set TimeScale multiplier", "yellow");
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
                UpdateSocialLog.LogAdd("\nGM commands: *not available in the demo build", "orange");
                UpdateSocialLog.LogAdd("/iamadev - Enable Dev Controls", "orange");
                UpdateSocialLog.LogAdd("/allitem - List all items", "orange");
                UpdateSocialLog.LogAdd("/item000, /item100, .../item 800  - List Items x00 through x99", "orange");
                UpdateSocialLog.LogAdd("/blessit - Bless item on mouse cursor", "orange");
                UpdateSocialLog.LogAdd("/additem 12 - (12: Cloth Sleeves) Add item to inventory; use /allitem or /item#00 to get item codes)", "orange");
                UpdateSocialLog.LogAdd("/cheater 35 (level 1-35) - OVERWRITE character level and load random level-appropriate equipment (CAUTION: Current character equipment and level will be overwritten!!)", "red");
                UpdateSocialLog.LogAdd("/loadset 35 (level 1-35) - Sets targetted SimPlayer to level and gear for level", "orange");
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
                UpdateSocialLog.LogAdd("/debugxp - Add 1000 xp to the player", "orange");
                UpdateSocialLog.LogAdd("/invisme - Toggle Dev Invis", "orange");
                UpdateSocialLog.LogAdd("/toscene Stowaway - Teleport to the named scene (may need to use Unstuck Me). Check scenes with /simlocs or use this list of Scenes: Abyssal, Azure, AzynthiClear, Blight, Bonepits, Brake, Braxonia, Braxonian, Duskenlight, Elderstone, FernallaField, Hidden, Krakengard, Loomingwood, Lost Cellar, Malaroth, PrielianPlateau, Ripper, Rockshade, Rottenfoot, SaltedStrand, Silkengrass, Soluna, Stowaway, Tutorial (Island Tomb), Underspine, Vitheo, Windwashed", "orange");
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
                UpdateSocialLog.LogAdd("/steamid - Lists which AppID and build is running)", "orange");
                UpdateSocialLog.LogAdd("/gamerdr - NPC shouts about GamesRadar release date", "orange");
                UpdateSocialLog.LogAdd("/testnre - Causes a Null Reference Exception for testing", "orange");
                UpdateSocialLog.LogAdd("/limit20, /limit30, /limit60 - Sets target frame rate", "orange");
                UpdateSocialLog.LogAdd("/preview - Enable Demonstration Mode (CAUTION: Save Files will be overwritten!!)", "red");
                UpdateSocialLog.LogAdd("/rocfest - Enable RocFest Demonstration Mode (CAUTION: Save Files will be overwritten!!)", "red");
                UpdateSocialLog.LogAdd("/saveloc Output Save Data location", "orange");
                UpdateSocialLog.LogAdd("/droneme - Toggle Drone Mode for Camera (/droneme again to turn off)", "orange");
                UpdateSocialLog.LogAdd("/debugap - List NPCs attacking the player", "orange");
                UpdateSocialLog.LogAdd("/spychar - List information about the target", "orange");
                UpdateSocialLog.LogAdd("/spyloot - List loot information about the target", "orange");
                UpdateSocialLog.LogAdd("/nodechk - List Nodes in the current zone", "orange");
                UpdateSocialLog.LogAdd("/yousolo - Removes SimPlayer from group", "orange");
                UpdateSocialLog.LogAdd("/allgrps - List group data", "orange");
            }
                static bool Prefix()
            {
                if (ErenshorQoLMod.QoLCommandsToggle.Value == Toggle.On)
                {
                    bool bankEnabled = true;
                    bool vendorEnabled = false;
                    bool auctionEnabled = true;
                    bool allSceneEnabled = true;
                    bool helpGMEnabled = true;

                    bool inputLengthCheck = false;
                    inputLengthCheck = GameData.TextInput.typed.text.Length >= 9;
                    if (inputLengthCheck)
                    {
                        string text = GameData.TextInput.typed.text.Substring(0, 9);
                        text = text.ToLower();
                        bool autoloot = text == "/autoloot";
                        if (autoloot)
                        {
                            if (ErenshorQoLMod.AutoLootToggle.Value == Toggle.Off)
                            {
                                ErenshorQoLMod.AutoLootToggle.Value = Toggle.On;
                            }
                            else if (ErenshorQoLMod.AutoLootToggle.Value == Toggle.On)
                            {
                                ErenshorQoLMod.AutoLootToggle.Value = Toggle.Off;
                            }
                            UpdateSocialLog.LogAdd("AutoLoot: " + ErenshorQoLMod.AutoLootToggle.Value.ToString(), "orange");
                            GameData.TextInput.typed.text = "";
                            GameData.TextInput.CDFrames = 10f;
                            GameData.TextInput.InputBox.SetActive(false);
                            GameData.PlayerTyping = false;
                            return false;
                        }
                    }
                    inputLengthCheck = GameData.TextInput.typed.text.Length >= 5;
                    if ((inputLengthCheck) && (bankEnabled))
                    {
                        string text = GameData.TextInput.typed.text.Substring(0, 5);
                        text = text.ToLower();
                        bool bank = text == "/bank";
                        if (bank)
                        {
                            GameData.BankUI.OpenBank(GameData.PlayerControl.transform.position);
                            GameData.TextInput.typed.text = "";
                            GameData.TextInput.CDFrames = 10f;
                            GameData.TextInput.InputBox.SetActive(false);
                            GameData.PlayerTyping = false;
                            return false;
                        }
                    }
                    inputLengthCheck = GameData.TextInput.typed.text.Length >= 7;
                    if ((inputLengthCheck) && (vendorEnabled))
                    {
                        string text = GameData.TextInput.typed.text.Substring(0, 7);
                        text = text.ToLower();
                        bool vendor = text == "/vendor";

                        //TODO: This doesn't work. The idea would be to spawn a vendor nearby the player.
                        /*
                        GameObject myVendor;
                        GameData.NPCEffects.
                        myVendor = ShiverEvent.Kio;


                        if (vendor && GameData.PlayerControl.CurrentTarget != null && GameData.PlayerControl.CurrentTarget.isNPC)
                        {
                            Character qolVendor = GameData.PlayerControl.CurrentTarget;
                            if (qolVendor.isVendor == false)
                            {
                                qolVendor.isVendor = true;
                            }
                            if (qolVendor.GetComponent<VendorInventory>() == null)
                            {

                                //qolVendor.gameObject.AddComponent<VendorInventory>();
                                qolVendor.GetComponent<VendorInventory>().ItemsForSale.Add(Resources.Load("Bread") as Item);
                                qolVendor.GetComponent<VendorInventory>().ItemsForSale.Add(Resources.Load("Water") as Item);
                                for (int i = 0; i <= 11; i++)
                                {
                                    bool emptyVendorItems = i >= qolVendor.GetComponent<VendorInventory>().ItemsForSale.Count;
                                    if (emptyVendorItems)
                                    {
                                        qolVendor.GetComponent<VendorInventory>().ItemsForSale.Add(Resources.Load("Empty") as Item);
                                        //this.ItemsForSale.Add(Resources.Load("Items/Empty") as Item);
                                    }
                                }

                                UpdateSocialLog.LogAdd("Vendor setup for: " + qolVendor.name, "yellow");
                                GameData.TextInput.typed.text = "";
                                GameData.TextInput.CDFrames = 10f;
                                GameData.TextInput.InputBox.SetActive(false);
                                GameData.PlayerTyping = false;
                                return false;
                            }

                            //GameData.VendorWindow.LoadWindow(qolVendor.GetComponent<VendorInventory>().ItemsForSale, qolVendor.GetComponent<VendorInventory>());
                        }
                        else
                        {
                            UpdateSocialLog.LogAdd("/vendor command requires an NPC target", "yellow");
                        }*/
                    }
                    inputLengthCheck = GameData.TextInput.typed.text.Length >= 8;
                    if ((inputLengthCheck) && (auctionEnabled) && (!GameData.GM.DemoBuild))
                    {
                        string text = GameData.TextInput.typed.text.Substring(0, 8);
                        text = text.ToLower();
                        bool auction = text == "/auction";
                        if (auction)
                        {
                            GameData.AHUI.OpenAuctionHouse(GameData.PlayerControl.transform.position);
                            GameData.TextInput.typed.text = "";
                            GameData.TextInput.CDFrames = 10f;
                            GameData.TextInput.InputBox.SetActive(false);
                            GameData.PlayerTyping = false;
                            return false;
                        }
                    }
                    inputLengthCheck = GameData.TextInput.typed.text.Length >= 10;
                    if ((inputLengthCheck) && (allSceneEnabled))
                    {
                        string text = GameData.TextInput.typed.text.Substring(0, 10);
                        text = text.ToLower();
                        bool allScene = text == "/allscenes";
                        if (allScene)
                        {
                            StringBuilder zoneNamesBuilder = new StringBuilder("Stowaway, Tutorial");

                            foreach (ZoneAtlasEntry zoneAtlasEntry in ZoneAtlas.Atlas)
                            {
                                zoneNamesBuilder.Append(", " + zoneAtlasEntry.ZoneName);
                            }

                            string zoneNames = zoneNamesBuilder.ToString();
                            UpdateSocialLog.LogAdd("Zone Names: " + zoneNames);

                            GameData.TextInput.typed.text = "";
                            GameData.TextInput.CDFrames = 10f;
                            GameData.TextInput.InputBox.SetActive(false);
                            GameData.PlayerTyping = false;
                            return false;
                        }
                    }

                    inputLengthCheck = GameData.TextInput.typed.text.Length >= 5;
                    if ((inputLengthCheck) && (helpGMEnabled))
                    {
                        bool helpGM = false;
                        bool helpMods = false;
                        bool helpOther = false;
                        bool helpPlayer = false;
                        bool help = false;

                        if (GameData.TextInput.typed.text.Length >= 12)
                        {
                            string text = GameData.TextInput.typed.text.Substring(0, 12);
                            text = text.ToLower();
                            helpPlayer = text == "/help player";
                        }
                        else if (GameData.TextInput.typed.text.Length >= 11)
                        {
                            string text = GameData.TextInput.typed.text.Substring(0, 11);
                            text = text.ToLower();
                            helpOther = text == "/help other";
                        }
                        else if (GameData.TextInput.typed.text.Length >= 10)
                        {
                            string text = GameData.TextInput.typed.text.Substring(0, 10);
                            text = text.ToLower();
                            helpMods = text == "/help mods";
                        }                        
                        else if (GameData.TextInput.typed.text.Length >= 8)
                        {
                            string text = GameData.TextInput.typed.text.Substring(0, 8);
                            text = text.ToLower();
                            helpGM = text == "/help gm";
                        }
                        else if (GameData.TextInput.typed.text.Length >= 5)
                        {
                            string text = GameData.TextInput.typed.text.Substring(0, 5);
                            text = text.ToLower();
                            help = text == "/help";
                        }

                        if (helpGM || helpMods || helpOther || helpPlayer || help)
                        {
                            if (helpGM)
                            {
                                HelpGM();
                            }
                            else if (helpMods)
                            {
                                HelpMods();
                            }
                            else if (helpOther)
                            {
                                HelpOther();
                            }
                            else if (helpPlayer)
                            {
                                HelpPlayer();
                            }
                            else if (help)
                            {
                                HelpOther();
                                HelpGM();
                                HelpPlayer();
                                HelpMods();

                                UpdateSocialLog.LogAdd("\nUse /help mods, /help gm, /help player, or /help other for the individual lists,", "orange");
                            }
                            GameData.TextInput.typed.text = "";
                            GameData.TextInput.CDFrames = 10f;
                            GameData.TextInput.InputBox.SetActive(false);
                            GameData.PlayerTyping = false;
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UseSkill))]
        [HarmonyPatch("DoSkill")]
        class AutoOnSkill
        {
            /// <summary>
            /// Automatically turn on Auto-Attack or Automatically send your pet when you use an offensive skill
            /// </summary>

            static void Prefix()
            {


                if (ErenshorQoLMod.AutoPetToggle.Value == Toggle.On)
                {
                    if (ErenshorQoLMod.AutoPetOnSkillToggle.Value == Toggle.On)
                    {
                        //HELP - Severity	Code	Description	Project	File	Line	Suppression State	Details
                        //Error(active)  CS0103 The name 'AutoPet' does not exist in the current context ErenshorQoL C: \Users\brumd\Documents\GitHub\ErenshorQoL\ErenshorQoL\ErenshorQoLMod.cs   468


                        AutoPet.AutoSendPet("Skill");
                    }
                }
                if (ErenshorQoLMod.AutoAttackToggle.Value == Toggle.On)
                {
                    if (ErenshorQoLMod.AutoAttackOnSkillToggle.Value == Toggle.On)
                    {
                        //HELP - Severity	Code	Description	Project	File	Line	Suppression State	Details
                        //Error(active)  CS0103 The name 'AutoAttack' does not exist in the current context ErenshorQoL C: \Users\brumd\Documents\GitHub\ErenshorQoL\ErenshorQoL\ErenshorQoLMod.cs   472
                        AutoAttack.EnableAutoAttack("Skill");
                    }
                }

            }
        }

        [HarmonyPatch(typeof(PlayerCombat))]
        [HarmonyPatch("ToggleAttack")]
        class AutoOnAutoattack
        {
            /// <summary>
            /// Automatically perform actions when Auto-Attack is used
            /// </summary>

            static void Postfix()
            {
                if (ErenshorQoLMod.AutoPetToggle.Value == Toggle.On)
                {
                    if ((ErenshorQoLMod.AutoPetOnSkillToggle.Value == Toggle.On) && (GameData.Autoattacking == true))
                    {
                        AutoPet.AutoSendPet("AutoAttack");
                    }
                    if ((ErenshorQoLMod.AutoPetOnSkillToggle.Value == Toggle.On) && (GameData.Autoattacking == false))
                    {
                        AutoPet.AutoBackoffPet("AutoAttack");
                    }
                    //SimPlayerGrouping.GroupAttack();
                    /*
                    if (ErenshorQoLMod.AutoGroupAttackToggle.Value == Toggle.On)
                    {
                        if (ErenshorQoLMod.AutoGroupAttackOnAutoAttackToggle.Value == Toggle.On)
                        {
                            AutoPet.AutoSendPet("AutoAttack");
                        }
                    }
                    */
                }
            }

        }

        [HarmonyPatch(typeof(AuctionHouseUI))]
        [HarmonyPatch("OpenListItem")]
        class AutoPriceYourItem
        {
            /// <summary>
            /// Automatically set the maximum gold value for an item that will sell
            /// </summary>

            static void Postfix(AuctionHouseUI __instance)
            {
                if (ErenshorQoLMod.AutoPriceItem.Value == Toggle.On)
                {
                    int maxAHPrice = 0;
                    maxAHPrice = GameData.SlotToBeListed.MyItem.ItemValue * 6-1;
                    GameData.AHUI.ListPrice.text = $"{maxAHPrice}";
                }
            }

        }

        [HarmonyPatch(typeof(LootWindow))]
        [HarmonyPatch("LootAll")]
        public static class LootWindowPatch
        {
            public static bool Prefix(LootWindow __instance)
            {
                foreach (ItemIcon itemIcon in __instance.LootSlots)
                {
                    bool isEmpty = itemIcon.MyItem == GameData.PlayerInv.Empty;
                    if (!isEmpty)
                    {
                        if (itemIcon.MyItem.ItemValue >= ErenshorQoLMod.AutoLootMinimum.Value)
                        { //only loot if at least minimum value
                            bool isGeneralItem = itemIcon.MyItem.RequiredSlot == Item.SlotType.General;
                            bool addedToInventory = false;
                            bool normalQuantity = false;
                            bool blueBlessed = false;
                            bool pinkBlessed = false;

                            if (isGeneralItem)
                            {
                                addedToInventory = GameData.PlayerInv.AddItemToInv(itemIcon.MyItem);
                            }
                            else
                            {
                                addedToInventory = GameData.PlayerInv.AddItemToInv(itemIcon.MyItem, itemIcon.Quantity);
                                normalQuantity = itemIcon.Quantity <= 1;
                                blueBlessed = itemIcon.Quantity == 2;
                                pinkBlessed = itemIcon.Quantity == 3;

                            }
                            bool looted = addedToInventory;
                            if (looted)
                            {
                                if (normalQuantity)
                                {
                                    UpdateSocialLog.LogAdd("Looted Item: " + itemIcon.MyItem.ItemName, "yellow");
                                }
                                else if (blueBlessed)
                                {
                                    UpdateSocialLog.LogAdd("Looted Blessed Item: " + itemIcon.MyItem.ItemName + "!", "lightblue");
                                }
                                else if (pinkBlessed)
                                {
                                    UpdateSocialLog.LogAdd("Looted Blessed Item: " + itemIcon.MyItem.ItemName + "!", "pink");
                                }

                                itemIcon.MyItem = GameData.PlayerInv.Empty;
                                itemIcon.UpdateSlotImage();
                            }
                            else
                            {
                                UpdateSocialLog.LogAdd("No room for " + itemIcon.MyItem.ItemName, "yellow");
                            }
                        }
                        else
                        {
                            if (ErenshorQoLMod.AutoLootDebug.Value == Toggle.On)
                            {
                                UpdateSocialLog.LogAdd("Item below " + ErenshorQoLMod.AutoLootMinimum.Value + " threshold: " + itemIcon.MyItem.ItemName + " - Value: " + itemIcon.MyItem.ItemValue, "yellow");
                            }
                        }
                    }
                }
                GameData.PlayerAud.PlayOneShot(GameData.GM.GetComponent<Misc>().DropItem, GameData.PlayerAud.volume / 2f * GameData.SFXVol);
                __instance.CloseWindow();
                return false; // Skip the original method
            }
        }
    }
}
