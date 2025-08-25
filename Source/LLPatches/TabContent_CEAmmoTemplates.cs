using CombatExtended;
using LudeonTK;
using ResearchConnector;
using RW_Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace LLPatches
{
	public class TabContent_CEAmmoTemplates : ITabView
	{
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

		private List<CEAmmoTemplate> _filteredList = new List<CEAmmoTemplate>();

		private static GUIStyle _disabledTextFieldStyle;
		public static readonly Color SelectedButtonRed = new Color(1, 0.5f, 0.5f, 1f);
		public static readonly Color SelectedButtonGreen = new Color(0.5f, 1f, 0.5f, 1f);

		CEAmmoTemplate _toDelete;
		bool _toAdd = false;

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

			DrawTemplatesData(menuActonsRect, menuRecipeRect, menuTemplateRect, curY);

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
				LLPatchesMod.settings.CEAmmoTemplates.Add(new CEAmmoTemplate("",""));
				_toAdd = false;
				UpdateFilter();
			}
		}

		private float DrawTemplatesData(Rect menuActonsRect, Rect menuRecipeRect, Rect menuTemplateRect, float curY)
		{
			foreach (CEAmmoTemplate template in _filteredList)
			{
				// Text field style selector.
				var tfStyle = template.IsActive ? Text.CurTextFieldStyle : DisabledTextFieldStyle;

				//== Actions.
				Rect actionsRect = menuActonsRect;
				actionsRect.y = curY;
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

				Rect recipeRect = menuRecipeRect;
				recipeRect.y = curY;
				recipeRect.height = contentRowHeight;
				Rect prefixRect = new Rect(recipeRect.x, recipeRect.y, _prefixWidth, contentRowHeight);
				template.Prefix = Utils_GUI.TextFieldStruck(prefixRect, template.Prefix, tfStyle, !template.IsActive);
				Rect suffixRect = new Rect(recipeRect.x + _prefixWidth + _verticalGapSmall, recipeRect.y, _suffixWidth, contentRowHeight);
				template.Suffix = Utils_GUI.TextFieldStruck(suffixRect, template.Suffix, tfStyle, !template.IsActive);
				Rect searchButton = new Rect(suffixRect.xMax, recipeRect.y, contentRowHeight, contentRowHeight);
				Widgets.ButtonImage(searchButton, TexButton.OpenInspector);
				//Widgets.DrawBox(searchButton);
				//Widgets.DrawBox(recipeRect);

				Rect templateRect = menuTemplateRect;
				templateRect.y = curY;
				templateRect.height = contentRowHeight;
				Rect templateFieldRect = new Rect(templateRect.x, templateRect.y, templateRect.width - _selectButtonWidth - _verticalGapSmall, contentRowHeight);
				template.Template = Utils_GUI.TextFieldStruck(templateFieldRect, template.Template, tfStyle, !template.IsActive);
				//Utils_GUI.LabelCentered(new Rect(templateRect.xMax - _selectButtonWidth, templateRect.y, _selectButtonWidth, contentRowHeight), "▼ Select");
				//Widgets.DrawBox(templateRect);

				GUI.color = Color.white;

				curY += contentRowHeight;
			}

			return curY;
		}

		private static Rect DrawMenuTemplates(Rect mainContentRect, Rect menuRecipeRect)
		{
			Rect menuTemplateRect = menuRecipeRect;
			menuTemplateRect.x += menuRecipeRect.width + _verticalGapBig;
			menuTemplateRect.width = mainContentRect.width - menuTemplateRect.x + mainContentRect.x;
			Utils_GUI.LabelCentered(new Rect(menuTemplateRect.x, menuTemplateRect.y + rowHeight, menuTemplateRect.width, rowHeight), "Life Lessons Proficiency Template");
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
