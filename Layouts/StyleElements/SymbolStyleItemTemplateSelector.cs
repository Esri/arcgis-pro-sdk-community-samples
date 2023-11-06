/*

   Copyright 2023 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ArcGIS.Desktop.Mapping;

namespace StyleElements {
  public class SymbolStyleItemTemplateSelector : DataTemplateSelector {


    #region The Templates

    public DataTemplate SymbolStyleItemTemplate { get; set; }
    public DataTemplate ScaleBarStyleItemTemplate { get; set; }

    public DataTemplate TableFrameStyleItemTemplate { get; set; }
    public DataTemplate LegendFrameStyleItemTemplate { get; set; }

    public DataTemplate GridStyleItemTemplate { get; set; }



    #endregion The Templates

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      var si = item as GeometrySymbolItem;
      if (si == null)
        return SymbolStyleItemTemplate;
      if (si.StyleItemType == StyleItemType.ScaleBar)
        return ScaleBarStyleItemTemplate;
      if (si.StyleItemType == StyleItemType.TableFrame)
        return TableFrameStyleItemTemplate;
      if (si.StyleItemType == StyleItemType.Legend ||
         si.StyleItemType == StyleItemType.LegendItem)
              return LegendFrameStyleItemTemplate;
      if (si.StyleItemType == StyleItemType.Grid)
        return GridStyleItemTemplate;
      return SymbolStyleItemTemplate;
        }
    }
}
