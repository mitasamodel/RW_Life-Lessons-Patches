using CombatExtended;
using LLPatches.DialogSelector;
using LudeonTK;
using RimWorld;
using RW_Utils;
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

		public void Draw(Rect inRect)
		{
			var wrap = Utils_GUI.SetWrap(false);

			Rect mainContentRect = inRect;
			mainContentRect.height -= Utils_GUI.resetButtonAreaHeight;
			float curY = mainContentRect.y;

			curY += SearchField(mainContentRect, curY);

			Rect menuRect = new Rect(mainContentRect.x, curY, mainContentRect.width, rowHeight * 2);
			Rect menuActonsRect = DrawMenuActions(menuRect);
			Rect menuRecipeRect = DrawMenuRecipes(menuActonsRect);
			Rect menuTemplateRect = DrawMenuTemplates(mainContentRect, menuRecipeRect);

			curY += menuRect.height;

			DrawTemplatesData(menuActonsRect, menuRecipeRect, menuTemplateRect, curY, mainContentRect);

			// Restore to defaults button.
			if (Utils_GUI.ResetButton(inRect, "Restore defaults"))
			{
				LLPatchesMod.settings.RestoreCEAmmoTemplates();
				UpdateFilter();
			}

			Utils_GUI.RestoreWrap(wrap);

			// Delete template.
			if (_toDelete != null)
			{
				LLPatchesMod.settings.CEAmmoTemplates.Remove(_toDelete);
				_toDelete = null;
				UpdateFilter();
			}

			// Add an item.
			if (_toAdd)
			{
				LLPatchesMod.settings.CEAmmoTemplates.Add(new CEAmmoTemplate("", ""));
				_toAdd = false;
				UpdateFilter();
			}
		}

		private float DrawTemplatesData(Rect menuActonsRect, Rect menuRecipeRect, Rect menuTemplateRect, float curY, Rect mainContentRect)
		{
			float totalHeight = contentRowHeight * _filteredList.Count;
			Rect scrollPositionRect = new Rect(mainContentRect.x, curY, mainContentRect.width, mainContentRect.height - curY + mainContentRect.y);
			Rect scrollContentRect = new Rect(0f, 0f, scrollPositionRect.width - Utils_GUI.scrollWidth, totalHeight);
			Utils_GUI.DrawBox(mainContentRect, Color.green);
			Widgets.DrawBox(scrollPositionRect);

			float viewTop = _scroll.y;
			float viewBot = _scroll.y + scrollPositionRect.height;
			float scrollY = 0f;
			Widgets.BeginScrollView(scrollPositionRect, ref _scroll, scrollContentRect);
			Rect rowRect = new Rect(0f, scrollY, scrollContentRect.width, Utils_GUI.rowHeight);

			bool drawBoxes = false;
#if DEBUG
			drawBoxes = false;
#endif

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
				if (drawBoxes) Widgets.DrawBox(suspendButtonRect);

				// Delete button.
				Rect deleteButtonRect = new Rect(suspendButtonRect.xMax, actionsRect.y, contentRowHeight, contentRowHeight);
				if (Widgets.ButtonImage(deleteButtonRect, LLPatches.IconRemove, Color.white, SelectedButtonRed))
					_toDelete = template;
				if (drawBoxes) Widgets.DrawBox(deleteButtonRect);

				// Restore button.
				Rect restoreButtonRect = new Rect(deleteButtonRect.xMax, actionsRect.y, contentRowHeight, contentRowHeight);
				//Widgets.ButtonImage(restoreButtonRect, LLPatches.IconRestore);
				if (drawBoxes) Widgets.DrawBox(restoreButtonRect);

				//== Recipe.
				Rect recipeRect = menuRecipeRect;
				recipeRect.y = scrollY;
				recipeRect.x -= Utils_GUI.scrollWidth;
				recipeRect.height = contentRowHeight;
				Rect prefixRect = new Rect(recipeRect.x, recipeRect.y, _prefixWidth, contentRowHeight);
				template.Prefix = Utils_GUI.TextFieldStruck(prefixRect, template.Prefix, tfStyle, !template.IsActive);
				Rect suffixRect = new Rect(recipeRect.x + _prefixWidth + _verticalGapSmall, recipeRect.y, _suffixWidth, contentRowHeight);
				template.Suffix = Utils_GUI.TextFieldStruck(suffixRect, template.Suffix, tfStyle, !template.IsActive);
				Rect searchButton = new Rect(suffixRect.xMax, recipeRect.y, contentRowHeight, contentRowHeight);
				if (Widgets.ButtonImage(searchButton, TexButton.OpenInspector))
				{
					Dialog_SelectorLauncher.Open2(
						() =>
							new DialogSelectorRecipe(
								BuildRowsRecipes(),
								recipe => template.Suffix = recipe
							)
						);
				}
				if (drawBoxes) Widgets.DrawBox(searchButton);
				if (drawBoxes) Widgets.DrawBox(recipeRect);

				Rect templateRect = menuTemplateRect;
				templateRect.y = scrollY;
				templateRect.x -= Utils_GUI.scrollWidth;
				templateRect.height = contentRowHeight;
				Rect templateFieldRect = new Rect(templateRect.x, templateRect.y, templateRect.width - _selectButtonWidth - _verticalGapSmall, contentRowHeight);
				template.Template = Utils_GUI.TextFieldStruck(templateFieldRect, template.Template, tfStyle, !template.IsActive);
				//Utils_GUI.LabelCentered(new Rect(templateRect.xMax - _selectButtonWidth, templateRect.y, _selectButtonWidth, contentRowHeight), "▼ Select");
				if (drawBoxes) Widgets.DrawBox(templateRect);

				GUI.color = Color.white;

				scrollY += contentRowHeight;
				curY += contentRowHeight;
			}
			Widgets.EndScrollView();

			return curY;
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
			//menuActionsRect.width = rowHeight * 3 + _verticalGapSmall * 2;
			menuActionsRect.width = rowHeight * 3;
			float iconWidth = rowHeight * 2;
			Rect iconRect = new Rect((menuActionsRect.width - iconWidth) / 2, menuActionsRect.y, iconWidth, iconWidth);
			if (Widgets.ButtonImage(iconRect, TexButton.Plus))
				_toAdd = true;
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

			// Filter first.
			var filtered = LLPatchesMod.settings.CEAmmoTemplates
				.Where(t => string.IsNullOrEmpty(_search) || Matches(t.Prefix) || Matches(t.Suffix) || Matches(t.Template));

			// Order.
			_filteredList = filtered
				.OrderBy(t => string.IsNullOrEmpty(t.Suffix) ? 0 : 1)   // All empty suffixes treated as "0", means they go up.
				.ThenByDescending(t => t.Suffix?.Length)    // All remaining items ordered normally by length.
				.ToList();
		}

		public bool Enabled() => ModsConfig.IsActive("CETeam.CombatExtended") && LLPatchesMod.settings.patchCEAmmo_Manual;
		public string GetLabel() => "CE Ammo Templates";
	}
}
