using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;



namespace ErenshorQoL
{
    [BepInPlugin("erenshorqol.ErenshorMod", "Erenshor Quality of Life Modpack", "1.1.0")]
    [BepInProcess("Erenshor.exe")]
    public class ErenshorMod : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("erenshorqol.ErenshorMod");
        void Awake()
        {
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Character))]
        [HarmonyPatch("DoDeath")]
        public class AutoLoot
        {
            /// <summary>
            /// Attempts to find the latest nearby corpse to loot after each Character.DoDeath call
            /// </summary>
            private static bool _isEnabled = true;

            public static bool IsEnabled
            {
                get { return _isEnabled; }
                set { _isEnabled = value; }
            }
            
            static void Postfix()
            {
                if (_isEnabled)
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
                                if (corpsetoloot == null) { corpsetoloot = corpse; }
                                float corpsetolootDistance = Vector3.Distance(GameData.PlayerControl.transform.position, corpsetoloot.MyNPC.transform.position);
                                float corpseDistance = Vector3.Distance(GameData.PlayerControl.transform.position, corpse.MyNPC.transform.position);

                                if (corpseDistance < autoLootDistance)
                                {
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
                                if (loottabletoloot != null && loottabletoloot.ActualDrops.Count > 0)
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
            private static bool _isEnabled = true;

            public static bool IsEnabled
            {
                get { return _isEnabled; }
                set { _isEnabled = value; }
            }
            static bool Prefix()
            {
                if (_isEnabled)
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
                        bool auction = text == "/autoloot";
                        if (auction)
                        {
                            AutoLoot.IsEnabled = !AutoLoot.IsEnabled;
                            UpdateSocialLog.LogAdd("AutoLoot: " + AutoLoot.IsEnabled, "orange");
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
                        bool sell = text == "/vendor";
                        if (sell && GameData.PlayerControl.CurrentTarget != null && GameData.PlayerControl.CurrentTarget.isNPC)
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
                            UpdateSocialLog.LogAdd("GM commands: *most not available in the demo build", "orange");
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

                            UpdateSocialLog.LogAdd("\nQoL Modded commands: ", "lightblue");
                            UpdateSocialLog.LogAdd("/bank - Opens the bank window", "lightblue");
                            //UpdateSocialLog.LogAdd("/vendor - Sets target NPC to a vendor", "lightblue");
                            UpdateSocialLog.LogAdd("/auction - Opens the auction hall window", "lightblue");
                            //UpdateSocialLog.LogAdd("/allscene - Lists all scenes", "lightblue");

                            UpdateSocialLog.LogAdd("\nPlayers commands (additional):", "yellow");
                            UpdateSocialLog.LogAdd("/keyring - List held keys", "yellow");
                            UpdateSocialLog.LogAdd("/all players || /all pla - List all players in Erenshor", "yellow");
                            UpdateSocialLog.LogAdd("/shout - Message the entire zone", "yellow");
                            UpdateSocialLog.LogAdd("/friend - Target SimPlayer is a FRIEND of this character. Their progress will be loosely tied to this character's progress.", "yellow");
                            UpdateSocialLog.LogAdd("/time 1, /time10, /time25, /time50 - Set TimeScale multiplier", "yellow");

                            UpdateSocialLog.LogAdd("\nPlayers commands:", "yellow");
                            UpdateSocialLog.LogAdd("/players - Get a list of players in zone", "yellow");
                            UpdateSocialLog.LogAdd("/time - Get the current game time", "yellow");
                            UpdateSocialLog.LogAdd("/whisper PlayerName Msg - Send a private message", "yellow");
                            UpdateSocialLog.LogAdd("/group - Send a message to your group (wait, attack, guard, etc)", "yellow");
                            UpdateSocialLog.LogAdd("/dance - boogie down.", "yellow");
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
    }
}
