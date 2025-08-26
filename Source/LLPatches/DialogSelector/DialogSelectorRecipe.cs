using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LLPatches.DialogSelector
{
	public class DialogSelectorRecipe : DialogSelectorBase
	{
		DialogSelectorRecipe(List<DialogSelectorRow> inputList, Action<string> onClick) : base(inputList, onClick) { }

		protected override void DrawRow(Rect inRect, DialogSelectorRow item)
		{
			Widgets.DrawHighlightIfMouseover(inRect);
			Widgets.Label(inRect, item.Label);
		}
	}
}
