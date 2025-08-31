using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using LLPatches.RW_JustUtils;

namespace LLPatches
{
	public class TabContent_CEAmmoMain : ITabView
	{
		private readonly ITabHost _host;

		private bool _manualPrev;

		public TabContent_CEAmmoMain(ITabHost host)
		{
			_host = host;
			_manualPrev = LLPatchesMod.Settings.patchCEAmmo_Manual;
		}

		public void Draw(Rect inRect)
		{
			Listing_Standard listing = new Listing_Standard();
			Rect listingRect = inRect;
			listingRect.height -= Utils_GUI.resetButtonAreaHeight;
			listing.Begin(listingRect);
			listing.LabelCentered("Combat Extended Ammo Patch Options");
			listing.CheckboxLabeled("Patch unpatched CE ammo", ref LLPatchesMod.Settings.patchUnpatchedCEAmmo,
				"Automatically apply proficiency requirements for crafting CE ammo if there is none.\n\n" +
				"Patch applied during startup or by manually clicking the button below.\n" +
				"To disable it, uncheck the option and restart the game.\n\n" +
				"Patch does NOT overwrite any files, it is safe to apply it, test it and then disable if not needed.");
			listing.CheckboxLabeled("Log ammo without template", ref LLPatchesMod.Settings.patchCEAmmo_LogUnpatched,
				"Output the list of CE ammo, for which no template has been found.\n\n" +
				"Log location: " + @Environment.CurrentDirectory + @"\Mods\LLPatches.log");
			listing.CheckboxLabeled("Log incorrect templates", ref LLPatchesMod.Settings.patchCEAmmo_LogWrongTemplates,
				"Output the list of assigned to CE ammo templates, which have no template data defined.\n\n" +
				"The template must be defined in order to use it. This mod does not create templates... Yet.\n\n" + 
				"Log location: " + @Environment.CurrentDirectory + @"\Mods\LLPatches.log");
			listing.CheckboxLabeled("Advanced", ref LLPatchesMod.Settings.patchCEAmmo_Manual, "Check the templates or set them manually.\n" +
				"Disabling this option will not restore the defaults. Use the button on corrensponding tab instead.\n\n" +
				"Default templates location:\nContent\\Combat Extended\\Defs\\Combat Extended\\"
				);

			//Setting changed
			if (_manualPrev != LLPatchesMod.Settings.patchCEAmmo_Manual)
			{
				_manualPrev = LLPatchesMod.Settings.patchCEAmmo_Manual;
				//if (!LLPatchesMod.settings.patchCEAmmo_Manual)
				//	LLPatchesMod.settings.CEAmmoToDefaults();
				_host.ResetTabs();
			}

			listing.CheckboxLabeled("Verbose logging", ref LLPatchesMod.Settings.patchCEAmmo_Logging, "Log all operations to file.\n\n" +
				"Location:" + @Environment.CurrentDirectory + @"\Mods\LLPatches.log");

			if (LLPatchesMod.Settings.patchCEAmmo_Manual)
			{
				listing.CheckboxLabeled("Force re-patch all CE ammo", ref LLPatchesMod.Settings.patchCEAmmo_ForceRemoveExisting,
						"Recommended: disabled. If patch already exists it will be removed and replaced by this automatic patch.\n\n" +
						"Should be used only for debug actions.\n" +
						"It is however SAFE, no files will be overwritten.");
			}

			if (listing.ButtonText("Patch CE ammo now"))
				if (LLPatchesMod.Settings.patchUnpatchedCEAmmo)
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
				LLPatchesMod.Settings.ResetToDefaults();
		}

		public bool Enabled() => ModsConfig.IsActive("CETeam.CombatExtended");
		public string GetLabel() => "CE Ammo";
	}
}
