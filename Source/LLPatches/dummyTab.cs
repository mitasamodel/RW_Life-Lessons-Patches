using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace LLPatches
{
	internal class dummyTab : ITabView
	{
		public dummyTab(ITabHost host) { }

		public void Draw(Rect inRect)
		{
			Widgets.Label(inRect, nameof(dummyTab));
		}

		public bool Enabled()
		{
			return true;
		}

		public string GetLabel()
		{
			return nameof(dummyTab);
		}
	}
}
