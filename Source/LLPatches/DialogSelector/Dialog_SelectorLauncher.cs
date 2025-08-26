using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace LLPatches
{
	public static class Dialog_SelectorLauncher
	{
		public static void Open(
			Func<List<DialogSelectorRow>> buildRows,
			Action<string> onClick,
			Vector2? scroll = null,
			Action<Vector2> onCloseScroll = null,
			Rect? anchorScreenRect = null
			)
		{
			if (buildRows == null)
			{
				Logger.Log_Error($"[Dialog_SelectorLauncher] Unexpected null 'buildRows'.");
				return;
			}

			// Now build rows.
			List<DialogSelectorRow> rows = buildRows();

			// New window class.
			Window dialog = new Dialog_Selector(rows, onClick, scroll ?? Vector2.zero, onCloseScroll, anchorScreenRect);
			Find.WindowStack.Add(dialog);
		}

		public static void Open2(Func<Window> factory)
		{
			Window dialog = factory();
			Find.WindowStack.Add(dialog);
		}
	}
}
