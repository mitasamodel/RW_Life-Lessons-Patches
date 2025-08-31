using CombatExtended;
using LifeLessons;
using LLPatches.DialogSelector;
using LudeonTK;
using RimWorld;
using LLPatches.RW_JustUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace LLPatches
{
	public class TabContent_CEAmmoTemplates : ITabView
	{
		// Caches, saved data
		private List<CEAmmoTemplate> _filteredList = new List<CEAmmoTemplate>();
		CEAmmoTemplate _toDelete;
		bool _toAdd = false;

		private readonly ITabHost _host;

		private const float rowHeight = Utils_GUI.rowHeight;
		private const float contentRowHeight = Utils_GUI.rowHeight;

		private static Vector2 _scroll;

		private static string _search;

		[TweakValue("0_Menu", 50f, 300f)]
		private static float _prefixWidth = 150f;
		[TweakValue("0_Menu", 50f, 300f)]
		private static float _suffixWidth = 150f;
		[TweakValue("0_Menu", 0f, 30f)]
		private static float _verticalGapSmall = 6f;
		[TweakValue("0_Menu", 0f, 50f)]
		private static float _verticalGapBig = 20f;
		[TweakValue("0_Menu", 30f, 200f)]
		private static float _selectButtonWidth = 75f;
		[TweakValue("0_Menu", 30f, 200f)]
		private static float _searchWidth = 75f;

		private static GUIStyle _disabledTextFieldStyle;
		public static readonly Color SelectedButtonRed = new Color(1, 0.5f, 0.5f, 1f);
		public static readonly Color SelectedButtonGreen = new Color(0.5f, 1f, 0.5f, 1f);

		public TabContent_CEAmmoTemplates(ITabHost host)
		{
			_host = host;
			_scroll = Vector2.zero;
			_search = "";
			UpdateFilter();
		}

		private static GUIStyle DisabledTextFieldStyle
		{
			get
			{
				if (_disabledTextFieldStyle == null)
				{
					_disabledTextFieldStyle = new GUIStyle(Text.CurTextFieldStyle);
					var c = new Color32(236, 56, 59, 255); // pick your grey
					_disabledTextFieldStyle.normal.textColor = c;
					_disabledTextFieldStyle.focused.textColor = c;
					_disabledTextFieldStyle.active.textColor = c;
					_disabledTextFieldStyle.hover.textColor = c;
					// Optional: selection color / caret
					// GUI.skin.settings.selectionColor = new Color(c.r, c.g, c.b, 0.35f);
					// GUI.skin.settings.cursorColor    = c;
				}
				return _disabledTextFieldStyle;
			}
		}

		//bool drawBox = true;
		//bool drawBoxDeep = false;

		public void Draw(Rect inRect)
		{
			var wrap = Utils_GUI.SetWrap(false);

			Rect mainContentRect = inRect;
			mainContentRect.height -= Utils_GUI.resetButtonAreaHeight;
			float curY = mainContentRect.y;

			curY += SearchField(mainContentRect, curY);

			Rect menuRect = new Rect(mainContentRect.x, curY, mainContentRect.width, rowHeight * 2);
			//Utils_GUI.DrawBox(menuRect, Color.green);
			Rect menuActonsRect = DrawMenuActions(menuRect);
			//Utils_GUI.DrawBox(menuActonsRect, Color.grey);
			Rect menuRecipeRect = DrawMenuRecipes(menuActonsRect);
			//if (drawBox) Utils_GUI.DrawBox(menuRecipeRect, Color.grey);
			Rect menuTemplateRect = DrawMenuTemplates(mainContentRect, menuRecipeRect);
			//if (drawBox) Utils_GUI.DrawBox(menuTemplateRect, Color.grey);

			curY += menuRect.height;

			DrawTemplatesData(menuActonsRect, menuRecipeRect, menuTemplateRect, curY, mainContentRect);

			// Restore to defaults button.
			if (Utils_GUI.ResetButton(inRect, "Restore defaults"))
			{
				LLPatchesMod.Settings.RestoreCEAmmoTemplates();
				UpdateFilter();
			}

			Utils_GUI.RestoreWrap(wrap);

			// Delete template.
			if (_toDelete != null)
			{
				LLPatchesMod.Settings.CEAmmoTemplates.Remove(_toDelete);
				_toDelete = null;
				UpdateFilter();
			}

			// Add an item.
			if (_toAdd)
			{
				LLPatchesMod.Settings.CEAmmoTemplates.Add(new CEAmmoTemplate("", ""));
				_toAdd = false;
				UpdateFilter();
			}
		}

		private float DrawTemplatesData(Rect menuActonsRect, Rect menuRecipeRect, Rect menuTemplateRect, float curY, Rect mainContentRect)
		{
			float totalHeight = contentRowHeight * _filteredList.Count;
			Rect scrollPositionRect = new Rect(mainContentRect.x, curY, mainContentRect.width, mainContentRect.height - curY + mainContentRect.y);
			Rect scrollContentRect = new Rect(0f, 0f, scrollPositionRect.width - Utils_GUI.scrollWidth, totalHeight);
			//Utils_GUI.DrawBox(mainContentRect, Color.green);
			//Widgets.DrawBox(scrollPositionRect);

			float viewTop = _scroll.y;
			float viewBot = _scroll.y + scrollPositionRect.height;
			float scrollY = 0f;
			Widgets.BeginScrollView(scrollPositionRect, ref _scroll, scrollContentRect);
			Rect rowRect = new Rect(0f, scrollY, scrollContentRect.width, Utils_GUI.rowHeight);

			foreach (CEAmmoTemplate template in _filteredList)
			{
				// Skip all labels above the view
				if (scrollY + contentRowHeight <= viewTop)
				{
					scrollY += contentRowHeight;
					continue;
				}
				// Stop at the bottom
				if (scrollY >= viewBot)
					break;

				// Text field style selector.
				var tfStyle = template.IsActive ? Text.CurTextFieldStyle : DisabledTextFieldStyle;

				//== Actions.
				Rect actionsRect = menuActonsRect;
				actionsRect.y = scrollY;
				actionsRect.x -= Utils_GUI.scrollWidth;
				actionsRect.height = contentRowHeight;

				// Suspend button.
				Rect suspendButtonRect = new Rect(actionsRect.x, actionsRect.y, contentRowHeight, contentRowHeight);
				if (Widgets.ButtonImage(suspendButtonRect, TexButton.Suspend))
					template.SwitchEnable();
				//Widgets.DrawBox(suspendButtonRect);

				// Delete button.
				Rect deleteButtonRect = new Rect(suspendButtonRect.xMax, actionsRect.y, contentRowHeight, contentRowHeight);
				if (Widgets.ButtonImage(deleteButtonRect, LLPatches.IconRemove, Color.white, SelectedButtonRed))
					_toDelete = template;
				//Widgets.DrawBox(deleteButtonRect);

				// Restore button.
				Rect restoreButtonRect = new Rect(deleteButtonRect.xMax, actionsRect.y, contentRowHeight, contentRowHeight);
				//Widgets.ButtonImage(restoreButtonRect, LLPatches.IconRestore);
				//Widgets.DrawBox(restoreButtonRect);

				//== Recipe.
				Rect recipeRect = menuRecipeRect;
				recipeRect.y = scrollY;
				recipeRect.x -= Utils_GUI.scrollWidth;
				recipeRect.height = contentRowHeight;
				Rect prefixRect = new Rect(recipeRect.x, recipeRect.y, _prefixWidth, contentRowHeight);
				template.Prefix = Utils_GUI.TextFieldStruck(prefixRect, template.Prefix, tfStyle, !template.IsActive);
				Rect suffixRect = new Rect(recipeRect.x + _prefixWidth + _verticalGapSmall, recipeRect.y, _suffixWidth, contentRowHeight);
				template.Suffix = Utils_GUI.TextFieldStruck(suffixRect, template.Suffix, tfStyle, !template.IsActive);
				Rect searchButtonRecipe = new Rect(suffixRect.xMax, recipeRect.y, contentRowHeight, contentRowHeight);
				if (Widgets.ButtonImage(searchButtonRecipe, TexButton.OpenInspector))
				{
					DialogSelectorLauncher.Open2(RecipeSelectorFactory, template);
				}
				//Widgets.DrawBox(searchButtonRecipe);
				//Widgets.DrawBox(recipeRect);

				Rect templateRect = menuTemplateRect;
				templateRect.y = scrollY;
				templateRect.x -= Utils_GUI.scrollWidth;
				templateRect.height = contentRowHeight;
				Rect templateFieldRect = new Rect(templateRect.x, templateRect.y, templateRect.width - _selectButtonWidth - _verticalGapSmall, contentRowHeight);
				template.Template = Utils_GUI.TextFieldStruck(templateFieldRect, template.Template, tfStyle, !template.IsActive);
				Rect searchButtonTemplate = new Rect(templateFieldRect.xMax, templateFieldRect.y, contentRowHeight, contentRowHeight);
				if (Widgets.ButtonImage(searchButtonTemplate, TexButton.OpenInspector))
				{
					DialogSelectorLauncher.Open2(TemplateSelectorFactory, template);
				}
				//Utils_GUI.LabelCentered(new Rect(templateRect.xMax - _selectButtonWidth, templateRect.y, _selectButtonWidth, contentRowHeight), "▼ Select");
				//Widgets.DrawBox(templateRect);

				GUI.color = Color.white;

				scrollY += contentRowHeight;
				curY += contentRowHeight;
			}
			Widgets.EndScrollView();

			return curY;
		}

		private static Window TemplateSelectorFactory(CEAmmoTemplate template)
		{
			//TODO: currently the same class as for recipies is used. Mb in future add more info to the dialog.
			return new DialogSelectorRecipe(
					BuildRowsTemplates(),
					recipe => template.Template = recipe
				);
		}

		private static Window RecipeSelectorFactory(CEAmmoTemplate template)
		{
			return new DialogSelectorRecipe(
					BuildRowsRecipes(),
					recipe => template.Suffix = recipe
				);
		}

		private static List<DialogSelectorRow> BuildRowsTemplates()
		{
			return DefDatabase<ThingProficiencyTemplateDef>.AllDefsListForReading
				.Select(t => new DialogSelectorRow(t.defName, t.label, t.label, t.defName))
				.ToList();
		}

		private static List<DialogSelectorRow> BuildRowsRecipes()
		{
			return Other.GetAllAmmoRecipes()
				.Select(recipe => new DialogSelectorRow(recipe.defName, recipe.label, recipe.label, recipe.defName))
				.ToList();
		}

		private static Rect DrawMenuTemplates(Rect mainContentRect, Rect menuRecipeRect)
		{
			Rect menuTemplateRect = menuRecipeRect;
			menuTemplateRect.x += menuRecipeRect.width + _verticalGapBig;
			menuTemplateRect.width = mainContentRect.width - menuTemplateRect.x + mainContentRect.x;
			Rect menuTemplateLabel = new Rect(menuTemplateRect.x, menuTemplateRect.y + rowHeight, menuTemplateRect.width - _selectButtonWidth, rowHeight);
			Utils_GUI.LabelCentered(menuTemplateLabel, "Life Lessons Proficiency Template");
			return menuTemplateRect;
		}

		private static Rect DrawMenuRecipes(Rect menuActonsRect)
		{
			Rect menuRecipeRect = menuActonsRect;
			menuRecipeRect.x += menuActonsRect.width + _verticalGapSmall;
			menuRecipeRect.width = _prefixWidth + _suffixWidth + _verticalGapSmall;     // Without search button to align easily.
			Utils_GUI.LabelCentered(new Rect(menuRecipeRect.x, menuRecipeRect.y, menuRecipeRect.width, rowHeight), "Recipe def name");
			Utils_GUI.LabelCentered(new Rect(menuRecipeRect.x, menuRecipeRect.y + rowHeight, menuRecipeRect.width / 2, rowHeight), "Prefix");
			Utils_GUI.LabelCentered(new Rect(menuRecipeRect.x + menuRecipeRect.width / 2, menuRecipeRect.y + rowHeight, menuRecipeRect.width / 2, rowHeight), "Suffix");
			menuRecipeRect.width += contentRowHeight;   // Add search button to the width.
			return menuRecipeRect;
		}

		private Rect DrawMenuActions(Rect menuRect)
		{
			Rect menuActionsRect = menuRect;
			menuActionsRect.width = 3 * rowHeight;
			Rect iconsRect = menuActionsRect.CenterMiddle(2 * rowHeight, rowHeight);
			Rect addIconRect = new Rect(iconsRect.x, iconsRect.y, rowHeight, rowHeight);
			if (Widgets.ButtonImage(addIconRect, TexButton.Plus))
				_toAdd = true;
			Rect infoIconRect = new Rect(addIconRect.xMax, addIconRect.y, rowHeight, rowHeight).CenterH(rowHeight);
			//Utils_GUI.LabelTooltip(infoIconRect, "[I]", "asd");
			Widgets.ButtonImage(infoIconRect, TexButton.Info, false,
				"Templates are applied in the same order as they are displayed here.\n" +
				"Exception: newly added templates without a Prefix or Suffix are displayed at the top (for convenience), but will not be applied.\n\n" +
				"Order:\n" +
				"1. Templates that have both a Prefix and a Suffix.\n" +
				"2. Templates sorted by the length of the Prefix.\n" +
				"3. Remaining templates sorted by the length of the Suffix.\n\n" +
				"In most cases, it is fine to use only a Suffix. A Prefix should only be used when the Suffix would otherwise match several unrelated items.\n" +
				"Example: the Suffix \"_HE\" without any Prefix will match both Ammos and Shells by default."
			);

			//Widgets.DrawBox(addIconRect);
			//Widgets.DrawBox(infoIconRect);
			return menuActionsRect;
		}

		private float SearchField(Rect mainContentRect, float yy)
		{
			Rect searchLabel = new Rect(mainContentRect.x, yy, _searchWidth, rowHeight);
			Widgets.Label(searchLabel, "Filter:");
			Rect searchRect = new Rect(searchLabel.xMax, yy, mainContentRect.width - searchLabel.width, rowHeight);
			var newSearch = Widgets.TextField(searchRect, _search);
			if (newSearch != _search)
			{
				_search = newSearch;
				UpdateFilter();
			}

			return searchRect.height;
		}

		private void UpdateFilter()
		{
			// Small helper.
			static bool Matches(string s) => !string.IsNullOrEmpty(_search) && (s?.ContainsIgnoreCase(_search) ?? false);

			if (LLPatchesMod.Settings.CEAmmoTemplates == null)
			{
				Logger.Log_Error($"[TabContent_CEAmmoTemplates] Templates list is null.");
				Verse.Log.Message($"[Life Lessons: Patches] Please report it to mod author.");
				return;
			}

			// Filter first.
			var filtered = LLPatchesMod.Settings.CEAmmoTemplates
				.Where(t => string.IsNullOrEmpty(_search) ||
					(string.IsNullOrEmpty(t.Prefix) && string.IsNullOrEmpty(t.Suffix)) ||
					Matches(t.Prefix) ||
					Matches(t.Suffix) ||
					Matches(t.Template)
				);

			// Order.
			_filteredList = filtered.OrderTemplates().ToList();
		}

		public bool Enabled() => ModsConfig.IsActive("CETeam.CombatExtended") && LLPatchesMod.Settings.patchCEAmmo_Manual;
		public string GetLabel() => "CE Ammo Templates";
	}
}
