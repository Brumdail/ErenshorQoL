using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ErenshorQoL
{
    public class Utilities
    {
        public static readonly ManualLogSource ErenshorUtilitiesLogger = BepInEx.Logging.Logger.CreateLogSource("ErenshorQoLUtilities");

        internal static void ApplyConfig()
        {

            ErenshorQoLMod._configApplied = true;
        }

        internal static void GenerateConfigs()
        {
            ErenshorQoLMod.QoLCommandsToggle = ErenshorQoLMod.context.config("2 - QoL Commands", "Enable QoL Commands", ErenshorQoLMod.Toggle.On, "Enable additional Quality of Life commands like /bank, /auction, updated /help etc.");
            ErenshorQoLMod.QoLBankKey = ErenshorQoLMod.context.config("2 - QoL Commands", "Bank Key", new KeyboardShortcut(KeyCode.F9), new ConfigDescription("Key(s) used to open the Bank window. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", new ErenshorQoLMod.AcceptableShortcuts()));
            ErenshorQoLMod.QoLAuctionKey = ErenshorQoLMod.context.config("2 - QoL Commands", "Auction Key", new KeyboardShortcut(KeyCode.F10), new ConfigDescription("Key(s) used to open the Auction House window. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", new ErenshorQoLMod.AcceptableShortcuts()));
            ErenshorQoLMod.QoLForgeKey = ErenshorQoLMod.context.config("2 - QoL Commands", "Forge Key", new KeyboardShortcut(KeyCode.F10, KeyCode.LeftShift), new ConfigDescription("Key(s) used to open the Forge window. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", new ErenshorQoLMod.AcceptableShortcuts()));
            ErenshorQoLMod.AutoPriceItem = ErenshorQoLMod.context.config("6 - Auto Price Item", "Auto Set AH Item Price", ErenshorQoLMod.Toggle.On, "Automatically start with the highest sellable auction house price when adding an item.");

            // Config definitions for obsolete settings
            ConfigDefinition autoLootToggleDefinition = new ConfigDefinition("1 - AutoLoot", "Enable AutoLoot");
            ConfigDefinition autoLootDistanceDefinition = new ConfigDefinition("1 - AutoLoot", "Auto Loot Distance");
            ConfigDefinition autoLootMinimumDefinition = new ConfigDefinition("1 - AutoLoot", "Auto Loot Minimum Threshold");
            ConfigDefinition autoLootDebugDefinition = new ConfigDefinition("1 - AutoLoot", "AutoLoot Debug Messages");
            ConfigDefinition autoLootToBankToggleDefinition = new ConfigDefinition("1 - AutoLoot", "Enable AutoLooting into the Bank");
            ConfigDefinition autoAttackToggleDefinition = new ConfigDefinition("3 - Auto Attack", "Enable Auto Attack");
            ConfigDefinition autoAttackOnSkillToggleDefinition = new ConfigDefinition("3 - Auto Attack", "Auto Attack on Skill Use");
            ConfigDefinition autoAttackOnAggroDefinition = new ConfigDefinition("3 - Auto Attack", "Auto Target,Attack on Aggro");
            ConfigDefinition autoPetToggleDefinition = new ConfigDefinition("4 - Auto Pet", "Enable Auto Pet Command");
            ConfigDefinition autoPetOnSkillToggleDefinition = new ConfigDefinition("4 - Auto Pet", "Auto Pet Command on Skill Use");
            ConfigDefinition autoPetOnAggroDefinition = new ConfigDefinition("4 - Auto Pet", "Auto Pet Command on Skill Use");
            ConfigDefinition autoPetOnAutoAttackToggleDefinition = new ConfigDefinition("4 - Auto Pet", "Auto Pet Command on Auto Attack");



            if (ErenshorQoLMod.context.appliedConfigChange == false)
            {
                //Remove obsolete config sections and settings
                if(ErenshorQoLMod.context.Config.ContainsKey(autoLootToggleDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoLootToggleDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Enable AutoLoot'");
                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoLootDistanceDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoLootDistanceDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Auto Loot Distance'");
                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoLootMinimumDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoLootMinimumDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Auto Loot Minimum Threshold'");
                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoLootDebugDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoLootDebugDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'AutoLoot Debug Messages'");
                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoLootToBankToggleDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoLootToBankToggleDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Enable AutoLooting into the Bank'");
                    
                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoAttackToggleDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoAttackToggleDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Enable Auto Attack'");

                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoAttackOnSkillToggleDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoAttackOnSkillToggleDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Auto Attack on Skill Use'");

                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoAttackOnAggroDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoAttackOnAggroDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Auto Target,Attack on Aggro'");

                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoPetToggleDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoPetToggleDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Enable Auto Pet Command'");

                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoPetOnSkillToggleDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoPetOnSkillToggleDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Auto Pet Command on Skill Use'");

                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoPetOnAggroDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoPetOnAggroDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Auto Pet Command on Skill Use'");

                }
                if (ErenshorQoLMod.context.Config.ContainsKey(autoPetOnAutoAttackToggleDefinition))
                {
                    ErenshorQoLMod.context.Config.Remove(autoPetOnAutoAttackToggleDefinition);
                    ErenshorUtilitiesLogger.LogInfo("Removed obsolete setting: 'Auto Pet Command on Auto Attack'");

                }

                //ensure the config change isn't repeated every time
                ErenshorQoLMod.context.appliedConfigChange = true;
            }
        }
    }
}
