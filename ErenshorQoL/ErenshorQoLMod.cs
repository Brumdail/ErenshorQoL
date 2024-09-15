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
using BepInEx.Logging;
using BepInEx.Configuration;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using System.Xml.Linq;



namespace ErenshorQoL
{
    [BepInPlugin(ModGUID, ModDescription, ModVersion)]
    [BepInProcess("Erenshor.exe")]
    public class ErenshorQoLMod : BaseUnityPlugin
    {
        internal const string ModName = "ErenshorQoLMod";
        internal const string ModVersion = "1.1.1";
        internal const string ModDescription = "Erenshor Quality of Life Mods";
        internal const string Author = "Brumdail";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        //internal static readonly int windowId = 777001; //will be used later for identifying the mod's window
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
            Config.Save();
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
            }
            catch
            {
                ErenshorQoLLogger.LogError($"There was an issue loading your {ConfigFileName}");
                ErenshorQoLLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        internal static ConfigEntry<Toggle> AutoLootToggle = null!;
        internal static ConfigEntry<float> AutoLootDistance = null!;
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
        internal static ConfigEntry<Toggle> AutoRunToggle = null!;
        internal static ConfigEntry<KeyboardShortcut> AutoRunKey = null!;
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
        class AutoLoot
        {
            /// <summary>
            /// Attempts to find the latest nearby corpse to loot after each Character.DoDeath call
            /// </summary>

            static void Postfix()
            {
                if (ErenshorQoLMod.AutoLootToggle.Value == Toggle.On)
                {
                    bool autoLootDebug = false;
                    float autoLootDistance = 30f;


                    if (CorpseDataManager.AllCorpseData != null && CorpseDataManager.AllCorpseData.Count > 0)
                    {
                        CorpseData corpsetoloot = null;


                        foreach (var corpse in CorpseDataManager.AllCorpseData)
                        {
                            if (corpse != null && corpse.MyNPC != null && corpse.MyNPC.transform != null)
                            {

                                float corpseDistance = Vector3.Distance(GameData.PlayerControl.transform.position, corpse.MyNPC.transform.position);
                                if (corpseDistance < autoLootDistance)
                                {
                                    if (corpsetoloot == null) { corpsetoloot = corpse; }
                                    float corpsetolootDistance = Vector3.Distance(GameData.PlayerControl.transform.position, corpsetoloot.MyNPC.transform.position);
                                    if (corpseDistance < corpsetolootDistance) { corpsetoloot = corpse; }
                                }
                            }
                        }

                        if (corpsetoloot != null)
                        {
                            if (autoLootDebug) { UnityEngine.Debug.Log($"Corpse: {corpsetoloot.ToString()}"); }
                            if (autoLootDebug) { UpdateSocialLog.LogAdd($"Corpse: {corpsetoloot.ToString()}"); }
                            if (corpsetoloot.MyNPC != null)
                            {
                                if (autoLootDebug) { UnityEngine.Debug.Log($"NPC: {corpsetoloot.MyNPC.ToString()}"); }
                                if (autoLootDebug) { UpdateSocialLog.LogAdd($"NPC: {corpsetoloot.MyNPC.ToString()}"); }

                                LootTable loottabletoloot = corpsetoloot.MyNPC.GetComponent<LootTable>();
                                if (loottabletoloot != null)
                                {
                                    if (autoLootDebug) { UnityEngine.Debug.Log($"LootTable: {loottabletoloot.ToString()}"); }
                                    if (autoLootDebug) { UpdateSocialLog.LogAdd($"LootTable: {loottabletoloot.ToString()}"); }
                                    loottabletoloot.LoadLootTable();
                                    if (autoLootDebug) { UnityEngine.Debug.Log($"LoadedLootTable: {loottabletoloot.ToString()}"); }
                                    if (autoLootDebug) { UpdateSocialLog.LogAdd($"LoadedLootTable: {loottabletoloot.ToString()}"); }
                                    GameData.LootWindow.LootAll();
                                }
                            }
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

            static bool Prefix()
            {
                if (ErenshorQoLMod.QoLCommandsToggle.Value == Toggle.On)
                {
                    bool bankEnabled = true;
                    bool vendorEnabled = false;
                    bool auctionEnabled = true;
                    bool allSceneEnabled = false;
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
                        }
                    }
                    inputLengthCheck = GameData.TextInput.typed.text.Length >= 8;
                    if ((inputLengthCheck) && (auctionEnabled))
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
                    inputLengthCheck = GameData.TextInput.typed.text.Length >= 9;
                    if ((inputLengthCheck) && (allSceneEnabled))
                    {
                        string text = GameData.TextInput.typed.text.Substring(0, 9);
                        text = text.ToLower();
                        bool allScene = text == "/allscene";
                        if (allScene)
                        {
                            /*
                             * WIP
                            UpdateSocialLog.LogAdd("Number of loaded Scenes:", SceneManager.loadedSceneCount.ToString(), "yellow");
                            for (int s = 0; s < SceneManager.sceneCount; s++) //get total number of scenes in build
                            {
                                if (SceneManager.GetSceneByBuildIndex(s)
                                {

                                }
                            }

                            Scene[] allScenes = SceneManager.GetAllScenes();


                            Debug.Log("total number of scenes: " + allScenes.Length);
                            int numOfScenesInBuild = 0;


                            UpdateSocialLog.LogAdd(SceneManager.GetSceneByBuildIndex.toString());

                            GameData.TextInput.typed.text = "";
                            GameData.TextInput.CDFrames = 10f;
                            GameData.TextInput.InputBox.SetActive(false);
                            GameData.PlayerTyping = false;*/
                            return false;
                        }
                    }
                    inputLengthCheck = GameData.TextInput.typed.text.Length >= 5;
                    if ((inputLengthCheck) && (helpGMEnabled))
                    {
                        string text = GameData.TextInput.typed.text.Substring(0, 5);
                        text = text.ToLower();
                        bool helpGM = text == "/help";
                        if (helpGM)
                        {
                            UpdateSocialLog.LogAdd("QoL Modded commands: ", "lightblue");
                            UpdateSocialLog.LogAdd("/autoloot - Toggles the feature to automatically Loot All items from the nearest corpse each time a creature dies.", "lightblue");
                            UpdateSocialLog.LogAdd("/bank - Opens the bank window", "lightblue");
                            //UpdateSocialLog.LogAdd("/vendor - Sets target NPC to a vendor", "lightblue");
                            UpdateSocialLog.LogAdd("/auction - Opens the auction hall window", "lightblue");
                            //UpdateSocialLog.LogAdd("/allscene - Lists all scenes", "lightblue");

                            UpdateSocialLog.LogAdd("\nGM commands: *most not available in the demo build", "orange");
                            UpdateSocialLog.LogAdd("/iamadev - Enable Dev Controls", "orange");
                            UpdateSocialLog.LogAdd("/allitem - List all items", "orange");
                            UpdateSocialLog.LogAdd("/additem 11823624 - Add item (use /allitem to get item codes)", "orange");
                            UpdateSocialLog.LogAdd("/hpscale 1.0 (multiplier) - NPC HP scale modifier. You must zone to activate this modifier", "orange");
                            UpdateSocialLog.LogAdd("/loadset 35 (level 1-35) - Sets targetted SimPlayer to level and gear for level", "orange");
                            UpdateSocialLog.LogAdd("/livenpc - List living NPCs in zone", "orange");
                            UpdateSocialLog.LogAdd("/levelup - Maxes target's Earned XP", "orange");
                            UpdateSocialLog.LogAdd("/simlocs - Report sim zone population", "orange");
                            UpdateSocialLog.LogAdd("/fastdev - Increases player RunSpeed to 24", "orange");
                            UpdateSocialLog.LogAdd("/raining - Changes atmosphere to raining", "orange");
                            UpdateSocialLog.LogAdd("/thunder - Changes atmosphere to thunderstorm", "orange");
                            UpdateSocialLog.LogAdd("/bluesky - Changes atmosphere to blue sky", "orange");
                            UpdateSocialLog.LogAdd("/dosunny - Toggles sun", "orange");
                            UpdateSocialLog.LogAdd("/gamepad - Gamepad Control Enabled (experimental)", "orange");
                            UpdateSocialLog.LogAdd("/devkill - Kill current target", "orange");
                            UpdateSocialLog.LogAdd("/preview - Enable Demonstration Mode (CAUTION: Save Files will be overwritten!!)", "red");
                            UpdateSocialLog.LogAdd("/invisme - Toggle Dev Invis", "orange");
                            UpdateSocialLog.LogAdd("/toscene Stowaway - Teleport to the named scene", "orange");
                            UpdateSocialLog.LogAdd("/droneme - Toggle Drone Mode", "orange");
                            UpdateSocialLog.LogAdd("/debugap - List NPCs attacking the player", "orange");
                            UpdateSocialLog.LogAdd("/spychar - List information about the target", "orange");
                            UpdateSocialLog.LogAdd("/nodechk - List Nodes in the current zone", "orange");
                            UpdateSocialLog.LogAdd("/faction 5 - Modify player's faction standing of the target's faction. Use negative numbers to decrease faction.", "orange");
                            UpdateSocialLog.LogAdd("/yousolo - Removes SimPlayer from group", "orange");
                            UpdateSocialLog.LogAdd("/allgrps - List group data", "orange");
                            UpdateSocialLog.LogAdd("/portsim SimName - Teleport specified SimPlayer to player", "orange");

                            UpdateSocialLog.LogAdd("\nPlayers commands:", "yellow");
                            UpdateSocialLog.LogAdd("/players - Get a list of players in zone", "yellow");
                            UpdateSocialLog.LogAdd("/time - Get the current game time", "yellow");
                            UpdateSocialLog.LogAdd("/whisper PlayerName Msg - Send a private message", "yellow");
                            UpdateSocialLog.LogAdd("/group - Send a message to your group (wait, attack, guard, etc)", "yellow");
                            UpdateSocialLog.LogAdd("/dance - boogie down.", "yellow");
                            UpdateSocialLog.LogAdd("/keyring - List held keys", "yellow");
                            UpdateSocialLog.LogAdd("/all players || /all pla - List all players in Erenshor", "yellow");
                            UpdateSocialLog.LogAdd("/shout - Message the entire zone", "yellow");
                            UpdateSocialLog.LogAdd("/friend - Target SimPlayer is a FRIEND of this character. Their progress will be loosely tied to this character's progress.", "yellow");
                            UpdateSocialLog.LogAdd("/time 1, /time10, /time25, /time50 - Set TimeScale multiplier", "yellow");
                            UpdateSocialLog.LogAdd("Hotkeys:", "yellow");
                            UpdateSocialLog.LogAdd("o - options", "yellow");
                            UpdateSocialLog.LogAdd("i - inventory", "yellow");
                            UpdateSocialLog.LogAdd("b - skill book", "yellow");
                            UpdateSocialLog.LogAdd("b - spell book", "yellow");
                            UpdateSocialLog.LogAdd("c - consider opponent", "yellow");
                            UpdateSocialLog.LogAdd("h - greet your target", "yellow");
                            UpdateSocialLog.LogAdd("q - autoattack toggle", "yellow");
                            UpdateSocialLog.LogAdd("escape (hold) - exit to menu", "yellow");

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
    }
}
