using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LLPatches
{
	public static class Listing_StandardExtensions
	{
		public static void LabelCentered(this Listing_Standard listing, string label, float maxHeight = -1f, TipSignal? tipSignal = null)
		{
			var oldAnchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleCenter;
			listing.Label(label, maxHeight, tipSignal);
			Text.Anchor = oldAnchor;
		}
	}
}
