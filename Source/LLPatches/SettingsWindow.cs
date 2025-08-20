using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using LudeonTK;

namespace LLPatches
{
	public class LLPatchesMod : Mod, ITabHost
	{
		public static LLPatchesSettings settings;

		public enum SettingsTab { Nothing, CEAmmoMain, CEAmmoTemplates, dummy1, dummy2 };		// Types of tabs to distinguish.
		private SettingsTab _currentTab;
		private readonly Dictionary<SettingsTab, ITabView> _tabInstances;           // Class for each tab. They must realize ITabView interface.
		private readonly List<TabRecord> _tabs = new List<TabRecord>();             // List for RW TabDrawer.
		static readonly float tabShift = 25f;
		static readonly float contractBy = 12f;

		public LLPatchesMod(ModContentPack content) : base(content)
		{
			// Load settings.
			settings = GetSettings<LLPatchesSettings>();

			// Initialize instances for tabs.
			_tabInstances = new Dictionary<SettingsTab, ITabView>()
			{
				{ SettingsTab.CEAmmoMain, new TabContent_CEAmmoMain(this) },
				{ SettingsTab.CEAmmoTemplates, new TabContent_CEAmmoTemplates(this) },
				{ SettingsTab.dummy1, new dummyTab(this) },
				{ SettingsTab.dummy2, new dummyTab2(this) },
			};
			ResetTabs();
			_currentTab = SettingsTab.CEAmmoMain;
		}

		public override string SettingsCategory() => "Life Lessons: Patches";

		public override void DoSettingsWindowContents(Rect inRect)
		{
			// Draw tabs.
			inRect.y += tabShift;
			inRect.height -= tabShift;
			Widgets.DrawMenuSection(inRect);
			TabDrawer.DrawTabs(inRect, _tabs);

			// Draw main content.
			Rect contentRect = inRect.ContractedBy(contractBy);
			if (_tabInstances.TryGetValue(_currentTab, out ITabView instance))
			{
				if (instance.Enabled())
					instance.Draw(contentRect);
			}
			else
				Widgets.Label(inRect, "Nothing is here for this mod combinations...");
		}

		/// <summary>
		/// Resets tabs. Can be called from inside of a tab.
		/// </summary>
		public void ResetTabs()
		{
			_tabs.Clear();
			foreach (var kv in _tabInstances)
			{
				if (kv.Value.Enabled())
					_tabs.Add(new TabRecord(kv.Value.GetLabel(), () => _currentTab = kv.Key, () => _currentTab == kv.Key));
			}
		}
	}
}
