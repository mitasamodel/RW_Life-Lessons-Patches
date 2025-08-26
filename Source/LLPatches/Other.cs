using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace LLPatches
{
	public static class Other
	{

		internal static IEnumerable<CEAmmoTemplate> OrderTemplates(this IEnumerable<CEAmmoTemplate> templates)
		{
			return templates
				.OrderBy(t => (string.IsNullOrEmpty(t.Prefix) && string.IsNullOrEmpty(t.Suffix)) ? 0 : 1)       // Empty goes up (newly added).
				.ThenByDescending(t => t.Prefix?.Length ?? -1)                // Then, the longest Prefixes.
				.ThenByDescending(t => t.Suffix?.Length ?? -1);                // Then, the longest Suffixes.
		}

		/// <summary>
		/// Gets all CE ammo recipes which don't have "recipeMaker" -> Non-auto-generated recipes. For auto-generated recipes the "item" should be modified instead.
		/// </summary>
		/// <returns>List of non-auto-generated recipes' defs.</returns>
		internal static List<RecipeDef> GetAllAmmoRecipes()
		{
			var ammoRecipesDefs = DefDatabase<RecipeDef>.AllDefsListForReading
				.Where(recipe => recipe.products
					.Any(prod =>
						(IsCEAmmo(prod.thingDef) &&
						prod.thingDef.recipeMaker == null)      //non-auto-generated recipe
					)
				)
				.ToList();
			//Logger.DebugLog($"ammoRecipesDefs: {ammoRecipesDefs.Count}");
			return ammoRecipesDefs;
		}

		/// <summary>
		/// Checks if the "thing" is an CE-ammo
		/// </summary>
		/// <param name="thing"></param>
		/// <returns></returns>
		public static bool IsCEAmmo(ThingDef thing)
		{
			if (thing == null)
				return false;

			//<thingClass>CombatExtended.AmmoThing</thingClass>
			if (thing.thingClass?.FullName == "CombatExtended.AmmoThing")
				return true;

			//<tradeTags>
			//	<li>CE_Ammo</li>
			//</tradeTags>
			if (thing.tradeTags?.Contains("CE_Ammo") ?? false)
				return true;

			return false;
		}
	}
}
