using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LLPatches
{
	public interface ITabView
	{
		void Draw(Rect inRect);
		bool Enabled();
		string GetLabel();
	}
}
