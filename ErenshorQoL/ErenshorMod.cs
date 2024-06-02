using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ErenshorQoL
{
    [BepInPlugin("erenshorqol.ErenshorMod", "Erenshor Quality of Life Modpack", "1.0.0")]
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
        class AutoLoot
        {
            /// <summary>
            /// Attempts to find the latest nearby corpse to loot after each Character.DoDeath call
            /// </summary>
            static void Postfix()
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
}
