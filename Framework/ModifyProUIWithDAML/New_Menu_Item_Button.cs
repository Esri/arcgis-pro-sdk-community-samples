using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;

namespace ModifyProUIWithDAML
{
    internal class New_Menu_Item_Button : Button
    {
        protected override void OnClick()
        {
            MessageBox.Show("New Menu item");
        }
    }
}
