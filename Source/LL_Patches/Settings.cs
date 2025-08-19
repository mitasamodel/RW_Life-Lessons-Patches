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
			{ "Arrow_Stone", "RecipeAmmoStoneArrowC1" },
			{ "Arrow_Steel", "RecipeAmmoMetalArrowC1" },
			{ "Arrow_Plasteel", "RecipeAmmoMetalArrowC2" },
			{ "Arrow_Venom" , "RecipeAmmoVenomArrowC1" },
			{ "Arrow_Flame", "RecipeAmmoFlameArrowC1" },
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

		/// <summary>
		/// Restores ammo templates to their defaults if defaults exist.
		/// </summary>
		public void CEAmmoToDefaults()
		{
			if (!MatchesDefaults(Values, CEAmmoDefaultValues))
			{
				// For the future - go through all keys and set them to default values.
				// TODO: Ignore if the key does not exist in defaults.
				Values = new Dictionary<string, string>(CEAmmoDefaultValues);
			}
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
