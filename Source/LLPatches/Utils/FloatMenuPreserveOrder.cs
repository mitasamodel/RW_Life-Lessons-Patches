using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace LLPatches.RW_JustUtils
{
	public class FloatMenuPreserveOrder : FloatMenu
	{
		public FloatMenuPreserveOrder(List<FloatMenuOption> optionsInOriginalOrder)
			: base(optionsInOriginalOrder)
		{
			this.options = optionsInOriginalOrder;
		}
	}
}
