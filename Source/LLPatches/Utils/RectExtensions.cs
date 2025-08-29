using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LLPatches.RW_JustUtils
{
	public static class RectExtensions
	{
		public static Rect CenterMiddle(this Rect rect, float width, float height)
		{
			var x = rect.x + (rect.width - width) / 2;
			var y = rect.y + (rect.height - height) / 2;
			return new Rect(x, y, width, height);
		}
		public static Rect CenterH(this Rect rect, float width)
		{
			var x = rect.x + (rect.width - width) / 2;
			return new Rect(x, rect.y, width, rect.height);
		}
		public static Rect MiddleV(this Rect rect, float height)
		{
			var y = rect.y + (rect.height - height) / 2;
			return new Rect(rect.x, y, rect.width, height);
		}
	}
}
