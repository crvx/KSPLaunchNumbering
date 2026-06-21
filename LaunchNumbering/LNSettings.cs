using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchNumbering
{
	public class LNSettings : GameParameters.CustomParameterNode
	{
		public override GameParameters.GameMode GameMode => GameParameters.GameMode.ANY;

		public override bool HasPresets => false;

		public override string Section => "Launch Numbering";

		public override int SectionOrder => 1;

		public override string Title => string.Empty;

		[GameParameters.CustomParameterUI("Use Alternate Skin",
			toolTip = "If true, uses an alternate skin for the window")]
		public bool useAltSkin { get; set; } = true;

		public override string DisplaySection => "Launch Numbering";
	}
}
