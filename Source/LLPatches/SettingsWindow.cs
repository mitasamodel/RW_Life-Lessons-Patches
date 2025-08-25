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
		internal static bool forceRewriteSettings = false;

		public enum SettingsTab { Nothing, CEAmmoMain, CEAmmoTemplates, dummy1, dummy2 };       // Types of tabs to distinguish.
		private SettingsTab _currentTab;
		private readonly Dictionary<SettingsTab, ITabView> _tabInstances;           // Class for each tab. They must implement ITabView interface.
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
				//{ SettingsTab.dummy1, new dummyTab(this) },
				//{ SettingsTab.dummy2, new dummyTab2(this) },
			};
			ResetTabs();
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
				if (!instance.Enabled())
				{
					ResetTabs();
					Verse.Log.Warning($"[{LLPatches.modName}: Pre Draw()] Unexpected _currentTab value. Resetting tabs.");
					return;
				}
				instance.Draw(contentRect);
			}
			else
				Widgets.Label(contentRect, "Nothing is here for this mods combination...");
		}

		/// <summary>
		/// Resets tabs. Can be called from inside of a tab.
		/// </summary>
		public void ResetTabs()
		{
			_tabs.Clear();
			ITabView instance;

			// Repopulate the tabs list.
			// Iteration through Dictionary is not guaranteed to be in the same order as in initialization. Use enum instead.
			foreach (SettingsTab key in Enum.GetValues(typeof(SettingsTab)))
			{
				if (_tabInstances.TryGetValue(key, out instance) && instance.Enabled())
				{
					_tabs.Add(new TabRecord(instance.GetLabel(), () => _currentTab = key, () => _currentTab == key));
				}
			}

			// If current tab is disabled.
			if (!_tabInstances.TryGetValue(_currentTab, out instance) || !instance.Enabled())
			{
				if (_tabs.Count > 0)
				{
					// Select first enabled tab.
					var firstEnabledKey = _tabInstances.First(kv => kv.Value.Enabled()).Key;
					_currentTab = firstEnabledKey;
				}
				else
					_currentTab = SettingsTab.Nothing;
			}
		}
	}
}
