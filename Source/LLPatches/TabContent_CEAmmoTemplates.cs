using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using LudeonTK;
using RW_Utils;

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
		[TweakValue("0_Menu", 0f, 30f)]
		private static float _verticalGapBig = 12f;


		public TabContent_CEAmmoTemplates(ITabHost host)
		{
			_host = host;
			_scroll = Vector2.zero;
			_search = "";
		}

		public void Draw(Rect inRect)
		{
			var wrap = Utils_GUI.SetWrap(false);

			Rect mainContentRect = inRect;
			mainContentRect.height -= Utils_GUI.resetButtonAreaHeight;
			float curY = 0f;

			curY += SearchField(mainContentRect, curY);

			Rect menuRect = new Rect(mainContentRect.x, mainContentRect.y + curY, mainContentRect.width, rowHeight * 2);
			Utils_GUI.DrawBox(menuRect, Color.grey);

			Rect menuActonsRect = menuRect;
			menuActonsRect.width = rowHeight * 3 + _verticalGapSmall * 2 + _verticalGapBig * 1;
			Widgets.DrawBox(menuActonsRect);

			Rect menuRecipeRect = menuActonsRect;
			menuRecipeRect.x += menuActonsRect.width;
			menuRecipeRect.width = _prefixWidth + _suffixWidth + _verticalGapSmall * 2 + contentRowHeight + _verticalGapBig * 1;
			

			Widgets.DrawBox(menuRecipeRect);

			Rect menuTemplateRect = menuRecipeRect;
			menuTemplateRect.x += menuRecipeRect.width;
			menuTemplateRect.width = mainContentRect.width - menuTemplateRect.x;
			Widgets.DrawBox(menuTemplateRect);


			curY += menuRect.height;


			// Reset to defaults button
			if (Utils_GUI.ResetButton(inRect, "Restore defaults"))
				LLPatchesMod.settings.ResetCEAmmoTemplates();

			Utils_GUI.RestoreWrap(wrap);

			//Old
			//Listing_Standard listing = new Listing_Standard();
			//listing.Begin(inRect);
			//foreach (var key in LLPatchesMod.settings.Values.Keys.ToList().OrderByDescending(k => k.Length))
			//	LLPatchesMod.settings.Values[key] = Utils_GUI.LabeledTextField(listing, key, LLPatchesMod.settings.Values[key]);
			//listing.End();
		}

		private float SearchField(Rect mainContentRect, float yy)
		{
			Rect searchRect = new Rect(mainContentRect.x, mainContentRect.y + yy, mainContentRect.width, rowHeight);
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

		}

		public bool Enabled() => ModsConfig.IsActive("CETeam.CombatExtended") && LLPatchesMod.settings.patchCEAmmo_Manual;
		public string GetLabel() => "CE Ammo Templates";
	}
}
