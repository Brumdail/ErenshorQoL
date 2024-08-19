using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BepInEx.Configuration;
using UnityEngine;

namespace ErenshorQoL
{
    public class Utilities
    {
        internal static void ApplyConfig()
        {

            /*
            string[]? split = AzuClockPlugin.ClockLocationString.Value.Split(',');
            AzuClockPlugin._clockPosition = new Vector2(
                split[0].Trim().EndsWith("%")
                    ? float.Parse(split[0].Trim().Substring(0, split[0].Trim().Length - 1)) / 100f * Screen.width
                    : float.Parse(split[0].Trim()),
                split[1].Trim().EndsWith("%")
                    ? float.Parse(split[1].Trim().Substring(0, split[1].Trim().Length - 1)) / 100f * Screen.height
                    : float.Parse(split[1].Trim()));

            AzuClockPlugin._windowRect = new Rect(AzuClockPlugin._clockPosition, new Vector2(1000, 100));

            if (AzuClockPlugin.ClockUseOSFont.Value == AzuClockPlugin.Toggle.On)
            {
                AzuClockPlugin._clockFont = Font.CreateDynamicFontFromOSFont(AzuClockPlugin.ClockFontName.Value,
                    AzuClockPlugin.ClockFontSize.Value);
            }
            else
            {
                AzuClockPlugin.AzuClockLogger.LogDebug("Getting fonts");
                Font[]? fonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (Font? font in fonts)
                    if (font.name == AzuClockPlugin.ClockFontName.Value)
                    {
                        AzuClockPlugin._clockFont = font;
                        AzuClockPlugin.AzuClockLogger.LogDebug($"Got font {font.name}");
                        break;
                    }
            }

            AzuClockPlugin.Style = new GUIStyle
            {
                richText = true,
                fontSize = AzuClockPlugin.ClockFontSize.Value,
                alignment = AzuClockPlugin.ClockTextAlignment.Value,
                font = AzuClockPlugin._clockFont
            };
            AzuClockPlugin.Style2 = new GUIStyle
            {
                richText = true,
                fontSize = AzuClockPlugin.ClockFontSize.Value,
                alignment = AzuClockPlugin.ClockTextAlignment.Value,
                font = AzuClockPlugin._clockFont
            };

            AzuClockPlugin._configApplied = true;
            */
            ErenshorQoLMod._configApplied = true;
        }

        /*
        public static bool IgnoreKeyPresses(bool extra = false)
        {
            if (!extra)
                return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() ||
                       Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() ||
                       Chat.instance?.HasFocus() == true;
            return ZNetScene.instance == null || Player.m_localPlayer == null || Minimap.IsOpen() ||
                   Console.IsVisible() || TextInput.IsVisible() || ZNet.instance.InPasswordDialog() ||
                   Chat.instance?.HasFocus() == true || StoreGui.IsVisible() || InventoryGui.IsVisible() ||
                   Menu.IsVisible() || TextViewer.instance?.IsVisible() == true;
        }
        */

        internal static void GenerateConfigs()
        {
            ErenshorQoLMod.AutoLootToggle = ErenshorQoLMod.context.config("1 - AutoLoot", "Enable AutoLoot", ErenshorQoLMod.Toggle.On, "Enable automatic looting within the LootDistance?");
            ErenshorQoLMod.AutoLootDistance = ErenshorQoLMod.context.config("1 - AutoLoot", "Auto Loot Distance", 30f, "Distance to automatically loot items from the ground.");
            //ErenshorQoLMod.AutoLootDistance = ErenshorQoLMod.context.config("1 - General", "Auto Loot Distance", 30f, "Distance to automatically loot items from the ground.", new AcceptableValueRange<float>(5f, 10000f));
            ErenshorQoLMod.QoLCommandsToggle = ErenshorQoLMod.context.config("2 - QoL Commands", "Enable QoL Commands", ErenshorQoLMod.Toggle.On, "Enable additional Quality of Life commands like /bank, /sell, /auction, updated /help etc.");
            ErenshorQoLMod.AutoAttackToggle = ErenshorQoLMod.context.config("3 - Auto Attack", "Enable Auto Attack", ErenshorQoLMod.Toggle.On, "Enable automatically turning on auto-attack.");
            ErenshorQoLMod.AutoAttackOnSkillToggle = ErenshorQoLMod.context.config("3 - Auto Attack", "Auto Attack on Skill Use", ErenshorQoLMod.Toggle.On, "Automatically start attacking when a skill is used.");
            ErenshorQoLMod.AutoAttackOnSpellToggle = ErenshorQoLMod.context.config("3 - Auto Attack", "Auto Attack on Spell Cast", ErenshorQoLMod.Toggle.On, "Automatically start attacking when a spell is cast.");
            ErenshorQoLMod.AutoAttackOnGroupAttackToggle = ErenshorQoLMod.context.config("3 - Auto Attack", "Auto Attack on Group Attack Command", ErenshorQoLMod.Toggle.On, "Automatically start attacking when a group attack command is issued.");
            ErenshorQoLMod.AutoAttackOnPetAttackToggle = ErenshorQoLMod.context.config("3 - Auto Attack", "Auto Attack on Pet Attack Command", ErenshorQoLMod.Toggle.On, "Automatically start attacking when a pet attack command is issued.");
            ErenshorQoLMod.AutoPetToggle = ErenshorQoLMod.context.config("4 - Auto Pet", "Enable Auto Pet Command", ErenshorQoLMod.Toggle.On, "Enable automatically commanding the pet to attack.");
            ErenshorQoLMod.AutoPetOnSkillToggle = ErenshorQoLMod.context.config("4 - Auto Pet", "Auto Pet Command on Skill Use", ErenshorQoLMod.Toggle.On, "Automatically command the pet to attack when a skill is used.");
            ErenshorQoLMod.AutoPetOnSpellToggle = ErenshorQoLMod.context.config("4 - Auto Pet", "Auto Pet Command on Spell Cast", ErenshorQoLMod.Toggle.On, "Automatically command the pet to attack when a spell is cast.");
            ErenshorQoLMod.AutoPetOnGroupAttackToggle = ErenshorQoLMod.context.config("4 - Auto Pet", "Auto Pet Command on Group Attack", ErenshorQoLMod.Toggle.On, "Automatically command the pet to attack when a group attack command is issued.");
            ErenshorQoLMod.AutoPetOnAutoAttackToggle = ErenshorQoLMod.context.config("4 - Auto Pet", "Auto Pet Command on Auto Attack", ErenshorQoLMod.Toggle.On, "Automatically command the pet to attack when auto-attack is enabled.");
            ErenshorQoLMod.AutoGroupAttackToggle = ErenshorQoLMod.context.config("5 - Auto Group Attack", "Enable Auto Group Attack Command", ErenshorQoLMod.Toggle.Off, "Enable automatically commanding the group to attack.");
            ErenshorQoLMod.AutoGroupAttackOnSkillToggle = ErenshorQoLMod.context.config("5 - Auto Group Attack", "Auto Group Attack on Skill Use", ErenshorQoLMod.Toggle.On, "Automatically command the group to attack when a skill is used.");
            ErenshorQoLMod.AutoGroupAttackOnSpellToggle = ErenshorQoLMod.context.config("5 - Auto Group Attack", "Auto Group Attack on Spell Cast", ErenshorQoLMod.Toggle.On, "Automatically command the group to attack when a spell is cast.");
            ErenshorQoLMod.AutoGroupAttackOnPetAttackToggle = ErenshorQoLMod.context.config("5 - Auto Group Attack", "Auto Group Attack on Pet Attack Command", ErenshorQoLMod.Toggle.On, "Automatically command the group to attack when a pet attack command is issued.");
            ErenshorQoLMod.AutoGroupAttackOnAutoAttackToggle = ErenshorQoLMod.context.config("5 - Auto Group Attack", "Auto Group Attack on Auto Attack", ErenshorQoLMod.Toggle.On, "Automatically command the group to attack when auto-attack is enabled.");
            ErenshorQoLMod.AutoRunToggle = ErenshorQoLMod.context.config("6 - Auto Run", "Enable Auto Run", ErenshorQoLMod.Toggle.On, "Automatically run while enabled");
            ErenshorQoLMod.AutoRunKey = ErenshorQoLMod.context.config("6 - Auto Run", "Auto Run Key", new KeyboardShortcut(KeyCode.Break), new ConfigDescription("Key(s) used to toggle Auto Run. Use https://docs.unity3d.com/Manual/ConventionalGameInput.html", new ErenshorQoLMod.AcceptableShortcuts()));
        }
    }
}
