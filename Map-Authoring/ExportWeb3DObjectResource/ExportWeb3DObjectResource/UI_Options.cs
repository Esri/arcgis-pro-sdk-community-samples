//   Copyright 2014 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ExportWeb3DObjectResource
{
    internal class ExportButton : Button
    {
        protected async override void OnClick()
        {
            await Exporter.Export3DMarkerSymbols();
        }
    }

    internal class DownScaleFactorEditBox : EditBox
    {
        DownScaleFactorEditBox()
        {
            Text = "1";
        }
        protected override void OnTextChange(string text)
        {
            base.OnTextChange(text);
            
            //set default value if text box is empty
            if (Text == null) Text = "1";

            double downFactor = System.Convert.ToDouble(Text);
            if (downFactor > 1 || downFactor < 0) Text = "1";

            Exporter.DownscaleFactor = System.Convert.ToDouble(Text);
        }

    }

    internal class MaxTextureDimensionEditBox : EditBox
    {
        MaxTextureDimensionEditBox()
        {
            Text = "4096";
        }

        protected override void OnTextChange(string text)
        {

            base.OnTextChange(text);
            //set default value if text box is empty
            if (Text == null) Text = "4096";

            double maxDimension = System.Convert.ToInt32(Text);
            if (maxDimension > 4096 || maxDimension < 0) Text = "4096";

            Exporter.MaxTextureDimension = System.Convert.ToInt32(Text);
        }

    }
}
