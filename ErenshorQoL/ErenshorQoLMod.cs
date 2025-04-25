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
using Unity;
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
		internal const string ModVersion = "1.4.18.0000"; //const so should be manually updated before release
		internal const string ModTitle = "Erenshor Quality of Life Mods";
		internal const string ModDescription = "Erenshor Quality of Life Mods";
		internal const string Author = "Brumdail";
		private const string ModGUID = Author + "." + ModName;
		private static string ConfigFileName = ModGUID + ".cfg";
		private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
		public bool appliedConfigChange = false; //used to check for options to change on each launch
		//internal static readonly int windowId = 777001; //will be used later for identifying the mod's window
		internal static ErenshorQoLMod context = null!;

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

			var harmony = new Harmony(ModGUID);
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

			}
			catch
			{
				Logger.LogError($"There was an issue loading your {ConfigFileName}");
				Logger.LogError("Please check your config entries for spelling and format!");
			}
		}


		#region ConfigOptions

		internal static ConfigEntry<Toggle> AutoLootToggle = null!;
		internal static ConfigEntry<Toggle> AutoLootDebug = null!;
		internal static ConfigEntry<float> AutoLootDistance = null!;
		internal static ConfigEntry<int> AutoLootMinimum = null!;
		internal static ConfigEntry<Toggle> QoLCommandsToggle = null!;
		internal static ConfigEntry<Toggle> AutoAttackToggle = null!;
		internal static ConfigEntry<Toggle> AutoAttackOnSkillToggle = null!;
		internal static ConfigEntry<Toggle> AutoAttackOnAggro = null!;
		//internal static ConfigEntry<Toggle> AutoAttackOnSpellToggle = null!;
		//internal static ConfigEntry<Toggle> AutoAttackOnGroupAttackToggle = null!;
		//internal static ConfigEntry<Toggle> AutoAttackOnPetAttackToggle = null!;
		internal static ConfigEntry<Toggle> AutoPetToggle = null!;
		internal static ConfigEntry<Toggle> AutoPetOnSkillToggle = null!;
		internal static ConfigEntry<Toggle> AutoPetOnAggro = null!;
		//internal static ConfigEntry<Toggle> AutoPetOnSpellToggle = null!;
		//internal static ConfigEntry<Toggle> AutoPetOnGroupAttackToggle = null!;
		internal static ConfigEntry<Toggle> AutoPetOnAutoAttackToggle = null!;
		//internal static ConfigEntry<Toggle> AutoGroupAttackToggle = null!;
		//internal static ConfigEntry<Toggle> AutoGroupAttackOnSkillToggle = null!;
		//internal static ConfigEntry<Toggle> AutoGroupAttackOnSpellToggle = null!;
		//internal static ConfigEntry<Toggle> AutoGroupAttackOnPetAttackToggle = null!;
		//internal static ConfigEntry<Toggle> AutoGroupAttackOnAutoAttackToggle = null!;
		internal static ConfigEntry<Toggle> AutoPriceItem = null!;

		//removed ConfigEntries:
		internal static ConfigEntry<Toggle> AutoLootToBankToggle = null!;

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


		/// <summary>
		/// Automatically sends the pet if it's not attacking your target
		/// </summary>
		public class AutoPet
		{
			public static void AutoSendPet(string activatedFrom)
			{
				var pet = GameData.PlayerControl.Myself.MyCharmedNPC;

				if (pet == null)
				{
					// no need for further processing if we don't actually have a pet
					return;
				}

				var playerTarget = GameData.PlayerControl.CurrentTarget;
				var petTarget = pet.CurrentAggroTarget;

				if (
					//ignore if already have a target or target is non-hostile or is a rock or invulnerable...
					   playerTarget != null
					&& playerTarget != pet
					&& !IsSimInPlayerGroup(playerTarget)
					&& !playerTarget.MiningNode
					&& !playerTarget.Invulnerable
					&& playerTarget.AggressiveTowards.Contains(GameData.PlayerControl.Myself.MyFaction)
					&& petTarget != playerTarget
				)
				{
					pet.AggroOn(playerTarget);
				}
			}

			private static bool IsSimInPlayerGroup(Character character) {
				return character == GameData.GroupMember1?.MyAvatar?.MyStats?.Myself
					|| character == GameData.GroupMember2?.MyAvatar?.MyStats?.Myself
					|| character == GameData.GroupMember3?.MyAvatar?.MyStats?.Myself;
			}

			public static void AutoBackoffPet(string activatedFrom)
			{
				var pet = GameData.PlayerControl.Myself.MyCharmedNPC;

				if ((pet != null) && (pet.CurrentAggroTarget != null))
				{
					pet.CurrentAggroTarget = null;
					UpdateSocialLog.LogAdd($"{pet.NPCName} says: backing off...");
				}
			}
		}


		public class AutoAttack
		/// <summary>
		/// Automatically starts auto attack if it isn't on
		/// </summary>
		{
			public static void EnableAutoAttack(string activatedFrom)
			{
				bool autoAttackDebug = false;

				if (autoAttackDebug) { UpdateSocialLog.LogAdd($"Auto-Attack On " + activatedFrom + " - GameData.Autoattacking is: {GameData.Autoattacking}", "lightblue"); }

				var player = GameData.PlayerControl;

				if (   player.CurrentTarget != null
					&& player.CurrentTarget.AggressiveTowards.Contains(player.Myself.MyFaction)
					&& !player.CurrentTarget.MiningNode
					&& !player.CurrentTarget.Invulnerable)
				{
					// Find the PlayerCombat component
					PlayerCombat playerCombat = player.Myself.GetComponent<PlayerCombat>();

					if (playerCombat != null)
					{
						// Use AccessTools reflection to get the private method info
						MethodInfo toggleAttackMethod = AccessTools.Method(typeof(PlayerCombat), "ToggleAttack");

						// Invoke the private method
						toggleAttackMethod?.Invoke(playerCombat, null);
						if (autoAttackDebug) { UpdateSocialLog.LogAdd($"Activated Auto-Attack On " + activatedFrom + " - GameData.Autoattacking is: {GameData.Autoattacking}", "orange"); }
					}
				}
			}
		}


		/*
		public class AutoGroupCommand
		/// <summary>
		/// Automatically command the group to attack if they are not attacking your target
		/// </summary>
		{
			public static void AutoCommandAttack(string activatedFrom)
			{
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
			}
		}*/


		/// <summary>
		/// Attempts to find the latest nearby corpse to loot after each Character.DoDeath call
		/// </summary>
		[HarmonyPatch(typeof(Character), "DoDeath")]
		[HarmonyPriority(50000)]
		[HarmonyAfter("Brumdail.ErenshorREL")]
		public class AutoLoot
		{
			static void Postfix(Character __instance)
			{
				if ((ErenshorQoLMod.AutoLootToggle.Value == Toggle.On) && (__instance != null) && (__instance.isNPC) && (__instance.MyNPC != null) && (GameData.PlayerControl.Myself.Alive))
				{
					bool autoLootDebug = false;
					float autoLootDistance = ErenshorQoLMod.AutoLootDistance.Value;
					if (autoLootDistance < 0) { autoLootDistance = 0; }
					float npcDistance = Vector3.Distance(GameData.PlayerControl.transform.position, __instance.MyNPC.transform.position);
					if (npcDistance < autoLootDistance)
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

		/// <summary>
		/// Adds new /commands to the game: /bank, /vendor, /auction and updates /help to include gm commands
		/// </summary>
		[HarmonyPatch(typeof(TypeText), "CheckCommands")]
		public class QoLCommands
		{
			static void HelpMods()
			{
				UpdateSocialLog.LogAdd("QoL Modded commands: ", "lightblue");
				UpdateSocialLog.LogAdd("/autoloot - Toggles the feature to automatically Loot All items from the nearest corpse each time a creature dies.", "lightblue");
				UpdateSocialLog.LogAdd("/bank - Opens the bank window", "lightblue");
				UpdateSocialLog.LogAdd("/forge - Opens the forge (blacksmithing) window", "lightblue");
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

			static void ResetTextInput()
			{
				GameData.TextInput.typed.text = "";
				GameData.TextInput.CDFrames = 10f;
				GameData.TextInput.InputBox.SetActive(false);
				GameData.PlayerTyping = false;
			}

			static bool Prefix()
			{
				if (ErenshorQoLMod.QoLCommandsToggle.Value == Toggle.On)
				{
					bool bankEnabled = true;
					bool forgeEnabled = true;
					bool vendorEnabled = false;
					bool auctionEnabled = true;
					bool allSceneEnabled = true;
					bool helpGMEnabled = true;

					string text = GameData.TextInput.typed.text.ToLower();
					switch (text)
					{
						case var _ when text.StartsWith("/help gm") && (helpGMEnabled):
							HelpGM();
							break;
						case var _ when text.StartsWith("/help mods"):
							HelpMods();
							break;
						case var _ when text.StartsWith("/help other"):
							HelpOther();
							break;
						case var _ when text.StartsWith("/help player"):
							HelpPlayer();
							break;
						case var _ when text.StartsWith("/help"):
							HelpGM();
							HelpPlayer();
							HelpOther();
							HelpMods();

							UpdateSocialLog.LogAdd("\nUse /help mods, /help gm, /help player, or /help other for the individual lists,", "orange");
							break;


						case var _ when text.StartsWith("/autoloot"):
							if (ErenshorQoLMod.AutoLootToggle.Value == Toggle.Off)
							{
								ErenshorQoLMod.AutoLootToggle.Value = Toggle.On;
							}
							else if (ErenshorQoLMod.AutoLootToggle.Value == Toggle.On)
							{
								ErenshorQoLMod.AutoLootToggle.Value = Toggle.Off;
							}
							UpdateSocialLog.LogAdd("AutoLoot: " + ErenshorQoLMod.AutoLootToggle.Value.ToString(), "orange");
							break;

						case var _ when text.StartsWith("/bank") && bankEnabled:
							if (GameData.ItemOnCursor == null || GameData.ItemOnCursor == GameData.PlayerInv.Empty)
							{
								GameData.BankUI.OpenBank(GameData.PlayerControl.transform.position);
							}
							else
							{
								UpdateSocialLog.LogAdd("Remove item from cursor before interacting with a vendor.", "yellow");
							}

							break;

						case var _ when text.StartsWith("/forge") && forgeEnabled:
							if (GameData.ItemOnCursor == null || GameData.ItemOnCursor == GameData.PlayerInv.Empty)
							{
								GameData.PlayerAud.PlayOneShot(GameData.Misc.SmithingOpen, GameData.PlayerAud.volume * GameData.SFXVol);
								GameData.Smithing.OpenWindow(GameData.PlayerControl.transform.position);

								//TODO: Can we instantiate a forge particle effect?
								// Spawn particle effects at player's position
								//GameData.Smithing.Success = raycastHit.transform.GetComponent<ForgeEffect>().Success;
								//ParticleSystem forgeParticles = Instantiate(forgeEffectPrefab, base.transform.position, Quaternion.identity);
								//forgeParticles.Play();

							}
							else
							{
								UpdateSocialLog.LogAdd("Remove item from cursor before interacting with a vendor.", "yellow");
							}

							break;

						case var _ when text.StartsWith("/vendor") && vendorEnabled:

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
									textInput.typed.text = "";
									textInput.CDFrames = 10f;
									textInput.InputBox.SetActive(false);
									GameData.PlayerTyping = false;
									return false;
								}

								//GameData.VendorWindow.LoadWindow(qolVendor.GetComponent<VendorInventory>().ItemsForSale, qolVendor.GetComponent<VendorInventory>());
							}
							else
							{
								UpdateSocialLog.LogAdd("/vendor command requires an NPC target", "yellow");
							}*/

							break;

						case var _ when text.StartsWith("/auction") && auctionEnabled && !GameData.GM.DemoBuild:
							if (GameData.ItemOnCursor == null || GameData.ItemOnCursor == GameData.PlayerInv.Empty)
							{
								GameData.AHUI.OpenAuctionHouse(GameData.PlayerControl.transform.position);
							}
							else
							{
								UpdateSocialLog.LogAdd("Remove item from cursor before interacting with a vendor.", "yellow");
							}

							break;

						case var _ when text.StartsWith("/allscenes") && allSceneEnabled:
							StringBuilder zoneNamesBuilder = new StringBuilder("Stowaway, Tutorial");

							foreach (ZoneAtlasEntry zoneAtlasEntry in ZoneAtlas.Atlas)
							{
								zoneNamesBuilder.Append(", " + zoneAtlasEntry.ZoneName);
							}

							string zoneNames = zoneNamesBuilder.ToString();
							UpdateSocialLog.LogAdd("Zone Names: " + zoneNames);

							break;

						default:
							break;
					}

					ResetTextInput();

					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Automatically turn on Auto-Attack or Automatically send your pet when you use an offensive skill
		/// </summary>
		[HarmonyPatch(typeof(UseSkill), "DoSkill")]
		public class AutoOnSkill
		{
			static void Prefix()
			{
				if (ErenshorQoLMod.AutoPetToggle.Value == Toggle.On && ErenshorQoLMod.AutoPetOnSkillToggle.Value == Toggle.On)
				{
					AutoPet.AutoSendPet("Skill");
				}
				if (ErenshorQoLMod.AutoAttackToggle.Value == Toggle.On && ErenshorQoLMod.AutoAttackOnSkillToggle.Value == Toggle.On)
				{
					AutoAttack.EnableAutoAttack("Skill");
				}

			}
		}


		/// <summary>
		/// Automatically turn on Auto-Attack or Automatically send your pet when your group aggros an enemy
		/// </summary>
		[HarmonyPatch(typeof(NPC), "AggroOn")]
		public class AutoOnAggro
		{
			static void Postfix()
			{
				if ((ErenshorQoLMod.AutoPetToggle.Value == Toggle.On) || (ErenshorQoLMod.AutoAttackToggle.Value == Toggle.On)
					&& GameData.AttackingPlayer != null && GameData.AttackingPlayer.Count > 0 && GameData.PlayerControl.CurrentTarget == null)
				{
					NPC attackingNPC = GameData.AttackingPlayer.First();
					FieldInfo fieldInfo = AccessTools.Field(typeof(NPC), "Myself");
					Character attackingCharacter = (Character)fieldInfo.GetValue(attackingNPC);
					if ((attackingCharacter != null) && (attackingCharacter.Alive) && (GameData.PlayerControl.CurrentTarget == null))
					{
						attackingCharacter.TargetMe();
						GameData.PlayerControl.CurrentTarget = attackingCharacter;
					}
				}

				if (ErenshorQoLMod.AutoPetToggle.Value == Toggle.On && GameData.PlayerControl.CurrentTarget != null && ErenshorQoLMod.AutoPetOnAggro.Value == Toggle.On)
				{
					AutoPet.AutoSendPet("Aggro");
				}

				if (ErenshorQoLMod.AutoAttackToggle.Value == Toggle.On && GameData.PlayerControl.CurrentTarget != null && ErenshorQoLMod.AutoAttackOnAggro.Value == Toggle.On)
				{
					AutoAttack.EnableAutoAttack("Aggro");
				}
			}
		}

		/// <summary>
		/// Automatically perform actions when Auto-Attack is used
		/// </summary>
		[HarmonyPatch(typeof(PlayerCombat), "ToggleAttack")]
		class AutoOnAutoattack
		{
			static void Postfix()
			{
				if (ErenshorQoLMod.AutoPetToggle.Value == Toggle.On && ErenshorQoLMod.AutoPetOnSkillToggle.Value == Toggle.On)
				{
					if (GameData.Autoattacking == true)
					{
						AutoPet.AutoSendPet("AutoAttack");
					}
					else
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

		/// <summary>
		/// Automatically set the maximum gold value for an item that will sell
		/// </summary>
		[HarmonyPatch(typeof(AuctionHouseUI), "OpenListItem")]
		public class AutoPriceYourItem
		{
			static void Postfix(AuctionHouseUI __instance)
			{
				if (ErenshorQoLMod.AutoPriceItem.Value == Toggle.On)
				{
					int maxAHPrice = GameData.SlotToBeListed.MyItem.ItemValue * 6 - 1;
					GameData.AHUI.ListPrice.text = $"{maxAHPrice}";
				}
			}
		}

		[HarmonyPatch(typeof(LootWindow), "LootAll")]
		public static class LootWindowPatch
		{
			public static bool Prefix(LootWindow __instance)
			{
				foreach (ItemIcon itemIcon in __instance.LootSlots)
				{
					if (itemIcon.MyItem != GameData.PlayerInv.Empty)
					{
						if (itemIcon.MyItem.ItemValue >= ErenshorQoLMod.AutoLootMinimum.Value)
						{
							//only loot if at least minimum value
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

							if (addedToInventory)
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
								itemIcon.InformGroupOfLoot(itemIcon.MyItem);
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
