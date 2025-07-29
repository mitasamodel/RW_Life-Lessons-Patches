using CombatExtended;
using LifeLessons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLPatches
{
	[StaticConstructorOnStartup]
	public static class LLPatches
	{
		static readonly string logFile = @Environment.CurrentDirectory + @"\Mods\LLPatches.log";

		static LLPatches()
		{
			if (LLPatchesMod.settings.patchCEAmmo_Logging || LLPatchesMod.settings.patchCEAmmo_LogUnpatched)
				File.WriteAllText(logFile, "[LLPatch] Debug start\n");    //create/rewrite file

			//CE Ammos have their separated recipes instead of auto-generated for items
			if (LLPatchesMod.settings.patchUnpatchedCEAmmo)
				ProceedCEAmmoRecipes();
		}

		public static void ProceedCEAmmoRecipes()
		{
			if (LLPatchesMod.settings.Values == null)
			{
				Log_Error($"[Life Lessons: Patches] Settings are null: {LLPatchesMod.settings.Values == null}");
				Verse.Log.Message("[Life Lessons: Patches] Please report it to mod author");
				return;
			}

			//List for ammo without appropriate template
			List<string> noTemplateRecipes = new List<string>();

			//Dictionary with k, v: k - ending of Ammo recipe, v - template to use
			Dictionary<string, string> Templates = LLPatchesMod.settings.Values;
			//Order Keys by lenght, starting from longest, so "*Charged_AP" will be checked before "*_AP"
			var keys = Templates.Keys.OrderByDescending(k => k.Length);

			foreach (RecipeDef recipe in GetAllAmmoRecipes())
			{
				if (recipe.products.NullOrEmpty())
				{
					Log_Error($"[Life Lessons: Patches] Unexpected empty products list: {recipe.defName}");
					Verse.Log.Message("[Life Lessons: Patches] Please report it to mod author");
					continue;
				}

				if (recipe.products.Count > 1)
				{
					Log_Error($"[Life Lessons: Patches] Recipe [{recipe.defName}] returns more than 1 product: {recipe.products.Count}");
					Verse.Log.Message("[Life Lessons: Patches] Please report it to mod author");
					continue;
				}

				if (LLPatchesMod.settings.patchCEAmmo_Logging)
					Log($"Recipe: {recipe.defName}. Ammo: {recipe.products[0].thingDef?.defName}");

				string templateName = null;
				foreach (string key in keys)
				{
					if (recipe.defName.EndsWith(key, StringComparison.OrdinalIgnoreCase))
					{
						templateName = Templates[key];
						if (LLPatchesMod.settings.patchCEAmmo_Logging)
							Log($"\t[Template] Key:{key} Name: {templateName}");
						break;
					}
				}

				if (string.IsNullOrEmpty(templateName))
					noTemplateRecipes.Add(recipe.defName);
				else
				{
					if (ExtensionExist(recipe))
					{
						if (LLPatchesMod.settings.patchCEAmmo_ForceRemoveExisting)
							RemoveExistingExtension(recipe);
						else
						{
							if (LLPatchesMod.settings.patchCEAmmo_Logging)
								Log($"\tBillProficiencyExtension already exists. Skipping");
							continue;
						}
					}
					recipe.AddTemplate(templateName);
				}
			}

			// Output summary if any recipes were unmatched
			if (noTemplateRecipes.Count > 0 && (LLPatchesMod.settings.patchCEAmmo_Logging || LLPatchesMod.settings.patchCEAmmo_LogUnpatched))
			{
				Log("The following ammo recipes had no matching template:");
				foreach (string recipeName in noTemplateRecipes)
					Log($"\t- {recipeName}");
			}
		}

		private static bool ExtensionExist(RecipeDef recipe)
		{
			return recipe.modExtensions?.OfType<BillProficiencyExtension>().FirstOrDefault() != null;
		}

		private static void RemoveExistingExtension(RecipeDef recipe)
		{
			//Check mod extension. If BillProficiencyExtension already set, remove it
			var extension = recipe.modExtensions?.OfType<BillProficiencyExtension>().FirstOrDefault();
			if (extension != null)
			{
				if (LLPatchesMod.settings.patchCEAmmo_Logging)
					Log($"\tBillProficiencyExtension already exists. Will remove");
				recipe.modExtensions.Remove(extension);
			}
		}

		static void AddTemplate(this RecipeDef recipe, string templateName)
		{
			var newExtension = new BillProficiencyExtension()
			{
				templateDef = DefDatabase<ThingProficiencyTemplateDef>.GetNamedSilentFail(templateName),
				hardRequirement = false
			};
			if (newExtension.templateDef == null)
				Log_Error($"[Life Lessons: Patches] Cannot find template [{templateName}]");
			else
			{
				if (recipe.modExtensions == null)
					recipe.modExtensions = new List<DefModExtension>();
				recipe.modExtensions.Add(newExtension);
			}
		}

		/// <summary>
		/// Gets all CE ammo recipes which don't have "recipeMaker" -> Non-auto-generated recipes. For auto-generated recipes the "item" should be modified instead.
		/// </summary>
		/// <returns>List of non-auto-generated recipes' defs.</returns>
		private static IEnumerable<RecipeDef> GetAllAmmoRecipes()
		{
			var ammoRecipesDefs = DefDatabase<RecipeDef>.AllDefsListForReading
				.Where(recipe => recipe.products
					.Any(prod =>
						(IsCEAmmo(prod.thingDef) &&
						prod.thingDef.recipeMaker == null)      //non-auto-generated recipe
					)
				);
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

		/// <summary>
		/// Debug logging
		/// </summary>
		/// <param name="msg"></param>
		public static void Log(string msg)
		{
				File.AppendAllText(logFile, msg + "\n");
		}

		public static void Log_Error(string msg)
		{
			Verse.Log.Error($"[Life Lessons: Patches] {msg}");
			if (LLPatchesMod.settings.patchCEAmmo_Logging)
				File.AppendAllText(logFile, msg + "\n");
		}
	}
}
