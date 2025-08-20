using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LLPatches
{
	public class LLPatchesMod : Mod
	{
		public static LLPatchesSettings settings;

		public LLPatchesMod(ModContentPack content) : base(content)
		{
			settings = GetSettings<LLPatchesSettings>();
		}

		public override string SettingsCategory() => "Life Lessons: Patches";

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listing = new Listing_Standard();
			listing.Begin(inRect);

			//***** CE Ammo Section
			float paddingSize = 12f;
			float groupHeight = inRect.height - Utils_GUI.resetButtonAreaHeight;
			Rect groupRect = listing.GetRect(groupHeight);            //Area

			Rect innerRect = groupRect.ContractedBy(paddingSize);     //Padding from inside
			Listing_Standard groupListing = new Listing_Standard();
			groupListing.Begin(innerRect);
			groupListing.LabelCentered("Combat Extended Ammo Patch Options");
			groupListing.CheckboxLabeled("Patch unpatched CE ammo", ref settings.patchUnpatchedCEAmmo,
				"Automatically apply proficiency requirements for crafting CE ammo if it is has none.\n\n" +
				"Patch applied during startup or by manually clicking the button below.\n" +
				"To disable it, uncheck the option and restart the game.\n\n" +
				"Patch does NOT overwrite any files, it is safe to apply it, test it and then disable if not needed.");
			groupListing.CheckboxLabeled("Log ammo without template", ref settings.patchCEAmmo_LogUnpatched,
				"Outputs the list of CE ammo, for which no template has been found.\n\n" +
				"Location: " + @Environment.CurrentDirectory + @"\Mods\LLPatches.log");
			bool wasManual = settings.patchCEAmmo_Manual;
			groupListing.CheckboxLabeled("Advanced", ref settings.patchCEAmmo_Manual, "Check the templates or set them manually.\n" +
				"Disabling this option will set values to their DEFAULTs.\n\n" +
				"Default templates location:\nContent\\Combat Extended\\Defs\\Combat Extended\\Templates_Recipies_Ammo.xml."
				);
			// Changed from true to false
			if (wasManual && !settings.patchCEAmmo_Manual)
				settings.CEAmmoToDefaults();

			// Advanced: set templates manually or check them
			if (settings.patchCEAmmo_Manual)
			{
				groupListing.CheckboxLabeled("Force re-patch all CE ammo", ref settings.patchCEAmmo_ForceRemoveExisting,
					"Recommended: disabled. If patch already exists it will be removed and replaced by this automatic patch.\n\n" +
					"Should be used only for debug actions.\n" +
					"It is however SAFE, no files will be overwritten.");
				foreach (var key in settings.Values.Keys.ToList().OrderByDescending(k => k.Length))
					settings.Values[key] = LabeledTextField(groupListing, key, settings.Values[key]);
			}

			groupListing.CheckboxLabeled("Verbose logging", ref settings.patchCEAmmo_Logging, "Log all operations to file.\n\n" +
				"Location:" + @Environment.CurrentDirectory + @"\Mods\LLPatches.log");

			if (groupListing.ButtonText("Patch CE ammo now"))
				if (settings.patchUnpatchedCEAmmo)
				{
					LLPatches.ProcessCEAmmoRecipes();

					// Play a sound
					SoundDefOf.Click.PlayOneShotOnCamera();

					// Show a notification
					Messages.Message("Patch for CE ammo applied!", MessageTypeDefOf.PositiveEvent);
				}
			groupListing.End();

			groupRect.yMax = groupListing.MaxColumnHeightSeen + paddingSize * 2;
			Utils_GUI.DrawBox(groupRect, Color.grey, 1);
			listing.End();

			// Reset to defaults button
			if (Utils_GUI.ResetButton(inRect, "Reset to defaults"))
				settings.ResetToDefaults();
		}


		private string LabeledTextField(Listing_Standard listing, string label, string value, float labelWidth = 120f, float gap = 6f)
		{
			Rect row = listing.GetRect(22f);

			Rect labelRect = new Rect(row.x, row.y, labelWidth, row.height);
			Rect fieldRect = new Rect(row.x + labelWidth + gap, row.y, row.width - labelWidth - gap, row.height);

			Widgets.Label(labelRect, label);
			return Widgets.TextField(fieldRect, value ?? "");
		}
	}
}
