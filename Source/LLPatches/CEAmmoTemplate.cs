using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace LLPatches
{
	public class CEAmmoTemplate : IExposable
	{
		private string _prefix;

		private string _suffix;

		private string _template;

		private bool _active;

		public string Prefix
		{
			get => _prefix;
			set => _prefix = value;
		}

		public string Suffix
		{
			get => _suffix;
			set => _suffix = value;
		}

		public string Template
		{
			get => _template;
			set => _template = value;
		}

		public bool IsActive => _active;

		public CEAmmoTemplate() { }     //Scribe needs a constructor without any parameters.
		public CEAmmoTemplate(string suffix, string template) : this(null, suffix, template) { }
		public CEAmmoTemplate(string prefix, string suffix, string template)
		{
			Prefix = prefix;
			Suffix = suffix;
			Template = template;
			this.Enable();
		}

		public void SwitchEnable()
		{
			if (IsActive) Disable();
			else Enable();
		}
		public void Disable() => _active = false;
		public void Enable() => _active = true;

		public void ExposeData()
		{
			Scribe_Values.Look(ref _prefix, "prefix", string.Empty);
			Scribe_Values.Look(ref _suffix, "suffix", string.Empty);
			Scribe_Values.Look(ref _template, "template", string.Empty);
			Scribe_Values.Look(ref _active, "active", true);
		}
	}
}
