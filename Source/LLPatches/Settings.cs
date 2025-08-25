using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LLPatches
{
	public class LLPatchesSettings : ModSettings
	{
		public const int currentVersion = 1;
		public int settingsVersion = 0;

		public const bool patchCEAmmo_Default = true;
		public const bool patchCEAmmo_Manual_Default = false;
		public const bool patchCEAmmo_ForceRemoveExisting_Default = false;
		public const bool patchCEAmmo_Logging_Default = false;
		public const bool patchCEAmmo_LogUnpatched_Default = false;

		// Legacy. Left for automatic migration.
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
			{ "Arrow_Stone", "RecipeAmmoStoneArrowC1" },
			{ "Arrow_Steel", "RecipeAmmoMetalArrowC1" },
			{ "Arrow_Plasteel", "RecipeAmmoMetalArrowC2" },
			{ "Arrow_Venom" , "RecipeAmmoVenomArrowC1" },
			{ "Arrow_Flame", "RecipeAmmoFlameArrowC1" },
		};

		private static readonly List<CEAmmoTemplate> CEAmmoTemplatesDefault = new List<CEAmmoTemplate>
		{
			new CEAmmoTemplate("_AP", "RecipeAmmoC1"),
			new CEAmmoTemplate("_HP", "RecipeAmmoC1"),
			new CEAmmoTemplate("_FMJ", "RecipeAmmoC1"),
			new CEAmmoTemplate("_Buck", "RecipeAmmoC1"),
			new CEAmmoTemplate("_Slug", "RecipeAmmoC1"),
			new CEAmmoTemplate("_Beanbag", "RecipeAmmoC1"),
			new CEAmmoTemplate("_Sabot", "RecipeAmmoC2"),
			new CEAmmoTemplate("_HE", "RecipeAmmoHEC1"),
			new CEAmmoTemplate("_Incendiary", "RecipeAmmoINC1"),
			new CEAmmoTemplate("_ElectroSlug", "RecipeAmmoEMPC1"),
			new CEAmmoTemplate("Charged", "RecipeAmmoChargedC1"),
			new CEAmmoTemplate("Charged_AP", "RecipeAmmoChargedC2"),
			new CEAmmoTemplate("Charged_Slug", "RecipeAmmoChargedC2"),
			new CEAmmoTemplate("Charged_Ion", "RecipeAmmoChargedC1"),
			new CEAmmoTemplate("Arrow_Stone", "RecipeAmmoStoneArrowC1"),
			new CEAmmoTemplate("Arrow_Steel", "RecipeAmmoMetalArrowC1"),
			new CEAmmoTemplate("Arrow_Plasteel", "RecipeAmmoMetalArrowC2"),
			new CEAmmoTemplate("Arrow_Venom", "RecipeAmmoVenomArrowC1"),
			new CEAmmoTemplate("Arrow_Flame", "RecipeAmmoFlameArrowC1"),
		};

		public bool patchUnpatchedCEAmmo = patchCEAmmo_Default;
		public bool patchCEAmmo_Manual = patchCEAmmo_Manual_Default;
		public bool patchCEAmmo_ForceRemoveExisting = patchCEAmmo_ForceRemoveExisting_Default;
		public bool patchCEAmmo_Logging = patchCEAmmo_Logging_Default;
		public bool patchCEAmmo_LogUnpatched = patchCEAmmo_LogUnpatched_Default;

		private Dictionary<string, string> _legacyDict = new Dictionary<string, string>();
		public List<CEAmmoTemplate> CEAmmoTemplates;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref settingsVersion, "settingsVersion", 0);

			Scribe_Values.Look(ref patchUnpatchedCEAmmo, "patchUnpatchedCEAmmo", patchCEAmmo_Default);
			Scribe_Values.Look(ref patchCEAmmo_Manual, "patchCEAmmo_Manual", patchCEAmmo_Manual_Default);
			Scribe_Values.Look(ref patchCEAmmo_ForceRemoveExisting, "patchCEAmmo_ForceRemoveExisting", patchCEAmmo_ForceRemoveExisting_Default);
			Scribe_Values.Look(ref patchCEAmmo_Logging, "patchCEAmmo_Logging", patchCEAmmo_Logging_Default);
			Scribe_Values.Look(ref patchCEAmmo_LogUnpatched, "patchCEAmmo_LogUnpatched", patchCEAmmo_LogUnpatched_Default);
			Scribe_Collections.Look(ref CEAmmoTemplates, "CEAmmoTemplates", LookMode.Deep);

			// Legacy dictionary. Only load, no saving.
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (settingsVersion == 0)
				{
					foreach (var key in CEAmmoDefaultValues.Keys.ToList())
					{
						string value = _legacyDict.ContainsKey(key) ? _legacyDict[key] : CEAmmoDefaultValues[key];
						Scribe_Values.Look(ref value, "patchCEAmmo" + key, CEAmmoDefaultValues[key]);
						_legacyDict[key] = value;
					}
				}
			}

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				// After everything has been loaded, let's check if the list is actually initialized.
				// Scribe will re-write the list, so initializing it before will not help. Scribe will re-write it with 'null'.
				CEAmmoTemplates ??= new List<CEAmmoTemplate>();

				// Legacy check.
				if (settingsVersion != currentVersion)
				{
					if (settingsVersion < 1)
						Migrate_0_1();

					// Migration finished.
					settingsVersion = currentVersion;
					LLPatchesMod.forceRewriteSettings = true;
				}
			}
		}

		/// <summary>
		/// Migrate versions: "0" => "1".
		/// Version "0" stores the strings directly in settings without any additional object.
		/// Then these values are stored into Dictionary<suffix,template>.
		/// </summary>
		private void Migrate_0_1()
		{
#if DEBUG
			Logger.Log("Migrate 0->1");
#endif
			foreach (var kv in _legacyDict)
			{
				string suffix = kv.Key;
				string template = kv.Value;

				var existing = CEAmmoTemplates.FirstOrDefault(t => t.Suffix == suffix);
				if (existing != null)
				{
#if DEBUG
					Logger.Log($"Exists: {suffix}[{template}]");
#endif
					existing.Template = template;
					existing.Enable();
				}
				else
				{
#if DEBUG
					Logger.Log($"Add new: {suffix}[{template}]");
#endif
					CEAmmoTemplates.Add(new CEAmmoTemplate(suffix, template));
				}
			}
			_legacyDict = null;
		}

		/// <summary>
		/// Goes through list of templates and replaces known suffixes to defaults.
		/// Also adds default suffixes if not found.
		/// </summary>
		public void ResetCEAmmoTemplates()
		{
			//TODO
		}

		public void ResetToDefaults()
		{
			patchUnpatchedCEAmmo = patchCEAmmo_Default;
			patchCEAmmo_Manual = patchCEAmmo_Manual_Default;
			patchCEAmmo_ForceRemoveExisting = patchCEAmmo_ForceRemoveExisting_Default;
			patchCEAmmo_Logging = patchCEAmmo_Logging_Default;
			patchCEAmmo_LogUnpatched = patchCEAmmo_LogUnpatched_Default;
			//Values = new Dictionary<string, string>(CEAmmoDefaultValues);
		}

		/// <summary>
		/// Restores ammo templates to their defaults if defaults exist.
		/// </summary>
		public void CEAmmoToDefaults()
		{
			//if (!MatchesDefaults(Values, CEAmmoDefaultValues))
			//{
			//	// For the future - go through all keys and set them to default values.
			//	// TODO: Ignore if the key does not exist in defaults.
			//	Values = new Dictionary<string, string>(CEAmmoDefaultValues);
			//}
		}

		/// <summary>
		/// Compare both dictionaries with templates.
		/// Prepare to allow to add new templates from the settings window.
		/// </summary>
		/// <param name="current"></param>
		/// <param name="defaults"></param>
		/// <returns></returns>
		private static bool MatchesDefaults(
			IReadOnlyDictionary<string, string> current,
			IReadOnlyDictionary<string, string> defaults)
		{
			// Same instance (will not happen)
			//if (ReferenceEquals(current, defaults)) return true;

			// If Default has more keys than current
			if (current.Count < defaults.Count) return false;

			foreach (var kv in defaults)
			{
				// Default has a key that is not in current
				if (!current.TryGetValue(kv.Key, out var v)) return false;

				// Default value does not match current value
				if (!string.Equals(v, kv.Value)) return false;
			}
			return true;
		}
	}
}
