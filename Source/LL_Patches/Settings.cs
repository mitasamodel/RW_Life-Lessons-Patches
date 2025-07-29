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

namespace LLPatches
{
	public class LLPatchesSettings : ModSettings
	{
		public const bool patchCEAmmo_Default = true;
		public const bool patchCEAmmo_Manual_Default = false;
		public const bool patchCEAmmo_ForceRemoveExisting_Default = false;
		public const bool patchCEAmmo_Logging_Default = false;
		public const bool patchCEAmmo_LogUnpatched_Default = false;

		private static readonly Dictionary<string, string> CEAmmoDefaultValues = new Dictionary<string, string>
		{
			{ "_AP", "RecipeAmmoC1" },
			{ "_HP", "RecipeAmmoC1" },
			{ "_FMJ", "RecipeAmmoC1" },
			{ "_Buck", "RecipeAmmoC1" },
			{ "_Slug", "RecipeAmmoC1" },
			{ "_Beanbag", "RecipeAmmoC1" },
			{ "_Sabot", "RecipeAmmoC2" },
			{ "_HE", "RecipeAmmoHEC1" },
			{ "_Incendiary", "RecipeAmmoINC1" },
			{ "_ElectroSlug", "RecipeAmmoEMPC1" },
			{ "Charged", "RecipeAmmoChargedC1" },
			{ "Charged_AP", "RecipeAmmoChargedC2" },
			{ "Charged_Slug", "RecipeAmmoChargedC2" },
			{ "Charged_Ion", "RecipeAmmoChargedC1" },
		};

		public bool patchUnpatchedCEAmmo = patchCEAmmo_Default;
		public bool patchCEAmmo_Manual = patchCEAmmo_Manual_Default;
		public bool patchCEAmmo_ForceRemoveExisting = patchCEAmmo_ForceRemoveExisting_Default;
		public bool patchCEAmmo_Logging = patchCEAmmo_Logging_Default;
		public bool patchCEAmmo_LogUnpatched = patchCEAmmo_LogUnpatched_Default;

		public Dictionary<string, string> Values = new Dictionary<string, string>(CEAmmoDefaultValues);

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref patchUnpatchedCEAmmo, "patchUnpatchedCEAmmo", patchCEAmmo_Default);
			Scribe_Values.Look(ref patchCEAmmo_Manual, "patchCEAmmo_Manual", patchCEAmmo_Manual_Default);
			Scribe_Values.Look(ref patchCEAmmo_ForceRemoveExisting, "patchCEAmmo_ForceRemoveExisting", patchCEAmmo_ForceRemoveExisting_Default);
			Scribe_Values.Look(ref patchCEAmmo_Logging, "patchCEAmmo_Logging", patchCEAmmo_Logging_Default);
			Scribe_Values.Look(ref patchCEAmmo_LogUnpatched, "patchCEAmmo_LogUnpatched", patchCEAmmo_LogUnpatched_Default);

			foreach (var key in CEAmmoDefaultValues.Keys.ToList())
			{
				string value = Values.ContainsKey(key) ? Values[key] : CEAmmoDefaultValues[key];
				Scribe_Values.Look(ref value, "patchCEAmmo" + key, CEAmmoDefaultValues[key]);
				Values[key] = value;
			}
		}
		public void ResetToDefaults()
		{
			patchUnpatchedCEAmmo = patchCEAmmo_Default;
			patchCEAmmo_Manual = patchCEAmmo_Manual_Default;
			patchCEAmmo_ForceRemoveExisting = patchCEAmmo_ForceRemoveExisting_Default;
			patchCEAmmo_Logging = patchCEAmmo_Logging_Default;
			patchCEAmmo_LogUnpatched = patchCEAmmo_LogUnpatched_Default;
			Values = new Dictionary<string, string>(CEAmmoDefaultValues);
		}
	}

	public class LLPatchesMod : Mod
	{
		public static LLPatchesSettings settings;

		public LLPatchesMod(ModContentPack content) : base(content)
		{
			settings = GetSettings<LLPatchesSettings>();
		}

		public override string SettingsCategory()
		{
			return "Life Lessons: Patches";
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listing = new Listing_Standard();
			listing.Begin(inRect);

			//***** CE Ammo Section

			int checkboxes = 5;
			int controls = settings.Values.Count;
			float lineHeight = 22f;

			int buttons = 1;
			float buttonHeigt = 30f;

			float paddingSize = 6f;

			float groupHeight = checkboxes * 2 + (checkboxes + controls) * lineHeight + buttons * buttonHeigt + paddingSize * 2;
			Rect groupRect = listing.GetRect(groupHeight);            //Area
			Widgets.DrawBox(groupRect);                               //Draw border
			Rect innerRect = groupRect.ContractedBy(paddingSize);     //Padding from inside

			Listing_Standard groupListing = new Listing_Standard();
			groupListing.Begin(innerRect);
			groupListing.CheckboxLabeled("Patch unpatched CE ammo", ref settings.patchUnpatchedCEAmmo,
				"Automatically apply proficiency requirements for crafting CE ammo if it is not done by mod authors.\n\n" +
				"Patch applied during startup or by manually clicking the button below.\n" +
				"To disable, uncheck the option and restart the game.\n\n" +
				"Patch does NOT overwrite any files, it is safe to apply it, test and then disable if not needed.");
			groupListing.CheckboxLabeled("Generate list of unpatched CE ammo", ref settings.patchCEAmmo_LogUnpatched,
				"Will output the list of CE ammo, for which no template has been found.\n\n" +
				"Location:" + @Environment.CurrentDirectory + @"\Mods\LLPatches.log");
			groupListing.CheckboxLabeled("Advanced", ref settings.patchCEAmmo_Manual, "Set templates manually (you have to create them)");
			if (!settings.patchCEAmmo_Manual)
				GUI.enabled = false;
			groupListing.CheckboxLabeled("Force re-patch all CE ammo", ref settings.patchCEAmmo_ForceRemoveExisting,
				"Recommended: disabled. If patch already exist it will be removed and replaced by this automatic patch.\n\n" +
				"Should be used only for debug actions.\n" + 
				"It is however SAFE, no files will be overwritten.");
			foreach (var key in settings.Values.Keys.ToList())
				settings.Values[key] = LabeledTextField(groupListing, key, settings.Values[key]);

			GUI.enabled = true;

			groupListing.CheckboxLabeled("Verbose logging", ref settings.patchCEAmmo_Logging, "Log all operations to file.\n\n" +
				"Location:" + @Environment.CurrentDirectory + @"\Mods\LLPatches.log");

			if (groupListing.ButtonText("Patch CE ammo now"))
				if (settings.patchUnpatchedCEAmmo)
				{
					LLPatches.ProceedCEAmmoRecipes();

					// Play a sound
					SoundDefOf.Click.PlayOneShotOnCamera();

					// Show a notification
					Messages.Message("Patch for CE ammo applied!", MessageTypeDefOf.PositiveEvent);
				}
			groupListing.End();

			listing.GapLine();
			if (listing.ButtonText("Reset to defaults"))
				settings.ResetToDefaults();

			listing.End();
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
