using CombatExtended;
using LifeLessons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

using static LLPatches.Logger;

namespace LLPatches
{
	[StaticConstructorOnStartup]
	public static class LLPatches
	{
		internal const string modName = "LLPatches";
		public static Texture2D IconRemove = ContentFinder<Texture2D>.Get("LLPatches/Remove", true);
		public static Texture2D IconRestore = ContentFinder<Texture2D>.Get("ui/widgets/rotleft", true);

		static LLPatches()
		{
			Logger.Init();

			// Force re-write settings if migration happened.
			if (LLPatchesMod.forceRewriteSettings)
				LoadedModManager.GetMod<LLPatchesMod>()?.WriteSettings();
			//LongEventHandler.ExecuteWhenFinished(() => LoadedModManager.GetMod<LLPatchesMod>()?.WriteSettings());

			//CE Ammos have their separated recipes instead of auto-generated for items
			if (LLPatchesMod.settings.patchUnpatchedCEAmmo)
				ProcessCEAmmoRecipes();
		}

		public static void ProcessCEAmmoRecipes()
		{
			if (LLPatchesMod.settings.CEAmmoTemplates == null)
			{
				Log_Error($"[Life Lessons: Patches] Settings are null: {LLPatchesMod.settings.CEAmmoTemplates == null}");
				Verse.Log.Message("[Life Lessons: Patches] Please report it to mod author");
				return;
			}

			// List for ammo without template.
			List<string> noTemplateRecipes = new List<string>();

			// List of wrong templates.
			List<string> wrongTemplates = new List<string>();

			// Templates.
			var templates = LLPatchesMod.settings.CEAmmoTemplates
				.Where(t => t.IsActive)
				.OrderTemplates()
				.ToList();

			int errorOnceKey = Rand.Int;

			foreach (RecipeDef recipe in Other.GetAllAmmoRecipes())
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
				foreach (CEAmmoTemplate template in templates)
				{
					// Both are set: Prefix & Suffix.
					if (!string.IsNullOrEmpty(template.Prefix) && !string.IsNullOrEmpty(template.Suffix))
					{
						if (recipe.defName.StartsWith(template.Prefix, StringComparison.OrdinalIgnoreCase) &&
							recipe.defName.EndsWith(template.Suffix, StringComparison.OrdinalIgnoreCase))
						{
							templateName = template.Template;
							if (LLPatchesMod.settings.patchCEAmmo_Logging)
								Log($"\t[Template] {template.Prefix}:::{template.Suffix} Name: {templateName}");
							break;
						}
					}

					// Only Prefix is set.
					else if (!string.IsNullOrEmpty(template.Prefix))
					{
						if (recipe.defName.StartsWith(template.Prefix, StringComparison.OrdinalIgnoreCase))
						{
							templateName = template.Template;
							if (LLPatchesMod.settings.patchCEAmmo_Logging)
								Log($"\t[Template] {template.Prefix}:::{template.Suffix} Name: {templateName}");
							break;
						}
					}

					// Only Suffix is set.
					else if (!string.IsNullOrEmpty(template.Suffix))
					{
						if (recipe.defName.EndsWith(template.Suffix, StringComparison.OrdinalIgnoreCase))
						{
							templateName = template.Template;
							if (LLPatchesMod.settings.patchCEAmmo_Logging)
								Log($"\t[Template] {template.Prefix}:::{template.Suffix} Name: {templateName}");
							break;
						}
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
					if (!recipe.AddTemplate(templateName))
					{
						Verse.Log.WarningOnce($"[Life Lessons: Patches] Some CE Ammo templates have not been found. Enable and check log.", errorOnceKey);
						wrongTemplates.Add(templateName);
					}
				}
			}

			// Output the wrong templates list.
			if (wrongTemplates.Count > 0 && (LLPatchesMod.settings.patchCEAmmo_Logging || LLPatchesMod.settings.patchCEAmmo_LogWrongTemplates))
			{
				Log("Templates without Defs:");
				foreach (var name in wrongTemplates)
					Log($"\t- {name}");
			}

			// Output summary if any recipes were unmatched.
			if (noTemplateRecipes.Count > 0 && (LLPatchesMod.settings.patchCEAmmo_Logging || LLPatchesMod.settings.patchCEAmmo_LogUnpatched))
			{
				Log("Recipes with no matching template:");
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

		static bool AddTemplate(this RecipeDef recipe, string templateName)
		{
			var newExtension = new BillProficiencyExtension()
			{
				templateDef = DefDatabase<ThingProficiencyTemplateDef>.GetNamedSilentFail(templateName),
				hardRequirement = false
			};
			if (newExtension.templateDef == null) return false;
			else
			{
				recipe.modExtensions ??= new List<DefModExtension>();
				recipe.modExtensions.Add(newExtension);
				return true;
			}
		}

		/// <summary>
		/// Debug logging
		/// </summary>
		/// <param name="msg"></param>

	}
}
