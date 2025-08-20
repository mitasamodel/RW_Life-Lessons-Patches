using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using LudeonTK;

namespace LLPatches
{
	public class LLPatchesMod : Mod
	{
		public static LLPatchesSettings settings;

		// Tabs
		private enum SettingsTab { Nothing, CEAmmoMain, CEAmmoTemplates };
		private SettingsTab _currentTab;
		private readonly List<TabRecord> _tabs;

		[TweakValue("0_SET", 0f, 50f)]
		static float tabShift = 25f;
		[TweakValue("0_SET", 0f, 50f)]
		static float contractBy = 12f;

		private bool _manualPrev;

		public LLPatchesMod(ModContentPack content) : base(content)
		{
			settings = GetSettings<LLPatchesSettings>();
			_tabs = new List<TabRecord>();
			_manualPrev = settings.patchCEAmmo_Manual;

			if (ModsConfig.IsActive("CETeam.CombatExtended"))
			{
				_tabs.Add(new TabRecord("CE Ammo", () => _currentTab = SettingsTab.CEAmmoMain, () => _currentTab == SettingsTab.CEAmmoMain));
				_currentTab = SettingsTab.CEAmmoMain;
				if (settings.patchCEAmmo_Manual)
					_tabs.Insert((int)(SettingsTab.CEAmmoTemplates - 1), new TabRecord("CE Ammo Templates", () => _currentTab = SettingsTab.CEAmmoTemplates, () => _currentTab == SettingsTab.CEAmmoTemplates));
			}
		}

		public override string SettingsCategory() => "Life Lessons: Patches";

		public override void DoSettingsWindowContents(Rect inRect)
		{
			inRect.y += tabShift;
			inRect.height -= tabShift;
			Widgets.DrawMenuSection(inRect);
			TabDrawer.DrawTabs(inRect, _tabs);

			Rect contentRect = inRect.ContractedBy(contractBy);
			switch (_currentTab)
			{
				case SettingsTab.CEAmmoMain:
					DrawCEAmmoMainTab(contentRect);
					break;
				case SettingsTab.CEAmmoTemplates:
					DrawCEAmmoTemplatesTab(contentRect);
					break;
				default:
					Widgets.Label(inRect, "Nothing is here for this mod combinations...");
					break;
			}
		}

		private void DrawCEAmmoTemplatesTab(Rect inRect)
		{
			Listing_Standard listing = new Listing_Standard();
			listing.Begin(inRect);
			foreach (var key in settings.Values.Keys.ToList().OrderByDescending(k => k.Length))
				settings.Values[key] = Utils_GUI.LabeledTextField(listing, key, settings.Values[key]);
			listing.End();
		}

		private void DrawCEAmmoMainTab(Rect inRect)
		{
			Listing_Standard listing = new Listing_Standard();
			Rect listingRect = inRect;
			listingRect.height -= Utils_GUI.resetButtonAreaHeight;
			listing.Begin(listingRect);
			listing.LabelCentered("Combat Extended Ammo Patch Options");
			listing.CheckboxLabeled("Patch unpatched CE ammo", ref settings.patchUnpatchedCEAmmo,
				"Automatically apply proficiency requirements for crafting CE ammo if it is has none.\n\n" +
				"Patch applied during startup or by manually clicking the button below.\n" +
				"To disable it, uncheck the option and restart the game.\n\n" +
				"Patch does NOT overwrite any files, it is safe to apply it, test it and then disable if not needed.");
			listing.CheckboxLabeled("Log ammo without template", ref settings.patchCEAmmo_LogUnpatched,
				"Outputs the list of CE ammo, for which no template has been found.\n\n" +
				"Location: " + @Environment.CurrentDirectory + @"\Mods\LLPatches.log");
			listing.CheckboxLabeled("Advanced", ref settings.patchCEAmmo_Manual, "Check the templates or set them manually.\n" +
				"Disabling this option will set values to their DEFAULTs.\n\n" +
				"Default templates location:\nContent\\Combat Extended\\Defs\\Combat Extended\\Templates_Recipies_Ammo.xml."
				);

			// Setting changed
			if (_manualPrev != settings.patchCEAmmo_Manual)
			{
				_manualPrev = settings.patchCEAmmo_Manual;
				if (!settings.patchCEAmmo_Manual)
				{
					settings.CEAmmoToDefaults();
					_tabs.RemoveAll(t => t.label.Equals("CE Ammo Templates"));
				}
				else
				{
					_tabs.Insert((int)(SettingsTab.CEAmmoTemplates - 1), new TabRecord("CE Ammo Templates", () => _currentTab = SettingsTab.CEAmmoTemplates, () => _currentTab == SettingsTab.CEAmmoTemplates));
				}
			}

			listing.CheckboxLabeled("Verbose logging", ref settings.patchCEAmmo_Logging, "Log all operations to file.\n\n" +
				"Location:" + @Environment.CurrentDirectory + @"\Mods\LLPatches.log");

			if (settings.patchCEAmmo_Manual)
			{
				listing.CheckboxLabeled("Force re-patch all CE ammo", ref settings.patchCEAmmo_ForceRemoveExisting,
						"Recommended: disabled. If patch already exists it will be removed and replaced by this automatic patch.\n\n" +
						"Should be used only for debug actions.\n" +
						"It is however SAFE, no files will be overwritten.");
			}

			if (listing.ButtonText("Patch CE ammo now"))
				if (settings.patchUnpatchedCEAmmo)
				{
					LLPatches.ProcessCEAmmoRecipes();

					// Play a sound
					SoundDefOf.Click.PlayOneShotOnCamera();

					// Show a notification
					Messages.Message("Patch for CE ammo applied!", MessageTypeDefOf.PositiveEvent);
				}

			listing.End();

			// Reset to defaults button
			if (Utils_GUI.ResetButton(inRect, "Reset to defaults"))
				settings.ResetToDefaults();
		}
	}
}
