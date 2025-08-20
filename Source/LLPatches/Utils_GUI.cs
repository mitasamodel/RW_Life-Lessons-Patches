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
	public static class Utils_GUI
	{
		public const float buttonHeigt = 30f;
		public const float gapHeight = 12f;
		public const float resetButtonAreaHeight = buttonHeigt + gapHeight;
		public const float rowHeight = 22f;

		public static void DrawBox(Rect rect, Color color, int thickness = 1)
		{
			GUI.color = color;
			Widgets.DrawBox(rect, thickness);
			GUI.color = Color.white;
		}
		public static void GapLine(float x, float y, float width)
		{
			GUI.color = Color.gray;
			Widgets.DrawLineHorizontal(x, y, width);
			GUI.color = Color.white;
		}
		public static bool ResetButton(Rect inRect, string label)
		{
			Utils_GUI.GapLine(inRect.x, inRect.yMax - buttonHeigt - gapHeight / 2f, inRect.width);
			return Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - buttonHeigt, inRect.width, buttonHeigt), label);
		}

		public static string LabeledTextField(Listing_Standard listing, string label, string value, float labelWidth = 120f, float gap = 6f)
		{
			Rect row = listing.GetRect(22f);

			Rect labelRect = new Rect(row.x, row.y, labelWidth, row.height);
			Rect fieldRect = new Rect(row.x + labelWidth + gap, row.y, row.width - labelWidth - gap, row.height);

			Widgets.Label(labelRect, label);
			return Widgets.TextField(fieldRect, value ?? "");
		}
	}
}
