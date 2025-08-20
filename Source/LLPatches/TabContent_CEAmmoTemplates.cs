using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LLPatches
{
	public class TabContent_CEAmmoTemplates : ITabView
	{
		ITabHost _host;

		public TabContent_CEAmmoTemplates(ITabHost host)
		{
			_host = host;
		}

		public void Draw(Rect inRect)
		{
			//Rect mainContentRect = inRect;
			//mainContentRect.height -= Utils_GUI.resetButtonAreaHeight;

			//// Reset to defaults button
			//if (Utils_GUI.ResetButton(inRect, "Reset to defaults"))
			//	LLPatchesMod.settings.ResetCEAmmoTemplates();

			//Old
			Listing_Standard listing = new Listing_Standard();
			listing.Begin(inRect);
			foreach (var key in LLPatchesMod.settings.Values.Keys.ToList().OrderByDescending(k => k.Length))
				LLPatchesMod.settings.Values[key] = Utils_GUI.LabeledTextField(listing, key, LLPatchesMod.settings.Values[key]);
			listing.End();
		}

		public bool Enabled() => ModsConfig.IsActive("CETeam.CombatExtended") && LLPatchesMod.settings.patchCEAmmo_Manual;
		public string GetLabel() => "CE Ammo Templates";
	}
}
