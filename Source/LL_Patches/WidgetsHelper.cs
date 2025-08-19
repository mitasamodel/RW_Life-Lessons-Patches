using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LLPatches
{
	public static class WidgetsHelper
	{
		public static void DrawBox(Rect rect, Color color, int thickness = 1)
		{
			GUI.color = color;
			Widgets.DrawBox(rect, thickness);
			GUI.color = Color.white;
		}
	}
}
