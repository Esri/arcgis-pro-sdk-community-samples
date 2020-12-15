using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicTools
{
    class QueryEditBox : ArcGIS.Desktop.Framework.Contracts.EditBox
    {

        public QueryEditBox()
        {
            Module1.Current.QueryValueEditBox = this;
        }


    }
}
