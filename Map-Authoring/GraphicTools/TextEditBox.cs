using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicTools
{
    class TextEditBox : ArcGIS.Desktop.Framework.Contracts.EditBox
    {

        public TextEditBox()
        {
            Module1.Current.TextValueEditBox = this;
            Text = "Default Text";
        }


    }
}
