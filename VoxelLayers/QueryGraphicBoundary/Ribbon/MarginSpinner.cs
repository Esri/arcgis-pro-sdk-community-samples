using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryGraphicBoundary.Ribbon
{
	/// <summary>
	/// Allows adjustment of the "buffer", or margin, of the outline feature
	/// geometry
	/// </summary>
	internal class MarginSpinner : ArcGIS.Desktop.Framework.Contracts.Spinner
	{

		public MarginSpinner()
		{
			this.Value = Module1.Current.Margin;
		}

		protected override void OnValueChanged(double? value)
		{
			if (value.HasValue)
			{
				Module1.Current.Margin = value.Value;
			}
		}
	}
}
