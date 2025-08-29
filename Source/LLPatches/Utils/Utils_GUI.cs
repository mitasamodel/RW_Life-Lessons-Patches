using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace LLPatches.RW_JustUtils
{
	public static class Utils_GUI
	{
		public const float buttonHeigt = 30f;
		public const float gapHeight = 12f;
		public const float resetButtonAreaHeight = buttonHeigt + gapHeight;
		public const float rowHeight = 22f;
		public const float scrollWidth = 16f;

		public static string TextFieldStruck(Rect inRect, string str, GUIStyle style = null, bool strike = false)
		{
			if (style == null) style = Text.CurTextFieldStyle;

			string result = GUI.TextField(inRect, str ?? "", style);

			if (strike)
			{
				GapLine(inRect.x, inRect.y + inRect.height / 2, inRect.width);
			}

			return result;
		}

		public static void DrawBox(Rect rect, Color color, int thickness = 1)
		{
			GUI.color = color;
			Widgets.DrawBox(rect, thickness);
			GUI.color = Color.white;
		}
		public static void GapLine(float x, float y, float width)
		{
			var color = GUI.color;
			GUI.color = Color.gray;
			Widgets.DrawLineHorizontal(x, y, width);
			GUI.color = color;
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

		public static bool SetWrap(bool set)
		{
			var wrap = Text.WordWrap;
			Text.WordWrap = set;
			return wrap;
		}

		public static void RestoreWrap(bool wrap)
		{
			Text.WordWrap = wrap;
		}

		public static bool ButtonInvisibleDoubleClick(Rect rect, bool doMouseoverSound = true, int button = 0)
		{
			if (doMouseoverSound)
			{
				MouseoverSounds.DoRegion(rect);
			}
			Event ev = Event.current;
			if (ev.type == EventType.MouseDown && ev.button == button && rect.Contains(ev.mousePosition))
			{
				if (ev.clickCount == 2)
				{
					ev.Use(); // consume the event
					return true;
				}
			}
			return false;
		}

		public static void DrawLineVertical(float x, float y, float length, Color color)
		{
			Widgets.DrawBoxSolid(new Rect(x, y, 1f, length), color);
		}

		public static void DrawTextureWithHighlight(Rect rect, Texture2D texture, Color color)
		{
			Widgets.DrawTextureFitted(rect, texture, 1f);
			if (Mouse.IsOver(rect))
			{
				GUI.color = new Color(color.r, color.g, color.b, 0.5f);
				Widgets.DrawTextureFitted(rect, texture, 1f);
				GUI.color = Color.white;
			}
		}

		public static void LabelTooltip(Rect inRect, string label, string tooltip = null)
		{
			if (!tooltip.NullOrEmpty())
			{
				if (Mouse.IsOver(inRect))
				{
					Widgets.DrawHighlight(inRect);
				}

				TooltipHandler.TipRegion(inRect, tooltip);
			}

			Widgets.Label(inRect, label);
		}

		public static void LabelCentered(Rect inRect, string label)
		{
			var oldAnchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(inRect, label);
			Text.Anchor = oldAnchor;
		}
	}
}
