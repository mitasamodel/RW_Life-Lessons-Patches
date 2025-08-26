using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLPatches
{
	public class DialogSelectorRow
	{
		public string Label { get; }
		public string Tooltip { get; }
		public string ExtraSearchField { get; }
		public string ReturnId { get; }
		public DialogSelectorRow(string label, string tooltips, string extraSearchField, string id)
		{
			Label = label;
			Tooltip = tooltips;
			ExtraSearchField = extraSearchField;
			ReturnId = id;
		}
	}
}
