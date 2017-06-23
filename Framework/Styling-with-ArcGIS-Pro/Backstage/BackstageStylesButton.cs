using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ControlStyles.Backstage
{
    internal class BackstageStylesButton : Button
    {
        protected override void OnClick()
        {
            FrameworkApplication.OpenBackstage("ControlStyles_Backstage_BackstageStyling");
        }
    }
}
